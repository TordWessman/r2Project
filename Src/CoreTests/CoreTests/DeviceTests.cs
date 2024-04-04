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
using System.Linq;
using System.Collections.Generic;

namespace R2Core.Tests
{


    /// <summary>
    /// Only to test enum transcriptions.
    /// </summary>
    class DummyEnumClass : DeviceBase {

        public DummyEnumClass() : base("dummy_enum") { }

        public LogLevel LogLevel = LogLevel.Info;

        public void SetLogLevel(LogLevel logLevel) { LogLevel = logLevel; }

    }

    [TestFixture]
	public class DeviceTests: NetworkTests, IDeviceObserver {

		const int tcp_port = 4444;

		[Test]
		public void TestRemotePythonScript() {
            PrintName();

            IServer s = factory.CreateTcpServer("s", tcp_port + 9998);
			s.Start();
            s.WaitFor();
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
			Assert.That(client.Ready);

			IClientConnection connection = new HostConnection("hc", client);

			dynamic python = new RemoteDevice("python_test", Guid.Empty, connection);

			Assert.Equals(142, python.add_42 (100));

			python.katt = 99;

			Assert.Equals(99, python.katt);

			Assert.Equals(99 * 10 , python.return_katt_times_10());

			python.dog_becomes_value("foo");

			Assert.Equals("foo", python.dog);

			s.Stop();
			client.Stop();
			Thread.Sleep(500);
		}

        [Test]
		public void TestRemoteDevice_UsingServers() {
            PrintName();

            IServer s = factory.CreateTcpServer("s", tcp_port + 9990);
			s.Start();
			DummyDevice dummyObject = m_deviceManager.Get("dummy_device");
			dummyObject.Bar = "XYZ";
			DeviceRouter rec = (DeviceRouter) factory.CreateDeviceRouter(m_deviceManager);
			rec.AddDevice(dummyObject);
			IWebEndpoint ep = factory.CreateJsonEndpoint(rec);
			s.AddEndpoint(ep);
            s.WaitFor();

            var client = factory.CreateTcpClient("c", "localhost", tcp_port + 9990);
			client.Start();

            client.WaitFor();

            //Client should be connected
            Assert.That(client.Ready);

			IClientConnection connection = new HostConnection("hc", client);

			RemoteDevice remoteDummy = new RemoteDevice("dummy_device", Guid.Empty, connection);

            //Remote dummy should be ready after Start()
            Assert.That(false == remoteDummy.Ready);
            dummyObject.Start();
            Assert.That(remoteDummy.Ready);

            // Test method with result
            Task invokeTask = remoteDummy.Async((result, ex) => { 
				
				Assert.That(null == ex);
				Assert.Equals(100, result);

			}).MultiplyByTen(10);

			Thread.Sleep(200);
			invokeTask.Wait();

			// Test invoking non-returning method
			Task invokeTask2 = remoteDummy.Async((result, ex) => { 

				Assert.That(null == ex);

			}).NoParamsNoNothing();

			invokeTask2.Wait();

			// Test get property:
			Task retrieveTask = remoteDummy.Async((response, ex) => {
				Assert.That(null == ex);
				Assert.Equals(dummyObject.Bar, response);
			}).Bar;

			retrieveTask.Wait();

			// Test set property
			remoteDummy.Async((response, ex) => {
				Assert.Equals(true, response);
				Assert.That(null == ex);

			}).HAHA = 1111;

			Thread.Sleep(100);

			Assert.Equals(dummyObject.HAHA, 1111);

			Task failTask = remoteDummy.Async((response, ex) => {

				Assert.That(null != ex);

			}).ThisMethodDoesNotExist();
		
			failTask.Wait();

            // Test that enumerations can be changed remotely
            DummyEnumClass dummyEnum = new DummyEnumClass();
            rec.AddDevice(dummyEnum);

            dynamic remoteDummyEnum = new RemoteDevice(dummyEnum.Identifier, Guid.Empty, connection);

            Assert.Equals(LogLevel.Info, dummyEnum.LogLevel);
            remoteDummyEnum.SetLogLevel(LogLevel.Message);
            Assert.Equals(LogLevel.Message, dummyEnum.LogLevel);
            remoteDummyEnum.SetLogLevel(2);
            Assert.Equals(LogLevel.Warning, dummyEnum.LogLevel);
            remoteDummyEnum.LogLevel = LogLevel.Error;
            Assert.Equals(LogLevel.Error, dummyEnum.LogLevel);
            remoteDummyEnum.LogLevel = 4;
            Assert.Equals(LogLevel.Temp, dummyEnum.LogLevel);

            /// -- Test struct invocation
            InvokableDummy dummyInvokable = new InvokableDummy();
            rec.AddDevice(dummyInvokable);

            dynamic remoteInvokable = new RemoteDevice(dummyInvokable.Identifier, Guid.Empty, connection);

            Assert.Equals(0, remoteInvokable.Decodable.AnInt);
            Assert.That(null == remoteInvokable.Decodable.SomeStrings);

            InvokableDecodableDummy decodable = new InvokableDecodableDummy { AnInt = 43, SomeStrings = new string[] { "Katt", "Hund" } };

            remoteInvokable.SetDecodable(decodable);

            Assert.Equals(43, dummyInvokable.Decodable.AnInt);
            Assert.Equals(2, dummyInvokable.Decodable.SomeStrings.Count());
            Assert.Equals("Hund", dummyInvokable.Decodable.SomeStrings.Last());

            remoteInvokable.Decodable = new InvokableDecodableDummy { AnInt = 44, SomeStrings = new string[] { "Din", "Mammas", "Ost" } };
            Assert.Equals(44, dummyInvokable.Decodable.AnInt);
            Assert.Equals(3, dummyInvokable.Decodable.SomeStrings.Count());
            Assert.Equals("Ost", dummyInvokable.Decodable.SomeStrings.Last());

            remoteInvokable.Decodable = new InvokableDecodableDummy { Nested = new InvokableNestedStruct { AString = "Foo" } };

            Assert.Equals(0, dummyInvokable.Decodable.AnInt);
            Assert.That(null == dummyInvokable.Decodable.SomeStrings);
            Assert.Equals("Foo", dummyInvokable.Decodable.Nested.AString);

            // Stop everything
            s.Stop();
			client.Stop();
			Thread.Sleep(500);

		}


		bool wasInvoked = false;

		[Test]
		public void X_DeviceNotificationTest() {
            PrintName();

            InvokerDummyDevice invokedDevice = new InvokerDummyDevice("x");
			m_deviceManager = new DeviceManager("dm");
			m_deviceManager.Add(invokedDevice);

			invokedDevice.AddObserver(this);
			invokedDevice.SomeMethod();
			Thread.Sleep(200);
			Assert.That(wasInvoked);

			Assert.Equals(invokedDevice.SomeValue, 43);
			invokedDevice.RemoveObserver(this);
			wasInvoked = false;
			invokedDevice.SomeMethod(44);
			Thread.Sleep(200);
			Assert.That(false == wasInvoked);
			Assert.Equals(invokedDevice.SomeValue, 44);

		}

        [Test]
        public void TestRemoteDevice() {
            PrintName();

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
            Assert.Equals("hund", remoteDeviceDynamic.hund);

            string hundValue = null;

            // -- Test asynchronous
            remoteDevice.Async((response, exception) => {

                Assert.That(null == exception);
                hundValue = response;

            }).din_mamma();

            Thread.Sleep(50);
            Assert.Equals("hund", hundValue);

            // Call a fake method 10 times.
            int receiveCount = 0;
            host.Delay = 10;
            for (int i = 0; i < 10; i++) {

                remoteDevice.Async((response, exception) => {

                    Assert.That(null == exception);
                    Assert.Equals("hund", response);
                    receiveCount++;

                }).din_mamma();

            }

            Thread.Sleep(100); // They should be done by now... (but maybe not)

            Assert.Equals(10, receiveCount); // .. and all 3 should have been executed.

            // Shoul lose requests before the last one.
            remoteDevice.LossyRequests = true;
            receiveCount = 0;
            host.Delay = 50; // Add a small delay

            // Call a fake method 50 times. If running this test continously, it will start failing.
            for (int i = 0; i < 50; i++) {
              
                remoteDevice.Async((response, exception) => {

                    Log.t($" -x- {i}");
                    Assert.That(null == exception);
                    receiveCount++;

                }).din_mamma();

            }

            Thread.Sleep(300); // They should be done by now... (but maybe not)
            Assert.Equals(2, receiveCount); // .. only 6 should have been executed

            // Test GetValue

            remoteDevice.Async((response, exception) => {

                Assert.That(null == exception);
                Assert.Equals("hund", response);

            }).GetValue("pappa");

            // Test error:

            host.NextResponse = new NetworkMessage {

                Payload = "Argh!",
                Code = NetworkStatusCode.BadRequest.Raw()

            };

            remoteDevice.Async((response, exception) => {

                Assert.That(null != exception);
                Assert.That(exception.Message.Contains("Argh!"));

            });

            Thread.Sleep(400);

        }


        public void OnValueChanged(IDeviceNotification<object> notification) {

			InvokerDummyDevice device = m_deviceManager.Get(notification.Identifier);

			Assert.That(null != device);
			Assert.Equals(notification.NewValue, 42);
			Assert.Equals(notification.NewValue, device.SomeValue);
			Assert.Equals(notification.Action, "SomeMethod");

			device.SomeValue = 43;
			wasInvoked = true;

		}

        class DummyDeviceConnectionManagerTestDevice: DeviceBase {

            private bool m_ready;

            public override bool Ready => m_ready;

            public DummyDeviceConnectionManagerTestDevice(): base("test_device") { }

            public override void Stop() { m_ready = false; }

            public override void Start() { m_ready = true; }

        }

        [Test]
        public void TestDeviceConnectionManager() {
            PrintName();

            DeviceConnectionManager deviceConnectionManager = new DeviceConnectionManager("dm", 100, 10);

            DummyDeviceConnectionManagerTestDevice device = new DummyDeviceConnectionManagerTestDevice();

            deviceConnectionManager.Add(device);

            Assert.That(false == device.Ready);
            deviceConnectionManager.Start();
            Assert.That(false == device.Ready);
            Thread.Sleep(500);
            Assert.That(device.Ready);
            device.Stop();
            Assert.That(false == device.Ready);
            Thread.Sleep(20);
            Assert.That(device.Ready);

        }


    }

}

