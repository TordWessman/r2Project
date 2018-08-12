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

namespace R2CoreTests
{
	[TestFixture]
	public class HttpTests: NetworkTests
	{
		[TestFixtureSetUp]
		public override void Setup() {

			base.Setup ();

		}


		[Test]
		public void TestEndpointUsingDummyReceiver() {

			IWebIntermediate response = new RubyWebIntermediate ();

			response.Payload.Foo = "Bar";

			DummyReceiver receiver = new DummyReceiver ();
			receiver.Response = response;

			IWebEndpoint ep = factory.CreateJsonEndpoint ("/json", receiver);

			INetworkMessage inputObject = new NetworkMessage () { 
				Payload = new DummyInput (),
				Headers = new Dictionary<string, object> () { { "InputBaz", "InputFooBar" } },
				Destination = "/json"
			};

			INetworkMessage output = ep.Interpret(inputObject, null);


			Assert.AreEqual(response.Payload.Foo, output.Payload.Foo);
			Assert.AreEqual (response.Headers ["Baz"], "FooBar");

		}

		[Test]
		public void TestDeviceRouterInvoke() {

			DummyDevice dummyObject = m_deviceManager.Get ("dummy_device");
			dummyObject.Bar = "XYZ";

			DeviceRouter rec = (DeviceRouter) factory.CreateDeviceObjectReceiver ();
			rec.AddDevice (dummyObject);

			var request = new DeviceRequest() {
				Params = new List<object>() {"Foo", 42, new Dictionary<string,string>() {{"Cat", "Dog"}}}.ToArray(),
				ActionType =  DeviceRequest.ObjectActionType.Invoke,
				Action = "GiveMeFooAnd42AndAnObject",
				Identifier = "dummy_device"};

			byte[] serialized = serialization.Serialize(request);
			dynamic deserialized = serialization.Deserialize(serialized);

			Assert.AreEqual("dummy_device", deserialized.Identifier);
			Assert.NotNull (deserialized.Params[2]);
			Assert.AreEqual ("Dog", deserialized.Params [2].Cat);

			INetworkMessage result = rec.OnReceive (new NetworkMessage{Payload = deserialized}, null);

			// Make sure the identifiers are the same.
			Assert.AreEqual (deserialized.Identifier, result.Payload.Object.Identifier);

			// This is what the function should return
			Assert.AreEqual (12.34f, result.Payload.ActionResponse);

			// The dummy object should now have been changed.
			Assert.AreEqual ("Foo", dummyObject.Bar);

			int fortytwo = 42;
			DeviceRequest wob = new DeviceRequest () { 
				Identifier = "dummy_device",
				ActionType = DeviceRequest.ObjectActionType.Invoke,
				Action = "MultiplyByTen",
				Params = new object[] { fortytwo }
			};

			serialized = serialization.Serialize (wob);
			//Mimic transformation to byte array -> send to host -> hoste deserialize
			deserialized = serialization.Deserialize(serialized);

			result = rec.OnReceive (new NetworkMessage{Payload = deserialized}, null);
			Assert.AreEqual (result.Payload.ActionResponse, fortytwo * 10);
			Assert.AreEqual (result.Payload.Object.Identifier, "dummy_device");
			Assert.AreEqual (result.Payload.Action, "MultiplyByTen");

		}

		[Test]
		public void TestDeviceRouterSet() {

			DummyDevice dummyObject = m_deviceManager.Get ("dummy_device");
			dummyObject.HAHA = 0;

			DeviceRouter rec = (DeviceRouter) factory.CreateDeviceObjectReceiver ();
			rec.AddDevice (dummyObject);

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

			INetworkMessage result = rec.OnReceive (new NetworkMessage{Payload = deserialized}, null);

			// Make sure the identifiers are the same.
			Assert.AreEqual (deserialized.Identifier, result.Payload.Object.Identifier);

			// The dummy object should now have been changed.
			Assert.AreEqual (42.1d, dummyObject.HAHA);


		}


		[Test]
		public void HttpBinaryMessageAndScriptServerTests() {

			var webServer = factory.CreateHttpServer ("test_server", 9999);

			var scriptFactory = new RubyScriptFactory ("sf", BaseContainer.RubyPaths , m_deviceManager);

			scriptFactory.AddSourcePath (Settings.Paths.TestData ());

			var file_server_script = scriptFactory.CreateScript("file_server");
			var file_server_receiver = factory.CreateRubyScriptObjectReceiver (file_server_script);
			var file_server_endpoint = factory.CreateJsonEndpoint (@"/test2", file_server_receiver);

			webServer.AddEndpoint (file_server_endpoint);
			webServer.Start ();

			Thread.Sleep (100);

			var client = factory.CreateHttpClient ("client");

			HttpMessage message = factory.CreateHttpMessage ("http://localhost:9999/test2");
			message.ContentType = "application/json";

			// Test binary message:

			message.Payload = new R2Dynamic ();
			message.Payload.FlName = Settings.Paths.TestData ("test.bin");

			var response = client.Send (message);
			Assert.AreEqual (200, response.Code);
			Assert.AreEqual (6, (response.Payload as byte[]).Length);

			Assert.AreEqual ('d', (response.Payload as byte[]) [0]);
			Assert.AreEqual ('h', (response.Payload as byte[]) [4]);

			webServer.Stop ();

			/*TODO: create endpoint that takes binary input
			 * 
			 * //message.ContentType = "application/octet-stream";
			* 			var file_server_script = scriptFactory.CreateScript("file_server");
			var file_server_receiver = factory.CreateRubyScriptObjectReceiver (file_server_script);
			var file_server_endpoint = factory.CreateJsonEndpoint (@"/test2", file_server_receiver);
			*/

		}

		[Test]
		public void HttpServerTests() {

			var webServer = factory.CreateHttpServer ("s", 9999);

			var fileEndpoint = factory.CreateFileEndpoint (Settings.Paths.TestData (), @"/test/[A-Za-z0-9\.]+");

			webServer.AddEndpoint (fileEndpoint);

			webServer.Start ();

			Thread.Sleep (100);

			var client = factory.CreateHttpClient ("client");

			// Test json-message:

			HttpMessage message = factory.CreateHttpMessage ("http://localhost:9999/test/test.json");
			message.Method = "GET";
			message.ContentType = "application/json";
			var response = client.Send (message);
			Assert.AreEqual (200, response.Code);
			Assert.NotNull (response.Payload);
			Assert.AreEqual ("Bar", response.Payload.Foo);


			webServer.Stop ();

		}

		[Test]
		public void HttpDeviceRouterTes() {

			var webServer = factory.CreateHttpServer ("s", 9999);

			DummyDevice dummyObject = m_deviceManager.Get ("dummy_device");
			dummyObject.Bar = "XYZ";

			DeviceRouter rec = (DeviceRouter) factory.CreateDeviceObjectReceiver ();
			rec.AddDevice (dummyObject);

			IWebEndpoint ep = factory.CreateJsonEndpoint ("/test", rec);

			var requestPayload = new DeviceRequest() {
				Params = new List<object>() {"Foo", 42, new Dictionary<string,string>() {{"Cat", "Dog"}}}.ToArray(),
				ActionType =  DeviceRequest.ObjectActionType.Invoke,
				Action = "GiveMeFooAnd42AndAnObject",
				Identifier = "dummy_device"};

			webServer.AddEndpoint (ep);

			webServer.Start ();

			Thread.Sleep (100);

			var client = factory.CreateHttpClient ("client");

			// Test json-message:

			HttpMessage message = factory.CreateHttpMessage ("http://localhost:9999/test");

			// Test json-message:
			message.ContentType = "application/json";
			message.Payload = requestPayload;

			var response = client.Send (message);
			Assert.AreEqual (200, response.Code);

			// Make sure the identifiers are the same.
			Assert.AreEqual (dummyObject.Identifier, response.Payload.Object.Identifier);

			// This is what the function should return
			Assert.AreEqual (12.34,  response.Payload.ActionResponse);

			webServer.Stop ();

		}




	}


}

