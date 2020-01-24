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
using R2Core.Network;
using System.Threading;
using R2Core.Device;
using System.Threading.Tasks;
using R2Core.Scripting;
using R2Core.Common;

namespace R2Core.Tests
{
	
	[TestFixture]
	public class DeviceTests: NetworkTests, IDeviceObserver {

		const int tcp_port = 4444;

		[Test]
		public void TestRemotePythonScript() {

			IServer s = factory.CreateTcpServer("s", tcp_port + 9998);
			s.Start();
			Thread.Sleep(500);
			DeviceRouter rec = (DeviceRouter) factory.CreateDeviceRouter(m_deviceManager);

			IWebEndpoint ep = factory.CreateJsonEndpoint(rec);
			s.AddEndpoint(ep);

			IScriptFactory<IronScript> m_pythonScriptFactory = new PythonScriptFactory("rf", BaseContainer.PythonPaths , m_deviceManager);
			m_pythonScriptFactory.AddSourcePath(Settings.Paths.TestData());
			m_pythonScriptFactory.AddSourcePath(Settings.Paths.Common());

			IScript localScript = m_pythonScriptFactory.CreateScript("python_test");

			m_deviceManager.Add(localScript);

			var client = factory.CreateTcpClient("c", "localhost", tcp_port + 9998);
			client.Start();

			//Client should be connected
			Assert.IsTrue(client.Ready);

			IClientConnection connection = new HostConnection("hc", client);

			dynamic python = new RemoteDevice("python_test", Guid.Empty, connection);

			Assert.AreEqual(142, python.add_42 (100));

			python.katt = 99;

			Assert.AreEqual(99, python.katt);

			Assert.AreEqual(99 * 10 , python.return_katt_times_10());

			python.dog_becomes_value("foo");

			Assert.AreEqual("foo", python.dog);

			s.Stop();
			client.Stop();
			Thread.Sleep(500);
		}

		[Test]
		public void TestRemoteDevice() {

			IServer s = factory.CreateTcpServer("s", tcp_port + 9990);
			s.Start();
			DummyDevice dummyObject = m_deviceManager.Get("dummy_device");
			dummyObject.Bar = "XYZ";
			DeviceRouter rec = (DeviceRouter) factory.CreateDeviceRouter(m_deviceManager);
			rec.AddDevice(dummyObject);
			IWebEndpoint ep = factory.CreateJsonEndpoint(rec);
			s.AddEndpoint(ep);
			Thread.Sleep(200);

			var client = factory.CreateTcpClient("c", "localhost", tcp_port + 9990);
			client.Start();

			Thread.Sleep(200);
			//Client should be connected
			Assert.IsTrue(client.Ready);

			IClientConnection connection = new HostConnection("hc", client);

			RemoteDevice remoteDummy = new RemoteDevice("dummy_device", Guid.Empty, connection);

			// Test method with result
			Task invokeTask = remoteDummy.Async((result, ex) => { 
				
				Assert.IsNull(ex);
				Assert.AreEqual(100, result);

			}).MultiplyByTen(10);

			Thread.Sleep(200);
			invokeTask.Wait();

			// Test invoking non-returning method
			Task invokeTask2 = remoteDummy.Async((result, ex) => { 

				Assert.IsNull(ex);

			}).NoParamsNoNothing();

			invokeTask2.Wait();

			// Test get property:
			Task retrieveTask = remoteDummy.Async((response, ex) => {
				Assert.IsNull(ex);
				Assert.AreEqual(dummyObject.Bar, response);
			}).Bar;

			retrieveTask.Wait();

			// Test set property
			remoteDummy.Async((response, ex) => {
				Assert.Equals(1111, response);
				Assert.IsNull(ex);

			}).HAHA = 1111;

			Thread.Sleep(100);

			Assert.AreEqual(dummyObject.HAHA, 1111);

			Task failTask = remoteDummy.Async((response, ex) => {

				Assert.NotNull(ex);

			}).ThisMethodDoesNotExist();
		
			failTask.Wait();

			s.Stop();
			client.Stop();
			Thread.Sleep(500);
		}


		bool wasInvoked = false;

		[Test]
		public void X_DeviceNotificationTest() {

			InvokerDummyDevice invokedDevice = new InvokerDummyDevice("x");
			m_deviceManager = new DeviceManager("dm");
			m_deviceManager.Add(invokedDevice);

			invokedDevice.AddObserver(this);
			invokedDevice.SomeMethod();
			Thread.Sleep(200);
			Assert.IsTrue(wasInvoked);

			Assert.AreEqual(invokedDevice.SomeValue, 43);
			invokedDevice.RemoveObserver(this);
			wasInvoked = false;
			invokedDevice.SomeMethod(44);
			Thread.Sleep(200);
			Assert.False(wasInvoked);
			Assert.AreEqual(invokedDevice.SomeValue, 44);

		}

		public void OnValueChanged(IDeviceNotification<object> notification) {

			InvokerDummyDevice device = m_deviceManager.Get(notification.Identifier);

			Assert.NotNull(device);
			Assert.AreEqual(notification.NewValue, 42);
			Assert.AreEqual(notification.NewValue, device.SomeValue);
			Assert.AreEqual(notification.Action, "SomeMethod");

			device.SomeValue = 43;
			wasInvoked = true;

		}


	}

}

