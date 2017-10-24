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
		private ISerialization serialization;

		private WebFactory factory;


		[TestFixtureSetUp]
		public override void Setup() {
		
			base.Setup ();
			serialization = m_dataFactory.CreateSerialization ("serializer", System.Text.Encoding.UTF8);
			factory = new WebFactory ("wf", m_deviceManager, serialization);

			DummyDevice d = new DummyDevice ("dummy_device");
			m_deviceManager.Add (d);

		}

		[Test]
		public void TestJsonEndpoint() {
		
			IWebIntermediate response = new RubyWebIntermediate ();

			response.Data.Foo = "Bar";

			DummyReceiver receiver = new DummyReceiver ();
			receiver.Response = response;

			IWebEndpoint ep = factory.CreateJsonEndpoint ("/json", receiver);

			DummyInput inputObject = new DummyInput ();

			IDictionary<string, object> headers = new Dictionary<string, object> ();
			headers ["InputBaz"] = "InputFooBar";

			byte[] input = serialization.Serialize (inputObject);

			byte[]r = ep.Interpret(input, "/json", headers);

			dynamic output = serialization.Deserialize (r);

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
				Token = "no_token", 
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

		}

		[Test]
		public void TestDeviceRouterSet() {

			DummyDevice dummyObject = m_deviceManager.Get ("dummy_device");
			dummyObject.HAHA = 0;

			DeviceRouter rec = (DeviceRouter) factory.CreateDeviceObjectReceiver ();
			rec.AddDevice (dummyObject);

			string jsonString = 
				"{ " +

				"\"Token\": \"no_token\"," +
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
		public void TestPackageFactory() {
		
			var packageFactory = factory.CreatePackageFactory ();

			DummyDevice d = new DummyDevice ("dummyXYZ");
			d.HAHA = 42.25f;

			IDictionary<string, object> headers = new Dictionary<string, object>();
		
			headers ["Dog"] = "Mouse";

			// Test dynamic serialization

			TCPMessage p = new TCPMessage () { Destination = "dummy_path", Headers = headers, Payload = d};

			byte[] raw = packageFactory.SerializeMessage (p);

			TCPMessage punwrapped = packageFactory.DeserializePackage (new System.IO.MemoryStream(raw));

			Assert.AreEqual ("Mouse", punwrapped.Headers ["Dog"]);

			Assert.AreEqual (42.25f, serialization.Deserialize(punwrapped.Payload).HAHA);

			Assert.AreEqual ("dummyXYZ", serialization.Deserialize(punwrapped.Payload).Identifier);


			// Test string serialization

			p = new TCPMessage () { Destination = "path", Headers = headers, Payload = "StringValue"};
			raw = packageFactory.SerializeMessage (p);
			punwrapped = packageFactory.DeserializePackage (new System.IO.MemoryStream(raw));

			Assert.AreEqual ("StringValue", serialization.Encoding.GetString (punwrapped.Payload));


			// Test byte array seralization

			byte[] byteArray = { 0, 1, 2, 3, 4, 5, 6, 255 };
			p = new TCPMessage () { Destination = "path", Payload = byteArray};
			raw = packageFactory.SerializeMessage (p);
			punwrapped = packageFactory.DeserializePackage (new System.IO.MemoryStream(raw));
			Assert.IsTrue (punwrapped.Payload is byte[]);

			for (int i = 0; i < byteArray.Length; i++) {
				Assert.AreEqual (byteArray [i], punwrapped.Payload [i]);
			}


			// Test null-payload
			p = new TCPMessage () { Destination = "path"};

			raw = packageFactory.SerializeMessage (p);
			punwrapped = packageFactory.DeserializePackage (new System.IO.MemoryStream(raw));

			Assert.IsEmpty (punwrapped.Payload);

			// Test null package with code:

			p = new TCPMessage () { Code = 666 };
			raw = packageFactory.SerializeMessage (p);
			punwrapped = packageFactory.DeserializePackage (new System.IO.MemoryStream(raw));

			Assert.IsEmpty (punwrapped.Payload);
			Assert.AreSame (punwrapped.Destination, "");
			Assert.AreEqual (punwrapped.Code, 666);

		}

		[Test]
		public void TestTCPServerBasics() {

			IWebServer s = factory.CreateTCPServer ("s", 1111);
			s.Start ();
			Thread.Sleep (100);
			Assert.IsTrue (s.Ready);
			Thread.Sleep (100);
			s.Stop ();
			Thread.Sleep (100);
			Assert.IsFalse (s.Ready);
			s = factory.CreateTCPServer ("s", 4242);

			s.Start ();
			Thread.Sleep (500);


			IMessageClient<TCPMessage> client = factory.CreateTCPClient ("c", "localhost", 4242);

			client.Start ();
			Assert.IsTrue (client.Ready);

			TCPMessage message = new TCPMessage () { Destination = "blah", Payload = "bleh"};
			TCPMessage response = client.Send (message);
			Assert.AreEqual (response.Code, (int)WebStatusCode.NotFound);
			client.Stop ();
			s.Stop ();



		}


		[Test]
		public void TestTCPServerWithEndpoint() {


			IWebServer s = factory.CreateTCPServer ("s", 4243);
			s.Start ();
			Thread.Sleep (100);

			// Set up scripts and add endpoint
			var scriptFactory = new RubyScriptFactory ("sf", new List<string>() { Settings.Paths.RubyLib(),
				Settings.Paths.Common()}, m_deviceManager);
			scriptFactory.AddSourcePath (Settings.Paths.TestData ());
			dynamic script = scriptFactory.CreateScript("test_server");
			var receiver = factory.CreateRubyScriptObjectReceiver (script);
			var jsonEndpoint = factory.CreateJsonEndpoint (@"/test", receiver);
			s.AddEndpoint (jsonEndpoint);

			// Do the message passing
			dynamic msg = new R2Dynamic ();
			msg.text = "foo";

			IMessageClient<TCPMessage> client = factory.CreateTCPClient ("c", "localhost", 4243);

			client.Start ();

			dynamic testObject = new R2Dynamic ();
			testObject.ob = new R2Dynamic ();
			testObject.ob.bar = 42;
			testObject.text = null;

			TCPMessage  message2 = new TCPMessage () { Destination = "/test", Payload = testObject};

			TCPMessage response2 = client.Send (message2);

			Assert.AreEqual (TCPPackageFactory.PayloadType.Dynamic, response2.PayloadType);
			Assert.AreEqual (42 * 10, response2.Payload.foo);

			TCPMessage message = new TCPMessage () { Destination = "/test", Payload = msg};
			TCPMessage response = client.Send (message);

			//System.IO.File.WriteAllText ("test.txt", (string)response.Payload);

			Assert.AreEqual ("foo", serialization.Encoding.GetString(response.Payload));
			script.additional_string = "bar";
			response = client.Send (message);
			Assert.AreEqual ((int)WebStatusCode.Ok, response.Code);
			Assert.AreEqual ("foobar", serialization.Encoding.GetString(response.Payload));


		}

		public struct JsonMessage {

			public string FlName;

		};


		[Test]
		public void HttpBinaryMessageAndScriptServerTests() {
		
			var webServer = factory.CreateHttpServer ("test_server", 9999);


			var scriptFactory = new RubyScriptFactory ("sf", new List<string>() { Settings.Paths.RubyLib(),
				Settings.Paths.Common()}, m_deviceManager);

			scriptFactory.AddSourcePath (Settings.Paths.TestData ());

			var script = scriptFactory.CreateScript("file_server");
			var receiver = factory.CreateRubyScriptObjectReceiver (script);
			var jsonEndpoint = factory.CreateJsonEndpoint (@"/test2", receiver);

			webServer.AddEndpoint (jsonEndpoint);
			webServer.Start ();

			Thread.Sleep (100);
		
			var client = factory.CreateHttpClient ("client");

			var message = factory.CreateHttpMessage ("http://localhost:9999/test2");

			// Test binary message:

			//dynamic msgBody = new R2Dynamic ();
			JsonMessage msgBody = new JsonMessage ();
			msgBody.FlName = Settings.Paths.TestData ("test.bin");
			message.Payload = msgBody;

			var response = client.Send (message);
			Assert.AreEqual (200, response.Code);
			Assert.AreEqual (6, (response.Payload as byte[]).Length);

			Assert.AreEqual ('d', (response.Payload as byte[]) [0]);
			Assert.AreEqual ('h', (response.Payload as byte[]) [4]);

			webServer.Stop ();

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

			var message = factory.CreateHttpMessage ("http://localhost:9999/test/test.json");

			var response = client.Send (message);
			Assert.AreEqual (200, response.Code);
			Assert.NotNull (response.Payload);
			Assert.AreEqual ("Bar", response.Payload.Foo);

			webServer.Stop ();
		}



	}

}