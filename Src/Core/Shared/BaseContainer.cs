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
using Core.Data;
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
		private IScriptFactory<IronScript> m_scriptFactory;
		private IRunLoop m_runLoop;

		private bool m_shouldRun;

		/// <summary>
		/// Search paths for the script engine.
		/// </summary>
		public static readonly ICollection<string> DEFAULT_RUBY_PATHS = new List<string> () {
			Settings.Paths.Ruby(),
			Settings.Paths.RubyLib(),
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

			m_taskMonitor = new SimpleTaskMonitor (Settings.Identifiers.TaskMonitor());

			// Set up a very simple network security handler
			INetworkSecurity simpleSecurity = new SimpleNetworkSecurity ("base_security", Settings.Consts.DefaultPassword());

			m_networkPackageFactory = new NetworkPackageFactory (simpleSecurity);

			m_shouldRun = true;

			//Set up logging
			SimpleConsoleLogger consoleLogger = new SimpleConsoleLogger (Settings.Identifiers.ConsoleLogger (), Settings.Consts.MaxConsoleHistory());
			consoleLogger.Start ();
			Log.Instantiate(Settings.Identifiers.Logger ());

			Log.Instance.AddLogger (consoleLogger);
			Log.Instance.AddLogger (new FileLogger("file_logger", "test_output.txt"));

			m_server = new Server (Settings.Identifiers.Server(), tcpPort == -1 ? Settings.Consts.DefaultRpcPort() : tcpPort);

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

			// Creating a device factory used for the creation of yet uncategorized devices...
			m_deviceFactory = new DeviceFactory (Settings.Identifiers.DeviceFactory(), m_devices, m_memory);

			m_devices.Add (simpleSecurity);
			m_devices.Add (m_taskMonitor);
			m_devices.Add (Settings.Instance);
			m_devices.Add (consoleLogger);
			m_devices.Add (Log.Instance);
			m_devices.Add (this);

			var psf = new PythonScriptFactory(Settings.Identifiers.PSF(), new List<string> () {Settings.Paths.PythonLib(), Settings.Paths.Common ()}, m_devices);

			// Point to the defauult ruby script files resides.
			psf.AddSourcePath (Settings.Paths.Python());
			// Point to the common folder.
			psf.AddSourcePath (Settings.Paths.Common ());

			m_devices.Add (psf);

			m_scriptFactory = new RubyScriptFactory (
				Settings.Identifiers.ScriptFactory(),
			    DEFAULT_RUBY_PATHS,
			    m_devices);

			// Point to the defauult ruby script files resides.
			m_scriptFactory.AddSourcePath (Settings.Paths.Ruby ());

			// Point to the common folder.
			m_scriptFactory.AddSourcePath (Settings.Paths.Common ());

			// Python modules
			m_scriptFactory.AddSourcePath ("/usr/lib/python2.7/dist-packages");
			m_scriptFactory.AddSourcePath ("/usr/lib/python2.7/Lib");

			// The run loop script must meet the method requirements of the InterpreterRunLoop.
			IronScript runLoopScript = m_scriptFactory.CreateScript (Settings.Identifiers.RunLoopScript());

			IScriptInterpreter runLoopInterpreter = m_scriptFactory.CreateInterpreter (runLoopScript);

			// Create the run loop. Use the IScript declared above to interpret commands and the consoleLogger for output.
			m_runLoop = new InterpreterRunLoop (Settings.Identifiers.RunLoop (), runLoopInterpreter, consoleLogger);
			var dataFactory = m_deviceFactory.CreateDataFactory ("data_factory", new List<string> () {Settings.Paths.Databases()});
			var serializer = dataFactory.CreateSerialization ("data_serializer", System.Text.Encoding.UTF8);

			// Set up database and memory
			m_db = new SqliteDatabase (Settings.Identifiers.Database(), dbFile);
			m_memory = new SharedMemorySource (Settings.Identifiers.Memory(), m_devices, m_db);

			// Creating a web factory used to create http/websocket related endpoints etc.
			WebFactory httpFactory = m_deviceFactory.CreateWebFactory (Settings.Identifiers.WebFactory(), serializer);

			// Add devices to device manager
			m_devices.Add (runLoopScript);
			m_devices.Add (m_runLoop);
			m_devices.Add (httpFactory);
			m_devices.Add (dataFactory);
			m_devices.Add (m_memory);
			m_devices.Add (m_db);
			m_devices.Add (m_server);
			m_devices.Add (m_hostManager);
			m_devices.Add (m_scriptFactory);
			m_devices.Add (m_deviceFactory);

			m_taskMonitor.AddMonitorable (m_server);
			m_taskMonitor.AddMonitorable (m_hostManager);
			m_taskMonitor.AddMonitorable (runLoopScript);
	
		}

		public void RemoveScript (IScript script)
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

