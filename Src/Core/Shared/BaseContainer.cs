// This file is part of r2Poject.
//
// Copyright 2016 Tord Wessman
// 
// r2Project is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// r2Project is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with r2Project. If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using Core.Device;
using Core.Network;
using Core.Network.Data;
using System.Net;
using Core.DB;
using Core.Memory;
using System.Threading.Tasks;
using System.Collections.Generic;
using Core.Scripting;
using System.IO;
using System.Linq;
using Core.Network.Web;

namespace Core
{

	/// <summary>
	/// The BaseContainer ccontains some functionality shared by all 
	/// projects
	/// </summary>
	public class BaseContainer : DeviceBase
	{

		private IBasicServer<IPEndPoint> m_server;
		private IRPCManager<IPEndPoint> m_rpcManager;
		private IHostManager<IPEndPoint> m_hostManager;
		private NetworkPackageFactory m_networkPackageFactory;
		private IDeviceManager m_devices;
		private ITaskMonitor m_taskMonitor;
		private IMemorySource m_memory;
		private DeviceFactory m_deviceFactory;
		
		private IDatabase m_db;
		private IScriptFactory m_scriptFactory;
		private IScript m_runLoopScript;
		private IRunLoop m_runLoop;
		
		private Serializer m_serializer;
		
		private bool m_shouldRun;

		public static readonly ICollection<string> DEFAULT_RUBY_PATHS = new List<string> () {
			Settings.Paths.Ruby(),
			Settings.Paths.Common()
		};

		public bool IsRunning {
			get {
				return m_server.Ready &&
						m_hostManager.IsRunning;
			}
		}
		
		public IHostManager<IPEndPoint> HostManager {
			get {
				return m_hostManager;
			}
		}
		
		public IDeviceManager DeviceManager {
			get {
				return m_devices;
			}
		}
		
		public ITaskMonitor TaskMonitor {
			get {
				return m_taskMonitor;
			}
		}
		
		public IBasicServer<IPEndPoint> Server { get { return m_server; } }
		
		public IDatabase DB {
			get {
				return m_db;
			}
		}
		                     
		
		public BaseContainer (string dbFile, int tcpPort = -1) : base (Settings.Identifiers.Core())
		{

			m_shouldRun = true;

			//Set up logging
			SimpleConsoleLogger consoleLogger = new SimpleConsoleLogger (Settings.Identifiers.ConsoleLogger ());
			consoleLogger.Start ();
			Log logger = new Log (Settings.Identifiers.Logger ());
			logger.AddLogger (consoleLogger);

			//Check if the file is in an absoulute path. If not create/open the database in the database-base folder.
			string dbPath = dbFile.Contains(Path.DirectorySeparatorChar) ? 
				dbFile : Settings.Paths.Databases() + Path.DirectorySeparatorChar + dbFile;

			m_server = new Server (Settings.Identifiers.Server(), tcpPort == -1 ? Settings.Consts.DefaultRpcPort() : tcpPort);
			m_taskMonitor = new SimpleTaskMonitor (Settings.Identifiers.TaskMonitor());

			m_serializer = new Serializer (Settings.Identifiers.Serializer());
		
			// Set up a very simple network security handler
			INetworkSecurity simpleSecurity = new SimpleNetworkSecurity ("base_security", Settings.Consts.DefaultPassword());

			m_networkPackageFactory = new NetworkPackageFactory (simpleSecurity);

			m_hostManager = new HostManager (
				Settings.Identifiers.HostManager(), 
				m_networkPackageFactory, 
				m_server, 
				Settings.Consts.BroadcastMgrPort(), 
				Settings.Consts.BroadcastMgrTcpPort());

			// handles remote device requests
			m_rpcManager = new RPCManager (m_hostManager, m_networkPackageFactory, m_taskMonitor);

			// contains and manages all devices
			m_devices = new DeviceManager (Settings.Identifiers.DeviceManager (), m_hostManager, m_rpcManager, m_networkPackageFactory);

			m_devices.Add (simpleSecurity);
			m_devices.Add (m_taskMonitor);
			m_devices.Add (Settings.Instance);
			m_devices.Add (consoleLogger);
			m_devices.Add (logger);
			m_devices.Add (this);

			m_scriptFactory = new RubyScriptFactory (
				Settings.Identifiers.ScriptFactory(),
				Settings.Paths.Ruby(),
			    DEFAULT_RUBY_PATHS,
			    m_devices,
			    m_taskMonitor);
			
			m_runLoopScript = m_scriptFactory.CreateScript (
				Settings.Identifiers.RunLoopScriptId() + "_script" ,
				Settings.Paths.Common(Settings.Consts.RunLoopScript()));

			m_runLoop = new InterpreterRunLoop (Settings.Identifiers.RunLoopScriptId (), m_runLoopScript);

			// Set up database and memory
			m_db = new SqliteDatabase (Settings.Identifiers.Database(), dbPath);
			m_memory = new SharedMemorySource (Settings.Identifiers.Memory(), m_devices, m_db);

			// Creating a device factory used for the creation of yet uncategorized devices...
			m_deviceFactory = new DeviceFactory (Settings.Identifiers.DeviceFactory(), m_devices, m_memory);

			// Creating a web factory used to create http/websocket related endpoints etc.
			WebFactory httpFactory = m_deviceFactory.CreateWebFactory (Settings.Identifiers.WebFactory());

			// Add devices to device manager
			m_devices.Add (m_runLoopScript);
			m_devices.Add (m_runLoop);
			m_devices.Add (httpFactory);
			m_devices.Add (m_serializer);
			m_devices.Add (m_memory);
			m_devices.Add (m_db);
			m_devices.Add (m_server);
			m_devices.Add (m_hostManager);
			m_devices.Add (m_scriptFactory);
			m_devices.Add (m_deviceFactory);

			m_taskMonitor.AddMonitorable (m_server);
			m_taskMonitor.AddMonitorable (m_hostManager);
	
		}
		
		public void AddScript (IScriptProcess script)
		{
			m_devices.Add (script);
			m_taskMonitor.AddMonitorable (script);
		}
		
		public void RemoveScript (IScriptProcess script)
		{

			if (m_devices.Has (script.Identifier)) {
			
				m_devices.Remove (script.Identifier);
				m_taskMonitor.RemoveMonitorable (script);
			
			}
		
		}
		
		public void RunLoop ()
		{

			m_runLoop.Start ();

		}
		
		public override void Start ()
		{

			m_taskMonitor.Start ();
			
			m_db.Start ();
			m_memory.Start();
			m_server.Start ();

			StartWhenReady (m_hostManager, Settings.Identifiers.Server());

			StartWhenReady (() => {

				m_hostManager.Broadcast ();

			}, Settings.Identifiers.HostManager());

			Log.d ("BaseContainer initialized");

		}
		
		public void StartWhenReady (Action action, params string[] deviceIds)
		{
			string name = "";
			foreach (string devid in deviceIds) {
				name += " " + devid;
			}
               
			if (deviceIds != null && deviceIds.Length > 0) {

				m_taskMonitor.AddTask ("StartWhenReady" + name,
				
					Task.Factory.StartNew (() => {
					IList<string> devs = new List<string> (deviceIds);

					while (devs.Count > 0 && m_shouldRun) {

						string dev = devs [0];
					
							if (m_devices.Has (dev) && m_devices.Get (dev).Ready) {

					
								devs.Remove (dev);
						
							} else {
							
								Log.d ("--- WAITING FOR " + dev + " TO CONNECT ---");
								Rest (1000);
						
							}

					}
					
					if (m_shouldRun) {

						action.Invoke ();
						return;

					} else {

						Log.t ("Death to me: " + name);
						return;
					
						}
				}

				));

			} else {

				action.Invoke ();
			
			}
				
		}
		
		public void StartWhenReady (IStartable startable, params string[] deviceIds)
		{

			StartWhenReady (startable.Start, deviceIds);
		
		}
		
		public override void Stop ()
		{

			Log.d ("Stopping base container.");
			m_devices.Stop (new IDevice[]{this});

			m_shouldRun = false;
		
		}
		
		public T GetDevice<T> (string deviceId)
		{
			return m_devices.Get<T> (deviceId);
		}
		
		public void AddDevice (IDevice device)
		{
			m_devices.Add (device);
		}
		
		public static void Rest (int milliseconds)
		{

			new System.Threading.ManualResetEvent (false).WaitOne (milliseconds);

		}
	}
}

