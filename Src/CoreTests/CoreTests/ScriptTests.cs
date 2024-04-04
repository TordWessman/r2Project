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
using NUnit.Framework;
using R2Core.Device;
using R2Core.Network;
using R2Core.Scripting;
using R2Core.Common;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace R2Core.Tests
{
	
	[TestFixture]
	public class ScriptTests: TestBase {
		
		private IScriptFactory<IronScript> m_pythonScriptFactory;
		private IScriptFactory<LuaScript> m_luaScriptFactory;

		[SetUp]
		public override void Setup() {

			base.Setup();

			m_pythonScriptFactory = CreatePythonScriptFactory(m_deviceManager);
            m_pythonScriptFactory.AddSourcePath(Settings.Paths.TestData());
            m_luaScriptFactory = new LuaScriptFactory(Settings.Identifiers.LuaScriptFactory());
			m_luaScriptFactory.AddSourcePath(Settings.Paths.TestData());

		}

		private IScriptFactory<IronScript> CreatePythonScriptFactory(IDeviceManager dm) {

			IScriptFactory<IronScript> sf = new PythonScriptFactory(Settings.Identifiers.PythonScriptFactory(), Settings.Instance.GetPythonPaths(), dm);
			sf.AddSourcePath(Settings.Paths.TestData());
			sf.AddSourcePath(Settings.Paths.Common());
			return sf;

		}

		[Test]
		public void LuaTest1() {
			PrintName();

			dynamic lua = m_luaScriptFactory.CreateScript("LuaTest1");
			Assert.Equals("fish", lua.str);

		}

        [Test]
		public void PythonTests() {
			PrintName();

			dynamic python = m_pythonScriptFactory.CreateScript("python_test");

			Assert.Equals(142, python.add_42 (100));

			python.katt = 99;

			Assert.Equals(99, python.katt);

			Assert.Equals(99 * 10 , python.return_katt_times_10());

			python.dog_becomes_value("foo");

			Assert.Equals("foo", python.dog);
		
		}

        [Test]
		public void PythonRemoteScriptTests() {
			PrintName();

			dynamic python = m_pythonScriptFactory.CreateScript("python_test");

            m_deviceManager.Add(python);
			var factory = new WebFactory("wf", new JsonSerialization("ser"));
			var router = factory.CreateDeviceRouter(m_deviceManager);
			var endpont = factory.CreateJsonEndpoint(router);
			var server = factory.CreateTcpServer(Settings.Identifiers.TcpServer(), 11111);

			server.AddEndpoint(endpont);
			server.Start();
            server.WaitFor();

			var client = factory.CreateTcpClient("client", "localhost", 11111);
			client.Start();
            client.WaitFor();

			var host = new HostConnection("hc", client);
			dynamic remoteScript = new RemoteDevice("python_test", Guid.Empty, host);
			remoteScript.katt = 10;

			Assert.Equals(2, remoteScript.wait_and_return_value_plus_value(1));
			Assert.Equals(100, remoteScript.return_katt_times_10());

			dynamic device_list = m_pythonScriptFactory.CreateScript("device_list");
			m_deviceManager.Add(device_list);

			// Test device_list script:
			DummyDevice dummy = new DummyDevice("dummy");
			m_deviceManager.Add(dummy);
			dummy.Bar = "Foo";

            LuaScript lua = new LuaScript("lua", Settings.Paths.TestData("LuaTest1.lua"));
            m_deviceManager.Add(lua);

            HttpClient testClient = new HttpClient("client", new JsonSerialization("serial"));
            m_deviceManager.Add(testClient);
            Thread.Sleep(200);

            // Manually create a remote device pointing to our "local" device_list
			dynamic remoteDeviceList = new RemoteDevice("device_list", Guid.Empty, host);

            // Create a list of devices. `device_list.py` should only return devices that exists.
			IEnumerable<string> deviceNames = new List<string>(){ "python_test", "dummy", "lua", "non-existing", "client" };
			Thread.Sleep(200);
			IEnumerable<dynamic> devices = remoteDeviceList.GetDevices(deviceNames);

			Assert.Equals(4, devices.Count());
			dynamic lastDevice = devices.Last();
			// Hmmm.. this used to work... Apparently the properties of DummyDevice is no longer serialized 
            // Assert.Equals("Foo", lastDevice.Bar);

			Thread.Sleep(200);
			client.Stop();
			server.Stop();

		}


		[Test]
		public void PythonInterpreterScriptTests() {
			PrintName();

			if (m_deviceManager.Has("python_test")) { m_deviceManager.Remove("python_test"); } 

			// Required for script to dynamically invoke devices
			m_deviceManager.Add(new ObjectInvoker());

			// Imitate the run loop script
			var dummyRunloop = new DummyDevice(Settings.Identifiers.RunLoop());
			dummyRunloop.Start();
			m_deviceManager.Add(dummyRunloop);

			// Used by interpreter to create scripts
			m_deviceManager.Add(m_pythonScriptFactory);

			// Load the script that will perform the interpretation
			dynamic interpreterScript = m_pythonScriptFactory.CreateScript(Settings.Identifiers.RunLoopScript());
			IScriptInterpreter interpreter = new ScriptInterpreter(interpreterScript);

			// Asesrt some stuff
			Assert.That(interpreter.Interpret("blah") == false);
			Assert.That(dummyRunloop.Ready);
			Assert.That(interpreter.Interpret("exit"));
			Assert.That(dummyRunloop.Ready == false);
			Assert.That(interpreter.Interpret("devices"));

			// Add the script that will be interpreted
			Assert.That(interpreter.Interpret("load python_test"));
			// Make sure it can be invoked
			Assert.That(interpreter.Interpret("python_test.add_42(42)"));

		}

		[Test]
		public void PythonInterpreterRemoteScriptTests() {
			PrintName();

			if (m_deviceManager.Has("python_test")) { m_deviceManager.Remove("python_test"); } 

			// Required for script to dynamically invoke devices
			m_deviceManager.Add(new ObjectInvoker());

			// Used by interpreter to create scripts
			m_deviceManager.Add(m_pythonScriptFactory);

			// Load the script that will perform the interpretation
			dynamic interpreterScript = m_pythonScriptFactory.CreateScript(Settings.Identifiers.RunLoopScript());
			IScriptInterpreter interpreter = new ScriptInterpreter(interpreterScript);

			// Set up the mocked endpoint for the remote script
			DeviceRouter deviceRouter = new DeviceRouter(m_deviceManager);
			ClientConnectionWebEndpoint epDummy = new ClientConnectionWebEndpoint(deviceRouter);

			// script & dummy will be invoked "remotely". Add only to deviceRouter, since the remote version will have the same Identifier and has to be added to the DeviceManager.
			var script = m_pythonScriptFactory.CreateScript("python_test");
			var dummy = new DummyDevice("dummy");
			deviceRouter.AddDevice(script);
			deviceRouter.AddDevice(dummy);

			RemoteDevice remoteScript = new RemoteDevice("python_test", Guid.NewGuid(), epDummy);
			m_deviceManager.Add(remoteScript);
			RemoteDevice remoteDummy = new RemoteDevice("dummy", Guid.NewGuid(), epDummy);
			m_deviceManager.Add(remoteDummy);

			// Make RemoteDevices dynamically callable
			dynamic remoteScriptDynamic = (dynamic)remoteScript;
			dynamic remoteDummyDynamic = (dynamic)remoteDummy;

			// Test the remote script
			Assert.Equals(84, remoteScriptDynamic.add_42(42));
			Assert.That(interpreter.Interpret("python_test.add_42(42)"));
			Assert.That(interpreter.Interpret("python_test.katt = 99"));
			Assert.That(interpreter.Interpret("python_test.katt"));
			Assert.That(interpreter.Interpret("python_test.Start()"));
			Assert.That(interpreter.Interpret("python_test.return_katt_times_10()"));
			Assert.That(interpreter.Interpret("python_test.CamelCaseMethod()"));

			//Test the dummy device
			Assert.Equals(100, remoteDummyDynamic.MultiplyByTen(10));
			Assert.That(interpreter.Interpret("dummy.Start()"));
			Assert.That(interpreter.Interpret("dummy.MultiplyByTen (10)"));
			Assert.That(interpreter.Interpret("dummy.Bar = \"Foo\""));
			Assert.That(interpreter.Interpret("dummy.Value"));
		}

	
	}
}

