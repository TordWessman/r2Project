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
using Core.Network.Web;
using Core.Data;
using System.Collections.Generic;
using Core.Device;
using System.Threading;
using System.Net;
using Core.Scripting;
using Core.Network;

namespace Core.Tests
{
	class DummyReceiver: IWebObjectReceiver {

		public IWebIntermediate Response;

		public IWebIntermediate OnReceive (dynamic input, string path, IDictionary<string, object> metadata = null) {
		
			Assert.AreEqual ("Bar", input.Foo);
			Assert.AreEqual (42, input.Bar);
			Assert.AreEqual ("InputFooBar", metadata ["InputBaz"]);
			Response.AddMetadata ("Baz", "FooBar");

			return Response;

		}

	}

	class DummyInput {
	
		public string Foo = "Bar";
		public int Bar = 42;
	}



	[TestFixture]
	public class NetworkTests: TestBase
	{
		protected ISerialization serialization;

		protected WebFactory factory;


		[TestFixtureSetUp]
		public override void Setup() {
		
			base.Setup ();
			serialization = m_dataFactory.CreateSerialization ("serializer", System.Text.Encoding.UTF8);
			factory = new WebFactory ("wf", m_deviceManager, serialization);

			DummyDevice d = new DummyDevice ("dummy_device");
			m_deviceManager.Add (d);

		}

		[Test]
		public void TestEndpointUsingDummyReceiver() {
		
			IWebIntermediate response = new RubyWebIntermediate ();

			response.Data.Foo = "Bar";

			DummyReceiver receiver = new DummyReceiver ();
			receiver.Response = response;

			IWebEndpoint ep = factory.CreateJsonEndpoint ("/json", receiver);

			DummyInput inputObject = new DummyInput ();

			IDictionary<string, object> headers = new Dictionary<string, object> ();
			headers ["InputBaz"] = "InputFooBar";

			dynamic r = ep.Interpret(inputObject, "/json", headers);

			dynamic output = r;

			Assert.AreEqual(response.Data.Foo, output.Foo);
			ep.Metadata ["Baz"] = "FooBar";

		}

		[Test]
		public void TestDeviceRouterInvoke() {
		
			DummyDevice dummyObject = m_deviceManager.Get ("dummy_device");
			dummyObject.Bar = "XYZ";

			DeviceRouter rec = (DeviceRouter) factory.CreateDeviceObjectReceiver ();
			rec.AddDevice (dummyObject);

			var request = new WebObjectRequest() {
				Params = new List<object>() {"Foo", 42, new Dictionary<string,string>() {{"Cat", "Dog"}}}.ToArray(),
				ActionType =  WebObjectRequest.ObjectActionType.Invoke,
				Action = "GiveMeFooAnd42AndAnObject",
				Identifier = "dummy_device"};
			
			byte[] serialized = serialization.Serialize(request);
			dynamic deserialized = serialization.Deserialize(serialized);

			Assert.AreEqual("dummy_device", deserialized.Identifier);
			Assert.NotNull (deserialized.Params[2]);
			Assert.AreEqual ("Dog", deserialized.Params [2].Cat);

			var metadata = new Dictionary<string, object> ();

			IWebIntermediate result = rec.OnReceive (deserialized, "", metadata);

			// Make sure the identifiers are the same.
			Assert.AreEqual (deserialized.Identifier, result.Data.Object.Identifier);

			// This is what the function should return
			Assert.AreEqual (12.34f, result.Data.ActionResponse);

			// The dummy object should now have been changed.
			Assert.AreEqual ("Foo", dummyObject.Bar);


			int fortytwo = 42;
			WebObjectRequest wob = new WebObjectRequest () { 
				Identifier = "dummy_device",
				ActionType = WebObjectRequest.ObjectActionType.Invoke,
				Action = "MultiplyByTen",
				Params = new object[] { fortytwo }
			};

			serialized = serialization.Serialize (wob);
			//Mimic transformation to byte array -> send to host -> hoste deserialize
			deserialized = serialization.Deserialize(serialized);

			result = rec.OnReceive (deserialized, "", null);
			Assert.AreEqual (result.Data.ActionResponse, fortytwo * 10);
			Assert.AreEqual (result.Data.Object.Identifier, "dummy_device");
			Assert.AreEqual (result.Data.Action, "MultiplyByTen");

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

			IWebIntermediate result = rec.OnReceive (deserialized, "", null);

			// Make sure the identifiers are the same.
			Assert.AreEqual (deserialized.Identifier, result.Data.Object.Identifier);

			// The dummy object should now have been changed.
			Assert.AreEqual (42.1f, dummyObject.HAHA);


		}


		[Test]
		public void HttpBinaryMessageAndScriptServerTests() {
		
			var webServer = factory.CreateHttpServer ("test_server", 9999);

			var scriptFactory = new RubyScriptFactory ("sf", new List<string>() { Settings.Paths.RubyLib(),
				Settings.Paths.Common()}, m_deviceManager);

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

			dynamic msgBody = new R2Dynamic ();
			msgBody.FlName = Settings.Paths.TestData ("test.bin");
			message.Payload = msgBody;

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
		public void HttpTests() {
		
			var webServer = factory.CreateHttpServer ("s", 9999);

			var fileEndpoint = factory.CreateFileEndpoint (Settings.Paths.TestData (), @"/test/[A-Za-z0-9\.]+");

			webServer.AddEndpoint (fileEndpoint);

			webServer.Start ();

			Thread.Sleep (100);

			var client = factory.CreateHttpClient ("client");

			// Test json-message:

			HttpMessage message = factory.CreateHttpMessage ("http://localhost:9999/test/test.json");
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

			var requestPayload = new WebObjectRequest() {
				Params = new List<object>() {"Foo", 42, new Dictionary<string,string>() {{"Cat", "Dog"}}}.ToArray(),
				ActionType =  WebObjectRequest.ObjectActionType.Invoke,
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