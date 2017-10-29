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
	[TestFixture]
	public class TCPTests : NetworkTests
	{
	
		[TestFixtureSetUp]
		public override void Setup() {

			base.Setup ();
		
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

			Assert.AreEqual (42.25f, punwrapped.Payload.HAHA);

			Assert.AreEqual ("dummyXYZ", punwrapped.Payload.Identifier);


			// Test string serialization

			p = new TCPMessage () { Destination = "path", Headers = headers, Payload = "StringValue"};
			raw = packageFactory.SerializeMessage (p);
			punwrapped = packageFactory.DeserializePackage (new System.IO.MemoryStream(raw));

			Assert.AreEqual ("StringValue", punwrapped.Payload);


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
		public void TestTCPServer_Ruby_Endpoint() {

			IWebServer s = factory.CreateTCPServer ("s", 4243);
			s.Start ();
			Thread.Sleep (100);

			// Set up scripts and add endpoint
			var scriptFactory = new RubyScriptFactory ("sf", new List<string>() { Settings.Paths.RubyLib(),
				Settings.Paths.Common()}, m_deviceManager);
			scriptFactory.AddSourcePath (Settings.Paths.TestData ());

			// see test_server.rb
			dynamic script = scriptFactory.CreateScript("test_server");
			var receiver = factory.CreateRubyScriptObjectReceiver (script);
			var jsonEndpoint = factory.CreateJsonEndpoint (@"/test", receiver);
			s.AddEndpoint (jsonEndpoint);

			// Do the message passing

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

			dynamic msg = new R2Dynamic ();
			msg.text = "foo";
			TCPMessage message = new TCPMessage () { Destination = "/test", Payload = msg};
			TCPMessage response = client.Send (message);

			Assert.AreEqual ("foo", response.Payload);

			// Now also test the scripts additional_string public property
			script.additional_string = "bar";
			response = client.Send (message);
			Assert.AreEqual ((int)WebStatusCode.Ok, response.Code);
			Assert.AreEqual ("foobar", response.Payload);

			s.Stop ();

		}

		[Test]
		public void TestTCPServer_DeviceRouter() {

			IWebServer s = factory.CreateTCPServer ("s", 4244);
			s.Start ();
			DummyDevice dummyObject = m_deviceManager.Get ("dummy_device");
			dummyObject.Bar = "XYZ";
			DeviceRouter rec = (DeviceRouter) factory.CreateDeviceObjectReceiver ();
			rec.AddDevice (dummyObject);
			IWebEndpoint ep = factory.CreateJsonEndpoint ("/test", rec);
			s.AddEndpoint (ep);
			Thread.Sleep (100);
		
			var client = factory.CreateTCPClient ("c", "localhost", 4244);
			client.Start ();

			//Client should be connected
			Assert.IsTrue (client.Ready);

			var requestPayload = new WebObjectRequest() {
				Params = new List<object>() {"Foo", 42, new Dictionary<string,string>() {{"Cat", "Dog"}}}.ToArray(),
				ActionType =  WebObjectRequest.ObjectActionType.Invoke,
				Action = "GiveMeFooAnd42AndAnObject",
				Identifier = "dummy_device"};
			
			TCPMessage  message = new TCPMessage () { Destination = "/test", Payload = requestPayload};
			TCPMessage response = client.Send (message);

			// Make sure the identifiers are the same.
			Assert.AreEqual (dummyObject.Identifier, response.Payload.Object.Identifier);

			// This is what the function should return
			Assert.AreEqual (12.34, response.Payload.ActionResponse);

			// The dummy object should now have been changed.
			Assert.AreEqual ("Foo", dummyObject.Bar);

			int fortytwo = 42;
			WebObjectRequest requestPayload2 = new WebObjectRequest () { 
				Identifier = "dummy_device",
				ActionType = WebObjectRequest.ObjectActionType.Invoke,
				Action = "MultiplyByTen",
				Params = new object[] { fortytwo }
			};

			TCPMessage  message2 = new TCPMessage () { Destination = "/test", Payload = requestPayload2};
			TCPMessage response2 = client.Send (message2);

			Assert.AreEqual (fortytwo * 10, response2.Payload.ActionResponse);
			Assert.AreEqual ("dummy_device", response2.Payload.Object.Identifier);
			Assert.AreEqual ("MultiplyByTen", response2.Payload.Action);

			s.Stop ();

		}


	}
}

