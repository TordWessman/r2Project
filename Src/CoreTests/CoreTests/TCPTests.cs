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
using R2Core.Data;
using System.Collections.Generic;
using R2Core.Device;
using System.Threading;
using System.Net;
using R2Core.Scripting;
using R2Core.Common;

namespace R2Core.Tests
{
	[TestFixture]
	public class TCPTests : NetworkTests
	{
	
		const int tcp_port = 4444;

		[TestFixtureSetUp]
		public override void Setup() {

			base.Setup ();
		
		}
		[Test]
		public void TestPackageFactory() {

			var packageFactory = factory.CreateTcpPackageFactory ();

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

			IServer s = factory.CreateTcpServer ("s", tcp_port);
			s.Start ();
			Thread.Sleep (100);
			Assert.IsTrue (s.Ready);
			Thread.Sleep (100);
			s.Stop ();
			Thread.Sleep (100);
			Assert.IsFalse (s.Ready);

			s.Start ();
			Thread.Sleep (100);

			IMessageClient client = factory.CreateTcpClient ("c", "localhost", tcp_port);

			client.Start ();
			Assert.IsTrue (client.Ready);

			TCPMessage message = new TCPMessage () { Destination = "blah", Payload = "bleh"};
			INetworkMessage response = client.Send (message);
			Log.t (" --- --- -- GOT RESPONSE HERE TOO!");
			Assert.AreEqual (WebStatusCode.NotFound.Raw(), response.Code);
			client.Stop ();
			s.Stop ();



		}


		[Test]
		public void TestTCPServer_Ruby_Endpoint() {

			IServer s = factory.CreateTcpServer ("s", tcp_port);
			s.Start ();
			Thread.Sleep (100);

			// Set up scripts and add endpoint
			var scriptFactory = new RubyScriptFactory ("sf", BaseContainer.RubyPaths, m_deviceManager);
			scriptFactory.AddSourcePath (Settings.Paths.TestData ());

			// see test_server.rb
			dynamic script = scriptFactory.CreateScript("test_server");
			var receiver = scriptFactory.CreateRubyScriptObjectReceiver (script);
			var jsonEndpoint = factory.CreateJsonEndpoint (@"/test", receiver);
			s.AddEndpoint (jsonEndpoint);

			// Do the message passing

			IMessageClient client = factory.CreateTcpClient ("c", "localhost", tcp_port);

			client.Start ();

			dynamic testObject = new R2Dynamic ();
			testObject.ob = new R2Dynamic ();
			testObject.ob.bar = 42;
			testObject.text = null;

			TCPMessage  message2 = new TCPMessage () { Destination = "/test", Payload = testObject};

			INetworkMessage response2 = client.Send (message2);

			Assert.AreEqual (TCPPackageFactory.PayloadType.Dynamic, ((TCPMessage) response2).PayloadType);
			Assert.AreEqual (42 * 10, response2.Payload.foo);

			dynamic msg = new R2Dynamic ();
			msg.text = "foo";
			TCPMessage message = new TCPMessage () { Destination = "/test", Payload = msg};
			INetworkMessage response = client.Send (message);

			Assert.AreEqual (response.Code, WebStatusCode.Ok.Raw());
			Assert.AreEqual ("foo", response.Payload);

			// Now also test the scripts additional_string public property
			script.additional_string = "bar";
			response = client.Send (message);
			Assert.AreEqual (WebStatusCode.Ok.Raw(), response.Code);
			Assert.AreEqual ("foobar", response.Payload);

			s.Stop ();

		}

		[Test]
		public void TestTCPServer_DeviceRouter() {

			IServer s = factory.CreateTcpServer ("s", tcp_port);
			s.Start ();
			DummyDevice dummyObject = m_deviceManager.Get ("dummy_device");
			dummyObject.Bar = "XYZ";
			DeviceRouter rec = (DeviceRouter) factory.CreateDeviceObjectReceiver ();
			rec.AddDevice (dummyObject);
			IWebEndpoint ep = factory.CreateJsonEndpoint ("/test", rec);
			s.AddEndpoint (ep);
			Thread.Sleep (500);
		
			var client = factory.CreateTcpClient ("c", "localhost", tcp_port);
			client.Start ();

			//Client should be connected
			Assert.IsTrue (client.Ready);

			var requestPayload = new DeviceRequest() {
				Params = new List<object>() {"Foo", 42, new Dictionary<string,string>() {{"Cat", "Dog"}}}.ToArray(),
				ActionType =  DeviceRequest.ObjectActionType.Invoke,
				Action = "GiveMeFooAnd42AndAnObject",
				Identifier = "dummy_device"};
			
			TCPMessage  message = new TCPMessage () { Destination = "/test", Payload = requestPayload};
			Thread.Sleep (500);
			INetworkMessage response = client.Send (message);

			Assert.AreEqual (WebStatusCode.Ok, (WebStatusCode)response.Code); 
			// Make sure the identifiers are the same.
			Assert.AreEqual (dummyObject.Identifier, response.Payload.Object.Identifier);

			// This is what the function should return
			Assert.AreEqual (12.34, response.Payload.ActionResponse);

			// The dummy object should now have been changed.
			Assert.AreEqual ("Foo", dummyObject.Bar);

			int fortytwo = 42;
			DeviceRequest requestPayload2 = new DeviceRequest () { 
				Identifier = "dummy_device",
				ActionType = DeviceRequest.ObjectActionType.Invoke,
				Action = "MultiplyByTen",
				Params = new object[] { fortytwo }
			};

			TCPMessage  message2 = new TCPMessage () { Destination = "/test", Payload = requestPayload2};
			INetworkMessage response2 = client.Send (message2);
			Assert.AreEqual (WebStatusCode.Ok, (WebStatusCode)response2.Code); 

			Assert.AreEqual (fortytwo * 10, response2.Payload.ActionResponse);
			Assert.AreEqual ("dummy_device", response2.Payload.Object.Identifier);
			Assert.AreEqual ("MultiplyByTen", response2.Payload.Action);

			s.Stop ();

		}

		public class DummyClientObserver : IMessageClientObserver {

			private string m_destination;

			public DummyClientObserver(string destination = null) {
			
				m_destination = destination;

			}

			public void OnReceive (INetworkMessage message, Exception ex) {
			
				Asserter (message);

			}

			public Action<INetworkMessage> Asserter;

			public string Destination { get { return m_destination; } }

		}
		[Test]
		public void TestTCP_Server_broadcast() {

			TCPServer s = (TCPServer) factory.CreateTcpServer ("s", tcp_port);
			s.Start ();
			//DummyDevice dummyObject = m_deviceManager.Get ("dummy_device");
			//dummyObject.Bar = "XYZ";
			//DeviceRouter rec = (DeviceRouter)factory.CreateDeviceObjectReceiver ();
			//rec.AddDevice (dummyObject);
			//IWebEndpoint ep = factory.CreateJsonEndpoint ("/test", rec);
			//s.AddEndpoint (ep);
			Thread.Sleep (100);

			DummyClientObserver observer = new DummyClientObserver ();

			var client = (TCPClient) factory.CreateTcpClient ("c", "localhost", tcp_port);
			client.AddObserver(observer);
			client.Start ();

			observer.Asserter = (msg) => { Assert.AreEqual(42, msg.Payload.Bar); };
		
			R2Dynamic tmp = new R2Dynamic ();
			tmp ["Bar"] = 42;

			s.Broadcast (new TCPMessage () { Payload = tmp }, (response, error) => {
				
			});
			s.Stop ();
			Thread.Sleep (2000);

		}

	}
}

