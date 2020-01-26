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
using R2Core.Device;
using R2Core.Network;
using System.Net;
using R2Core.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using R2Core.Scripting;
using System.IO;
using System.Linq;
using R2Core.DataManagement.Memory;
using R2Core.DataManagement;


namespace R2Core.Common
{

	/// <summary>
	/// The BaseContainer ccontains some functionality shared by all 
	/// projects
	/// </summary>
	public class BaseContainer : DeviceBase {

		private IDeviceManager m_devices;
		private ITaskMonitor m_taskMonitor;
		private IMemorySource m_memory;
		private DeviceFactory m_deviceFactory;
		
		private IDatabase m_db;
		private IRunLoop m_runLoop;

		public ITaskMonitor TaskMonitor { get { return m_taskMonitor; } }

		public static IList<string> PythonPaths {

			get {
				var pythonPaths = Settings.Consts.PythonPaths().Split(new char[] { ';' }).ToList();
				pythonPaths.Add(Settings.Paths.Common());
				pythonPaths.Add(Settings.Paths.Python());
				return pythonPaths;
			}
		}

		public IDeviceManager GetDeviceManager() { return m_devices; }

		public BaseContainer(string dbFile, int tcpPort = -1) : base(Settings.Identifiers.Core()) {

			m_taskMonitor = new SimpleTaskMonitor(Settings.Identifiers.TaskMonitor());

			//Set up logging
			SimpleConsoleLogger consoleLogger = new SimpleConsoleLogger(Settings.Identifiers.ConsoleLogger(), Settings.Consts.MaxConsoleHistory());
			consoleLogger.Start();
			Log.Instantiate(Settings.Identifiers.Logger());

			Log.Instance.AddLogger(consoleLogger);
			Log.Instance.AddLogger(new FileLogger(Settings.Identifiers.FileLogger(), Settings.Consts.FileLoggerDefaultFile()));

			// contains and manages all devices
			m_devices = new DeviceManager(Settings.Identifiers.DeviceManager());

			// Creating a device factory used for the creation of yet uncategorized devices...
			m_deviceFactory = new DeviceFactory(Settings.Identifiers.DeviceFactory(), m_devices);

			m_devices.Add(m_taskMonitor);
			m_devices.Add(Settings.Instance);
			m_devices.Add(consoleLogger);
			m_devices.Add(Log.Instance);
			m_devices.Add(this);
			m_devices.Add(new ObjectInvoker());

			var psf = new PythonScriptFactory(Settings.Identifiers.PythonScriptFactory(), PythonPaths, m_devices);

			// Point to the defauult ruby script files resides.
			psf.AddSourcePath(Settings.Paths.Python());
			// Point to the common folder.
			psf.AddSourcePath(Settings.Paths.Common());

			m_devices.Add(psf);

			// The run loop script must meet the method requirements of the InterpreterRunLoop.
			IronScript runLoopScript = psf.CreateScript(Settings.Identifiers.RunLoopScript());

			// Create the interpreter used to evaluate exrpessions in the run loop script
			IScriptInterpreter runLoopInterpreter = psf.CreateInterpreter(runLoopScript);

			// Create the run loop. Use the IScript declared above to interpret commands and the consoleLogger for output.
			m_runLoop = new InterpreterRunLoop(Settings.Identifiers.RunLoop(), runLoopInterpreter, consoleLogger);
			var dataFactory = m_deviceFactory.CreateDataFactory(Settings.Identifiers.DataFactory(), new List<string>() {Settings.Paths.Databases()});

			// Set up database and memory
			m_db = new SqliteDatabase(Settings.Identifiers.Database(), dbFile);
			m_memory = new SharedMemorySource(Settings.Identifiers.Memory(), m_devices, m_db);

			var serializer = new JsonSerialization(Settings.Identifiers.Serializer(), System.Text.Encoding.UTF8);

			// Creating a web factory used to create http/websocket related endpoints etc.
			WebFactory httpFactory = m_deviceFactory.CreateWebFactory(Settings.Identifiers.WebFactory(), serializer);

			// Add devices to device manager
			m_devices.Add(runLoopScript);
			m_devices.Add(m_runLoop);
			m_devices.Add(httpFactory);
			m_devices.Add(dataFactory);
			m_devices.Add(m_memory);
			m_devices.Add(m_db);
			m_devices.Add(m_deviceFactory);
			m_taskMonitor.AddMonitorable(runLoopScript);

		}

		public void RemoveScript(IScript script) {

			if (m_devices.Has(script.Identifier)) {
			
				m_devices.Remove(script.Identifier);
				m_taskMonitor.RemoveMonitorable(script);
			
			}
		
		}

		public void RunLoop() {
		
			m_runLoop.Start();

		}

		public override void Start() {

			m_taskMonitor.Start();
			
			m_db.Start();
			m_memory.Start();
		
			Log.d("BaseContainer initialized");

		}

		public override void Stop() {

			Log.d("Stopping base container.");
			m_devices.Stop(new IDevice[]{this});

		}

		public static void Rest(int milliseconds) {

			new System.Threading.ManualResetEvent(false).WaitOne(milliseconds);

		}
	}
}

