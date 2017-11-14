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
using Core.Memory;
using Core.Data;

namespace Core.Tests
{

	public abstract class TestBase
	{
		protected IDeviceManager m_deviceManager;
		protected ITaskMonitor m_dummyTaskMonitor;
		protected IMessageLogger console;
		protected DeviceFactory m_deviceFactory;
		protected DataFactory m_dataFactory;

		[TestFixtureSetUp]
		public virtual void Setup() {

			Console.WriteLine ("pappa");

			try {
				
				if (Log.Instance == null) {


					Log.Instantiate(Settings.Identifiers.Logger ());

					console = new ConsoleLogger (Settings.Identifiers.ConsoleLogger());

					Log.Instance.AddLogger (new FileLogger("file_logger", "test_output.txt"));

					Log.Instance.AddLogger (console);
				}

				m_dummyTaskMonitor = new DummyTaskMonitor ();

				m_deviceManager = new DeviceManager (Settings.Identifiers.DeviceManager());

				m_deviceManager.Add (Settings.Instance);
				m_deviceManager.Add (Log.Instance);

				m_deviceFactory = new DeviceFactory ("device_factory", m_deviceManager, new TemporaryMemorySource("ms"));

				m_dataFactory = new DataFactory("f", new System.Collections.Generic.List<string>() {Settings.Paths.TestData()});

			} catch (Exception ex) {
			
				Console.WriteLine (ex.Message);

			}


		}

		public TestBase ()
		{

		}
	}
}

