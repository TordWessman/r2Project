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
using R2Core.Tests;
using R2Core;
using R2Core.Network;
using System.Threading;
using R2Core.Data;
using R2Core.Scripting;
using System.Collections.Generic;
using R2Core.Common;

namespace R2Core.Tests
{
	[TestFixture]
	public class HttpTests: NetworkTests {
		
		[TestFixtureSetUp]
		public override void Setup() {

			base.Setup();

		}


		[Test]
		public void TestHTTP_EndpointUsingDummyReceiver() {
			PrintName();

			ScriptNetworkMessage response = new ScriptNetworkMessage() { 
				Headers = new System.Collections.Generic.Dictionary<string, object>(),
				Payload = new R2Dynamic()
			};
			response.Payload.Foo = "Bar";

			DummyReceiver receiver = new DummyReceiver();
			receiver.Response = response;

			IWebEndpoint ep = factory.CreateJsonEndpoint(receiver);

			INetworkMessage inputObject = new NetworkMessage() { 
				Payload = new DummyInput(),
				Headers = new Dictionary<string, object>() { { "InputBaz", "InputFooBar" } },
				Destination = receiver.DefaultPath
			};

			INetworkMessage output = ep.Interpret(inputObject, null);

			Assert.AreEqual(response.Payload.Foo, output.Payload.Foo);
			Assert.AreEqual(response.Headers ["Baz"], "FooBar");

		}

		[Test]
		public void TestHTTP_DeviceRouterInvoke() {
			PrintName();

			DummyDevice dummyObject = m_deviceManager.Get("dummy_device");
			dummyObject.Bar = "XYZ";

			DeviceRouter rec = (DeviceRouter) factory.CreateDeviceRouter(m_deviceManager);
			rec.AddDevice(dummyObject);

			var request = new DeviceRequest() {
				Params = new List<object>() {"Foo", 42, new Dictionary<string,string>() {{"Cat", "Dog"}}}.ToArray(),
				ActionType =  DeviceRequest.ObjectActionType.Invoke,
				Action = "GiveMeFooAnd42AndAnObject",
				Identifier = "dummy_device"};

			byte[] serialized = serialization.Serialize(request);
			dynamic deserialized = serialization.Deserialize(serialized);

			Assert.AreEqual("dummy_device", deserialized.Identifier);
			Assert.NotNull(deserialized.Params[2]);
			Assert.AreEqual("Dog", deserialized.Params [2].Cat);

			INetworkMessage result = rec.OnReceive(new NetworkMessage{Payload = deserialized}, null);

			// Make sure the identifiers are the same.
			Assert.AreEqual(deserialized.Identifier, result.Payload.Object.Identifier);

			// This is what the function should return
			Assert.AreEqual(12.34f, result.Payload.ActionResponse);

			// The dummy object should now have been changed.
			Assert.AreEqual("Foo", dummyObject.Bar);

			int fortytwo = 42;
			DeviceRequest wob = new DeviceRequest() { 
				Identifier = "dummy_device",
				ActionType = DeviceRequest.ObjectActionType.Invoke,
				Action = "MultiplyByTen",
				Params = new object[] { fortytwo }
			};

			serialized = serialization.Serialize(wob);
			//Mimic transformation to byte array -> send to host -> hoste deserialize
			deserialized = serialization.Deserialize(serialized);

			result = rec.OnReceive(new NetworkMessage{Payload = deserialized}, null);
			Assert.AreEqual(result.Payload.ActionResponse, fortytwo * 10);
			Assert.AreEqual(result.Payload.Object.Identifier, "dummy_device");
			Assert.AreEqual(result.Payload.Action, "MultiplyByTen");

		}

		[Test]
		public void TestHTTP_DeviceRouterSet() {
			PrintName();

			DummyDevice dummyObject = m_deviceManager.Get("dummy_device");
			dummyObject.HAHA = 0;

			DeviceRouter rec = (DeviceRouter) factory.CreateDeviceRouter(m_deviceManager);
			rec.AddDevice(dummyObject);

			string jsonString = 
				"{ " +
				" \"Params\": [ 42.1 ]," +
				" \"ActionType\": 1, " +
				" \"Action\": \"HAHA\", " +
				" \"Identifier\": \"dummy_device\"" +
				" }";

			byte[] serialized = serialization.Encoding.GetBytes(jsonString);
			dynamic deserialized = serialization.Deserialize(serialized);
			Assert.AreEqual("dummy_device", deserialized.Identifier);

			INetworkMessage result = rec.OnReceive(new NetworkMessage{Payload = deserialized}, null);

			// Make sure the identifiers are the same.
			Assert.AreEqual(deserialized.Identifier, result.Payload.Object.Identifier);

			// The dummy object should now have been changed.
			Assert.AreEqual(42.1d, dummyObject.HAHA);

		}


		[Test]
		public void TestHTTP_BinaryMessageAndScriptServerTests() {
			PrintName();

			var webServer = factory.CreateHttpServer("test_server", 9999);

			var scriptFactory = new PythonScriptFactory("sf", Settings.Instance.GetPythonPaths(), m_deviceManager);

			scriptFactory.AddSourcePath(Settings.Paths.TestData());
            scriptFactory.AddSourcePath(Settings.Paths.Common());
            var file_server_script = scriptFactory.CreateScript("file_server");
			var file_server_endpoint = scriptFactory.CreateEndpoint(file_server_script, @"/test2");

			webServer.AddEndpoint(file_server_endpoint);
			webServer.Start();

			Thread.Sleep(100);

			var client = factory.CreateHttpClient("client");

			HttpMessage message = factory.CreateHttpMessage("http://localhost:9999/test2");
			message.ContentType = "application/json";

			// Test binary message:

			message.Payload = new R2Dynamic();
			message.Payload.name = Settings.Paths.TestData("test.bin");

			var response = client.Send(message);
			Assert.AreEqual(200, response.Code);
			Assert.AreEqual(6, (response.Payload as byte[]).Length);

			Assert.AreEqual('d', (response.Payload as byte[]) [0]);
			Assert.AreEqual('h', (response.Payload as byte[]) [4]);

			webServer.Stop();

			/*TODO: create endpoint that takes binary input
			 * 
			*/

		}

		[Test]
		public void TestHTTP_ServerTests() {
			PrintName();

			var webServer = factory.CreateHttpServer("s", 19999);

			var dummyEndpoint = new DummyEndpoint("test");
			webServer.AddEndpoint(dummyEndpoint);

			webServer.Start();

			Thread.Sleep(100);

			var client = factory.CreateHttpClient("client");

			HttpMessage message = factory.CreateHttpMessage("http://localhost:19999/test?parameter=foo");
			message.Method = "GET";
			message.Headers = new Dictionary<string, object> ();
			message.Headers["TestHeader"] = "a header value";

			dummyEndpoint.MessingUp = new Func<INetworkMessage, INetworkMessage>(msg => {

				Assert.AreEqual("foo", msg.Payload.parameter);
				Assert.AreEqual("a header value", msg.Headers["TestHeader"]);
				return new HttpMessage() {Code = 242, Payload = "din mamma"};
			});

			INetworkMessage response = client.Send(message);

			Assert.AreEqual("din mamma", response.Payload);
			Assert.AreEqual(242, response.Code);

			message = factory.CreateHttpMessage("/test");
			R2Dynamic payload = new R2Dynamic();
			payload["parameter"] = "foo";
			message.Payload = payload;

			message.Headers = new Dictionary<string, object> ();
			message.Headers["TestHeader"] = "a header value";

			response = webServer.Interpret(message, new System.Net.IPEndPoint(0,0));
			Assert.AreEqual("din mamma", response.Payload);
			Assert.AreEqual(242, response.Code);

			HttpMessage message2 = factory.CreateHttpMessage("/not.found");

			response = webServer.Interpret(message2, new System.Net.IPEndPoint(0,0));

			Assert.AreEqual(NetworkStatusCode.NotFound, (NetworkStatusCode)response.Code);

		}

		[Test]
		public void TestHTTP_ServerClientTests() {
			PrintName();

			var webServer = factory.CreateHttpServer("s", 9999);

			var fileEndpoint = factory.CreateFileEndpoint(Settings.Paths.TestData(), @"/test/[A-Za-z0-9\.]+");

			webServer.AddEndpoint(fileEndpoint);

			webServer.Start();

			Thread.Sleep(100);

			var client = factory.CreateHttpClient("client");

			// Test json-message:

			HttpMessage message = factory.CreateHttpMessage("http://localhost:9999/test/test.json");
			message.Method = "GET";
			message.ContentType = "application/json";
			var response = client.Send(message);
			Assert.AreEqual(200, response.Code);
			Assert.NotNull(response.Payload);
			Assert.AreEqual("Bar", response.Payload.Foo);


			webServer.Stop();

		}

		[Test]
		public void TestHTTP_DeviceRouterTest() {
			PrintName();

			var webServer = factory.CreateHttpServer("s", 9999);

			DummyDevice dummyObject = m_deviceManager.Get("dummy_device");
			dummyObject.Bar = "XYZ";

			DeviceRouter rec = (DeviceRouter) factory.CreateDeviceRouter(m_deviceManager);
			rec.AddDevice(dummyObject);

			IWebEndpoint ep = factory.CreateJsonEndpoint(rec);

			var requestPayload = new DeviceRequest() {
				Params = new List<object>() {"Foo", 42, new Dictionary<string,string>() {{"Cat", "Dog"}}}.ToArray(),
				ActionType =  DeviceRequest.ObjectActionType.Invoke,
				Action = "GiveMeFooAnd42AndAnObject",
				Identifier = "dummy_device"};

			webServer.AddEndpoint(ep);

			webServer.Start();

			Thread.Sleep(100);

			var client = factory.CreateHttpClient("client");

			// Test json-message:

			HttpMessage message = factory.CreateHttpMessage($"http://localhost:9999{Settings.Consts.DeviceDestination()}");

			// Test json-message:
			message.ContentType = "application/json";
			message.Payload = requestPayload;

			var response = client.Send(message);
			Assert.AreEqual(200, response.Code);

			// Make sure the identifiers are the same.
			Assert.AreEqual(dummyObject.Identifier, response.Payload.Object.Identifier);

			// This is what the function should return
			Assert.AreEqual(12.34,  response.Payload.ActionResponse);

			webServer.Stop();

		}

		[Test]
		public void TestHTTP_TCPClientServerRouter() {
			PrintName();

			// Router side port
			var tcpPort = 1119;
			var httpPort = 1118;

			// Server name
			var myHostName = "test_host";

			// Create the routing server that will route incomming tcp requests
			TCPServer routingServer = factory.CreateTcpServer("tcp_router", tcpPort);

			// Create the routing server that will route incomming http requests
			HttpServer routingHttpServer = factory.CreateHttpServer("http_router", httpPort);

			// Create the catch-all endpoint that will route traffic to the routingServer's connections 
			TCPRouterEndpoint routingEndpoint = factory.CreateTcpRouterEndpoint(routingServer);
			routingServer.Start();
			routingServer.AddEndpoint(routingEndpoint);
			routingHttpServer.Start();
			routingHttpServer.AddEndpoint(routingEndpoint);

			// Server side server. The final destination of the request
			HttpServer httpServer = factory.CreateHttpServer("http_server", tcpPort + 1);
			httpServer.Start();

			Thread.Sleep(100);

			// Server side router that will route traffic to a local servers
			TCPClientServer clientServer = factory.CreateTcpClientServer("client_server");
			clientServer.Configure(myHostName, "127.0.0.1", tcpPort);
			clientServer.Start();

			Thread.Sleep(100);

			Assert.IsTrue(clientServer.Ready);


			// Allow the router to access the destination http server
			clientServer.AddServer(Settings.Consts.ConnectionRouterHeaderServerTypeHTTP(), httpServer);

			// Create a dummy endpoint to test requests
			DummyEndpoint ep = new DummyEndpoint("test");

			// Add dummy endpoint functionality
			ep.MessingUp = new Func<INetworkMessage, INetworkMessage> (msg => {

				return new HttpMessage() {
					Code = 210,
					Payload = "din mamma: " + msg.Payload
				};
			});

			httpServer.AddEndpoint(ep);

			// Create the remote TCP client (imitate being a HTTP client)
			IMessageClient client = factory.CreateTcpClient("client", "127.0.0.1", tcpPort, myHostName);
			client.Start();

			// Override default behaviour: Set the destination server type. This means that all requests from this client should be directed to the HTTP server (if present)
			client.Headers[Settings.Consts.ConnectionRouterHeaderServerTypeKey()] = Settings.Consts.ConnectionRouterHeaderServerTypeHTTP();

			TCPMessage message = new TCPMessage() { Destination = "test", Payload = "argh" };

			INetworkMessage response = client.Send(message);

			Assert.AreEqual("din mamma: argh", response.Payload);
			Assert.AreEqual(210, response.Code);

			IMessageClient httpClient = factory.CreateHttpClient("http_client", myHostName);

			HttpMessage httpMessage = new HttpMessage () { Destination = $"http://127.0.0.1:{httpPort}/not_found", Payload = "argh" };
			httpMessage.ContentType = "text/string";

			INetworkMessage httpResponse = httpClient.Send(httpMessage);

			Assert.AreEqual(NetworkStatusCode.NotFound.Raw(), httpResponse.Code);

			httpMessage = new HttpMessage () { Destination = $"http://127.0.0.1:{httpPort}/test", Payload = "ugh" };
			httpMessage.ContentType = "text/string";
			httpResponse = httpClient.Send(httpMessage);

			Assert.AreEqual(210, httpResponse.Code);
			Assert.AreEqual("din mamma: ugh", httpResponse.Payload);

			routingServer.Stop();
			clientServer.Stop();
			client.Stop();
			httpServer.Stop();
		}

	}


}

