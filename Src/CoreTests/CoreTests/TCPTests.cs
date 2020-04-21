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
using System.Collections.Generic;
using R2Core.Device;
using System.Threading;
using R2Core.Scripting;
using System.Linq;
using System.Threading.Tasks;

namespace R2Core.Tests
{
	[TestFixture]
	public class TCPTests : NetworkTests {
	
		const int tcp_port = 4444;

		[TestFixtureSetUp]
		public override void Setup() {

			base.Setup();
		
		}
		[Test]
		public void TestPackageFactory() {
			PrintName();

			var packageFactory = factory.CreateTcpPackageFactory();

			DummyDevice d = new DummyDevice("dummyXYZ");
			d.HAHA = 42.25f;

			IDictionary<string, object> headers = new Dictionary<string, object>();

			headers["Dog"] = "Mouse";

			// Test dynamic serialization

			TCPMessage p = new TCPMessage() { Destination = "dummy_path", Headers = headers, Payload = d};

			byte[] raw = packageFactory.SerializeMessage(p);

			TCPMessage punwrapped = packageFactory.DeserializePackage(new System.IO.MemoryStream(raw));

			Assert.AreEqual("Mouse", punwrapped.Headers["Dog"]);

			Assert.AreEqual(42.25f, punwrapped.Payload.HAHA);

			Assert.AreEqual("dummyXYZ", punwrapped.Payload.Identifier);


			// Test string serialization

			p = new TCPMessage() { Destination = "path", Headers = headers, Payload = "StringValue"};
			raw = packageFactory.SerializeMessage(p);
			punwrapped = packageFactory.DeserializePackage(new System.IO.MemoryStream(raw));

			Assert.AreEqual("StringValue", punwrapped.Payload);


			// Test byte array seralization

			byte[] byteArray = { 0, 1, 2, 3, 4, 5, 6, 255 };
			p = new TCPMessage() { Destination = "path", Payload = byteArray};
			raw = packageFactory.SerializeMessage(p);
			punwrapped = packageFactory.DeserializePackage(new System.IO.MemoryStream(raw));
			Assert.IsTrue(punwrapped.Payload is byte[]);

			for (int i = 0; i < byteArray.Length; i++) {
				Assert.AreEqual(byteArray[i], punwrapped.Payload[i]);
			}


			// Test null-payload
			p = new TCPMessage() { Destination = "path"};

			raw = packageFactory.SerializeMessage(p);
			punwrapped = packageFactory.DeserializePackage(new System.IO.MemoryStream(raw));

			Assert.IsEmpty(punwrapped.Payload);

			// Test null package with code:

			p = new TCPMessage() { Code = 666 };
			raw = packageFactory.SerializeMessage(p);
			punwrapped = packageFactory.DeserializePackage(new System.IO.MemoryStream(raw));

			Assert.IsEmpty(punwrapped.Payload);
			Assert.AreSame(punwrapped.Destination, "");
			Assert.AreEqual(punwrapped.Code, 666);

		}

		[Test]
		public void TestTCPServerBasics() {
			PrintName();

			IServer s = factory.CreateTcpServer("s", tcp_port);
			s.Start();
			Thread.Sleep(200);
			Assert.IsTrue(s.Ready);
			Thread.Sleep(200);
			s.Stop();
			Thread.Sleep(200);
			Assert.IsFalse(s.Ready);

			s.Start();
			Thread.Sleep(200);

			IMessageClient client = factory.CreateTcpClient("c", "localhost", tcp_port);

			client.Start();
			Assert.IsTrue(client.Ready);

			TCPMessage message = new TCPMessage() { Destination = "blah", Payload = "bleh"};
			INetworkMessage response = client.Send(message);
			Assert.AreEqual(NetworkStatusCode.NotFound.Raw(), response.Code);

			DummyEndpoint ep = new DummyEndpoint("test");
			s.AddEndpoint(ep);
			ep.MessingUp = new Func<INetworkMessage, INetworkMessage>(msg => {
				return new HttpMessage() {Code = 242, Payload = "din mamma"};
			});

			response = s.Interpret(new TCPMessage() { Destination = "/test" }, new System.Net.IPEndPoint(0,0));
			Assert.AreEqual("din mamma", response.Payload);
			Assert.AreEqual(242, response.Code);

			client.Stop();
			s.Stop();



		}


		[Test]
		public void TestTCPServer_Python_Endpoint() {
			PrintName();

			IServer s = factory.CreateTcpServer("s", tcp_port + 45);
			s.Start();
			Thread.Sleep(100);

			// Set up scripts and add endpoint
			var scriptFactory = new PythonScriptFactory("sf", Settings.Instance.GetPythonPaths(), m_deviceManager);
			scriptFactory.AddSourcePath(Settings.Paths.TestData());

			// see test_server.rb
			dynamic script = scriptFactory.CreateScript("test_server");

			var jsonEndpoint = scriptFactory.CreateEndpoint(script, @"/test");
			s.AddEndpoint(jsonEndpoint);

			// Do the message passing

			IMessageClient client = factory.CreateTcpClient("c", "localhost", tcp_port + 45);

			client.Start();

			dynamic testObject = new R2Dynamic();
			testObject.ob = new R2Dynamic();
			testObject.ob.bar = 42;
			testObject.text = null;

			TCPMessage  message2 = new TCPMessage() { Destination = "/test", Payload = testObject};

			INetworkMessage response2 = client.Send(message2);

			Assert.AreEqual(TCPPackageFactory.PayloadType.Dynamic, ((TCPMessage) response2).PayloadType);
			Assert.AreEqual(42 * 10, response2.Payload.foo);

			dynamic msg = new R2Dynamic();
			msg.text = "foo";
			TCPMessage message = new TCPMessage() { Destination = "/test", Payload = msg};
			INetworkMessage response = client.Send(message);

			Assert.AreEqual(NetworkStatusCode.Ok.Raw(), response.Code);
			Assert.AreEqual("foo", response.Payload);

			// Now also test the scripts additional_string public property
			script.additional_string = "bar";
			response = client.Send(message);
			Assert.AreEqual(NetworkStatusCode.Ok.Raw(), response.Code);
			Assert.AreEqual("foobar", response.Payload);

			s.Stop();
			client.Stop();

		}

		[Test]
		public void TestTCPServer_DeviceRouter() {
			PrintName();

			IServer s = factory.CreateTcpServer(Settings.Identifiers.TcpServer(), tcp_port + 44);
			s.Start();
			DummyDevice dummyObject = m_deviceManager.Get("dummy_device");
			dummyObject.Bar = "XYZ";
			DeviceRouter rec = (DeviceRouter) factory.CreateDeviceRouter(m_deviceManager);
			rec.AddDevice(dummyObject);
			IWebEndpoint ep = factory.CreateJsonEndpoint(rec);
			s.AddEndpoint(ep);
			Thread.Sleep(500);
		
			var client = factory.CreateTcpClient("c", "localhost", tcp_port + 44);
			client.Start();

			//Client should be connected
			Assert.IsTrue(client.Ready);

			var requestPayload = new DeviceRequest() {
				Params = new List<object> {"Foo", 42, new Dictionary<string,string> {{"Cat", "Dog"}}}.ToArray(),
				ActionType =  DeviceRequest.ObjectActionType.Invoke,
				Action = "GiveMeFooAnd42AndAnObject",
				Identifier = "dummy_device"};
			
			TCPMessage  message = new TCPMessage() { Destination = Settings.Consts.DeviceDestination(), Payload = requestPayload};
			Thread.Sleep(500);
			INetworkMessage response = client.Send(message);

			Assert.AreEqual(NetworkStatusCode.Ok, (NetworkStatusCode)response.Code); 
			// Make sure the identifiers are the same.
			Assert.AreEqual(dummyObject.Identifier, response.Payload.Object.Identifier);

			// This is what the function should return
			Assert.AreEqual(12.34, response.Payload.ActionResponse);

			// The dummy object should now have been changed.
			Assert.AreEqual("Foo", dummyObject.Bar);

			int fortytwo = 42;
			DeviceRequest requestPayload2 = new DeviceRequest() { 
				Identifier = "dummy_device",
				ActionType = DeviceRequest.ObjectActionType.Invoke,
				Action = "MultiplyByTen",
				Params = new object[] { fortytwo }
			};

			TCPMessage  message2 = new TCPMessage() { Destination = Settings.Consts.DeviceDestination(), Payload = requestPayload2};
			INetworkMessage response2 = client.Send(message2);
			Assert.AreEqual(NetworkStatusCode.Ok, (NetworkStatusCode)response2.Code); 

			Assert.AreEqual(fortytwo * 10, response2.Payload.ActionResponse);
			Assert.AreEqual("dummy_device", response2.Payload.Object.Identifier);
			Assert.AreEqual("MultiplyByTen", response2.Payload.Action);

			s.Stop();
			client.Stop();

		}

		[Test]
		public void TestTCP_AsyncRemoteDevice() {
			PrintName();

			IServer s = factory.CreateTcpServer(Settings.Identifiers.TcpServer(), tcp_port + 1144);
			s.Start();
			DummyDevice dummyObject = m_deviceManager.Get("dummy_device");
			dummyObject.Bar = "XYZ";
			DeviceRouter rec = (DeviceRouter) factory.CreateDeviceRouter(m_deviceManager);
			rec.AddDevice(dummyObject);
			IWebEndpoint ep = factory.CreateJsonEndpoint(rec);
			s.AddEndpoint(ep);
			Thread.Sleep(500);

			var client = factory.CreateTcpClient("c", "localhost", tcp_port + 1144);
			client.Start();

			//Client should be connected
			Assert.IsTrue(client.Ready);
			HostConnection connection = new HostConnection("hc", client);

			RemoteDevice remoteDummy = new RemoteDevice("dummy_device", Guid.Empty, connection);

			Task getTask = remoteDummy.Async ((result, exception) => {

				Assert.IsNull(exception);
				Assert.AreEqual("XYZ",result);

			}).GetValue("Bar");

			Thread.Sleep(200);

			getTask.Wait();

		}

		[Test]
		public void TestTCP_OnCloseDelegate() {
			PrintName();

			TCPServer s = (TCPServer) factory.CreateTcpServer(Settings.Identifiers.TcpServer(), tcp_port);
			s.Timeout = 1000;
			s.Start();

			DummyEndpoint ep = new DummyEndpoint("apa");
			s.AddEndpoint(ep);

			Thread.Sleep(200);

			DummyClientObserver observer1 = new DummyClientObserver();
			DummyClientObserver observer2 = new DummyClientObserver();

			bool client1ReactsOnItsOwnClose = false;
			bool serverReactsOnItsClient1Close = false;
			bool client2ReactsOnServerClose = false;

			// Test that OnClose is called after a normal client close
			observer1.OnCloseAsserter = (c, exception) => { 

				client1ReactsOnItsOwnClose = true;
				Assert.IsFalse(c.Ready);
				Assert.IsNull(exception);

			};

			// Test that the OnClose is called after a server stop
			observer2.OnCloseAsserter = (c, exception) => { 

				client2ReactsOnServerClose = true;
				Assert.IsFalse(c.Ready);
				Assert.Null(exception);


			};

			var client1 = (TCPClient) factory.CreateTcpClient("c", "localhost", tcp_port);
			client1.Timeout = 1000;
			client1.AddClientObserver(observer1);
			client1.Start();
		
			Thread.Sleep(200);
			var client2 = (TCPClient) factory.CreateTcpClient("c2", "localhost", tcp_port);
			client2.AddClientObserver(observer2);
			client2.Timeout = 1000;
			client2.Start();

			Thread.Sleep(200);

			// Network polling does only start after the first Send.
			client2.Send (new OkMessage ());

			foreach (IClientConnection connection in s.Connections) {
			
				connection.OnDisconnect += (c, ex) => {

					serverReactsOnItsClient1Close = true;
					Assert.IsNull(ex);

				};

			}

			Log.d("Closing client1");
			client1.Stop();
			Thread.Sleep(2500);

			Assert.True(client1ReactsOnItsOwnClose);
			Assert.True(serverReactsOnItsClient1Close);

			Log.d("Closing server");
			s.Stop();
			Thread.Sleep(2500);

			Assert.True(client2ReactsOnServerClose);
			Log.d("Closing client2");
			client2.Stop();

		}

		[Test]
		public void TestTCP_Server_broadcast() {

			TCPServer s = (TCPServer)factory.CreateTcpServer(Settings.Identifiers.TcpServer(), tcp_port);
			s.Start();
			DummyEndpoint ep = new DummyEndpoint("apa");
			s.AddEndpoint(ep);

			//DummyDevice dummyObject = m_deviceManager.Get("dummy_device");
			//dummyObject.Bar = "XYZ";
			//DeviceRouter rec = (DeviceRouter)factory.CreateDeviceObjectReceiver();
			//rec.AddDevice(dummyObject);
			//IWebEndpoint ep = factory.CreateJsonEndpoint("/test", rec);
			//s.AddEndpoint(ep);
			Thread.Sleep(100);

			DummyClientObserver observer = new DummyClientObserver("ehh");

			var client = (TCPClient) factory.CreateTcpClient("c", "localhost", tcp_port);
			client.AddClientObserver(observer);
			client.Start();

			observer.Asserter = (msg) => { 

				Assert.AreEqual("bleh", msg.Payload); 
			
			};

			TCPMessage message = new TCPMessage() { Destination = "apa", Payload = "bleh"};
			client.Send(message);

			Thread.Sleep(200);

			// Check broadcast
			observer.Asserter = (msg) => { 

				Assert.AreEqual(42, msg.Payload.Bar); 
			};
		
			R2Dynamic tmp = new R2Dynamic();
			tmp["Bar"] = 42;

			s.Broadcast(new TCPMessage() { Destination = "ehh", Payload = tmp }, (response, error) => {

				Assert.Null(error);

			});

			Thread.Sleep(500);
			Assert.True(observer.OnRequestCalled);

			// For some reason, the value below is false, even if it has been called...
			Assert.True(observer.OnResponseCalled);

			Assert.True(observer.OnBroadcastReceived);
	
			s.Stop();
			client.Stop();
			Thread.Sleep(500);


		}

		bool onClientDisconnect = false;
		bool waitingForClientStop = true;

		public void TestTCP_ClientFunc() {
		
			DummyClientObserver observer = new DummyClientObserver();

			observer.OnCloseAsserter = (c, exception) => { 

				onClientDisconnect = true;
				Assert.IsFalse(c.Ready);
				Assert.IsNull(exception);

			};

			var client = (TCPClient) factory.CreateTcpClient("c", "localhost", tcp_port);
			client.AddClientObserver(observer);
			client.Start();

			Thread.Sleep(200);

			TCPMessage message = new TCPMessage() { Destination = "apa", Payload = "bleh"};
			client.Send(message);

			client.Stop();
			Thread.Sleep(200);
			client = null;
			waitingForClientStop = false;

		}

		[Test]
		public void TestTCP_ServerClientConnectionsDelegatesTest() {
			PrintName();

			TCPServer s = (TCPServer)factory.CreateTcpServer(Settings.Identifiers.TcpServer(), tcp_port);
			s.Timeout = 1000;
			s.Start();
			DummyEndpoint ep = new DummyEndpoint("apa");
			s.AddEndpoint(ep);
			Thread.Sleep(200);

			Thread t = new Thread(new ThreadStart(TestTCP_ClientFunc));
			t.Start();
			bool onServerReceived = false;
			bool onServerDisconnect = false;

			Thread.Sleep(200);

			IClientConnection connection = s.Connections.FirstOrDefault();

			connection.OnDisconnect += (c, ex) => {

				onServerDisconnect = true;
				Assert.IsNull(ex);

			};

			connection.OnReceive += (request, address) => {
			
				Assert.AreEqual(request.Payload, "bleh");
				onServerReceived = true;
				return default(TCPMessage);

			};

			while(waitingForClientStop) { Thread.Sleep(100); }

			Thread.Sleep(200);

			t.Abort();

			t = null;

			Thread.Sleep(2500);
			Assert.True(onClientDisconnect);
			Assert.True(onServerReceived);
			Assert.True(onServerDisconnect);
			s.Stop();

		}

		bool m_ClientReconnect_ServerCheck;

		[Test]
		public void TestTCP_ClientReconnect() {
			PrintName();
		
			var port = tcp_port + 912;
			TCPServer s = (TCPServer)factory.CreateTcpServer(Settings.Identifiers.TcpServer(), port);
			s.Timeout = 1000;
			s.Start();

			Thread.Sleep(200);
			Assert.True(s.Ready);
			m_ClientReconnect_ServerCheck = true;

			DummyClientObserver observer = new DummyClientObserver();

			observer.OnCloseAsserter = (c, exception) => { 

				// Wait until the server status has been asserted before continuing
				while(m_ClientReconnect_ServerCheck) { Thread.Sleep(100); }
				c.Start();
			
			};

			TCPClient client = (TCPClient)factory.CreateTcpClient("c", "localhost", port);
			client.Timeout = 500;
			client.AddClientObserver(observer);
			client.Start();

			Thread.Sleep(200);

			Assert.True(client.Ready);

			s.Stop();

			Thread.Sleep(600);

			Assert.False(s.Ready);
			Assert.False(client.Ready);
			s.Start();
			Thread.Sleep(200);
			Assert.True(s.Ready);
			m_ClientReconnect_ServerCheck = false;

			// After this, the DummyClientObservers OnCloseAsserter should have started the client again.
			Thread.Sleep(500);

			Assert.True(client.Ready);

			//test the stop-listen-method
			Assert.AreEqual(NetworkStatusCode.NotFound.Raw(), client.Send(new TCPMessage()).Code);
			client.StopListening();
			Assert.AreEqual(NetworkStatusCode.Ok.Raw(), client.Send(new TCPMessage()).Code);

			s.Stop();
			client.Stop();

		}

		[Test]
		public void TestTCP_ClientServer() {
			PrintName();
			var port = tcp_port + 913;
			TCPServer s = (TCPServer)factory.CreateTcpServer(Settings.Identifiers.TcpServer(), port);
			DummyEndpoint ep = new DummyEndpoint(Settings.Consts.ConnectionRouterAddHostDestination());

			IIdentity identity = new DummyIdentity();
			ep.MessingUp = new Func<INetworkMessage,INetworkMessage>(msg => {
			
				Assert.AreEqual(identity.Name, msg.Payload.HostName);
				return new TCPMessage() { Code = NetworkStatusCode.Ok.Raw() };

			});

			s.AddEndpoint(ep);
			s.Start();
			Thread.Sleep(100);

			TCPClientServer clientServer = factory.CreateTcpClientServer("client_server");
			clientServer.Timeout = 250;
			clientServer.Configure(identity, "127.0.0.1", port);
			clientServer.Start();

			Thread.Sleep(100);

			Assert.IsTrue(clientServer.Ready);

			// Try reconnection if remote router is down
			s.Stop();
			Thread.Sleep(50);
			Assert.IsFalse(clientServer.Ready);
			s.Start();
			Thread.Sleep(600);
			Assert.IsTrue(clientServer.Ready);

			s.Stop();
			clientServer.Stop();

		}

		[Test]
		public void TestTCP_TcpProxy() {
			PrintName();

			var proxy_inPort = tcp_port + 914;
			var proxy_outPort = tcp_port + 915;

			TCPProxy proxy = new TCPProxy("proxy", proxy_inPort, proxy_outPort);
			proxy.Start();
		}

	}

}

