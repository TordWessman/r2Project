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
using R2Core.Device;
using NUnit.Framework;
using System.Diagnostics;
using R2Core.Common;
using System.Threading;

namespace R2Core.Tests
{

	public abstract class TestBase {
		
		protected IDeviceManager m_deviceManager;
		protected ITaskMonitor m_dummyTaskMonitor;
		protected IMessageLogger console;
		protected DeviceFactory m_deviceFactory;
		protected DataFactory m_dataFactory;

		[TestFixtureTearDown]
		public virtual void Teardown() {

			Thread.Sleep(200);

			Log.d($"--- TEST `{TestContext.CurrentContext.Test.FullName }` FINISHED ---");
			Log.d($"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}");

		}

		[TestFixtureSetUp]
		public virtual void Setup() {
			
			try {
				
				if (Log.Instance == null) {
					
					Log.Instantiate(Settings.Identifiers.Logger());

					console = new ConsoleLogger(Settings.Identifiers.ConsoleLogger());

					Log.Instance.AddLogger(new FileLogger(Settings.Identifiers.FileLogger(), "test_output.txt"));

					Log.Instance.AddLogger(console);
					Log.Instance.MaxStackTrace = 100;
                    Log.Instance.LogLevel = LogLevel.Info;

                }

				m_dummyTaskMonitor = new DummyTaskMonitor();

				m_deviceManager = new DeviceManager(Settings.Identifiers.DeviceManager());
                m_dataFactory = new DataFactory(Settings.Identifiers.DataFactory(), new System.Collections.Generic.List<string> { Settings.Paths.TestData() });
                m_deviceFactory = new DeviceFactory(Settings.Identifiers.DeviceFactory(), m_deviceManager);

                m_deviceManager.Add(Settings.Instance);
				m_deviceManager.Add(Log.Instance);
				m_deviceManager.Add(m_dummyTaskMonitor);
                m_deviceManager.Add(m_dataFactory);
                m_deviceManager.Add(m_deviceFactory);
				
			} catch (Exception ex) {
			
				Console.WriteLine(ex.Message);

			}

			Log.d($"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine} --- TEST `{TestContext.CurrentContext.Test.FullName}` STARTED ---");

		}

		/// <summary>
		/// Print the name of the current method
		/// </summary>
		protected void PrintName() {

			StackTrace stackTrace = new StackTrace();

			Log.d($"########################## {stackTrace.GetFrames()[1].GetMethod().Name} ##########################");
		
        }

	}

}