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

			IScriptFactory<IronScript> m_pythonScriptFactory = new PythonScriptFactory("rf", Settings.Instance.GetPythonPaths(), m_deviceManager);
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
		public void TestRemoteDevice_UsingServers() {

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

            //Remote dummy should be ready after Start()
            Assert.False(remoteDummy.Ready);
            dummyObject.Start();
            Assert.True(remoteDummy.Ready);

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
				Assert.AreEqual(true, response);
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

        [Test]
        public void TestRemoteDevice() {

            var host = new DummyNetworkConnection();
            RemoteDevice remoteDevice = new RemoteDevice("test", Guid.Empty, host);

            // Next response will return a string
            host.NextResponse = new NetworkMessage {

                Payload = new DeviceResponse() {
                    ActionResponse = "hund"
                },
                Code = NetworkStatusCode.Ok.Raw()
            };

            // Cast to dynamic
            dynamic remoteDeviceDynamic = remoteDevice;

            // -- Test synchronous request
            Assert.AreEqual("hund", remoteDeviceDynamic.hund);

            string hundValue = null;

            // -- Test asynchronous
            remoteDevice.Async((response, exception) => {

                Assert.IsNull(exception);
                hundValue = response;

            }).din_mamma();

            Thread.Sleep(50);
            Assert.AreEqual("hund", hundValue);

            // Call a fake method 10 times.
            int receiveCount = 0;
            host.Delay = 10;
            for (int i = 0; i < 10; i++) {

                remoteDevice.Async((response, exception) => {

                    Assert.IsNull(exception);
                    Assert.AreEqual("hund", response);
                    receiveCount++;

                }).din_mamma();

            }

            Thread.Sleep(100); // They should be done by now... (but maybe not)

            Assert.AreEqual(10, receiveCount); // .. and all 3 should have been executed.


            // Shoul lose requests before the last one.
            remoteDevice.LossyRequests = true;
            receiveCount = 0;
            host.Delay = 50; // Add a small delay

            // Call a fake method 50 times. If running this test continously, it will start failing.
            for (int i = 0; i < 50; i++) {
              
                remoteDevice.Async((response, exception) => {

                    Log.t($" -- {i}");
                    Assert.IsNull(exception);
                    receiveCount++;

                }).din_mamma();

            }

            Thread.Sleep(200); // They should be done by now... (but maybe not)
            Assert.AreEqual(2, receiveCount); // .. only 2 should have been executed

            // Test GetValue

            remoteDevice.Async((response, exception) => {

                Assert.IsNull(exception);
                Assert.AreEqual("hund", response);

            }).GetValue("pappa");

            // Test error:

            host.NextResponse = new NetworkMessage {

                Payload = "Argh!",
                Code = NetworkStatusCode.BadRequest.Raw()

            };

            remoteDevice.Async((response, exception) => {

                Assert.NotNull(exception);
                Assert.True(exception.Message.Contains("Argh!"));

            });

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

