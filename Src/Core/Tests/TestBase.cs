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
//
using System;
using Core.Device;
using Core.Network;
using NUnit.Framework;


namespace Core.Tests
{

	public abstract class TestBase
	{
		protected IDeviceManager m_deviceManager;
		protected ITaskMonitor m_dummyTaskMonitor;
		protected IMessageLogger console;

		[TestFixtureSetUp]
		public virtual void Setup() {

			if (Log.Instance == null) {
			
				Console.WriteLine ("pappa");
				Log.Instantiate(Settings.Identifiers.Logger ());

			}

			console = new ConsoleLogger (Settings.Identifiers.ConsoleLogger());

			Log.Instance.AddLogger (new FileLogger("file_logger", "test_output.txt"));

			Log.Instance.AddLogger (console);

			var dummyServer = new DummyR2Server ();
			var dummyHostManager = new DummyHostManager (dummyServer);
			m_dummyTaskMonitor = new DummyTaskMonitor ();

			var packageFactory = new NetworkPackageFactory (null);
			var rpcManager = new RPCManager (dummyHostManager, packageFactory, m_dummyTaskMonitor);

			m_deviceManager = new DeviceManager (Settings.Identifiers.DeviceManager(), dummyHostManager, rpcManager, packageFactory);

			m_deviceManager.Add (Settings.Instance);
			m_deviceManager.Add (Log.Instance);

		}

		public TestBase ()
		{

		}
	}
}

