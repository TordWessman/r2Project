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

namespace Core.Tests
{
	class DummyReceiver: IWebObjectReceiver {

		public IWebIntermediate Response;

		public IWebIntermediate OnReceive (dynamic input, IDictionary<string, object> metadata = null) {
		
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
		private IR2Serialization serialization;

		private WebFactory factory;


		[TestFixtureSetUp]
		public override void Setup() {
		
			base.Setup ();
			serialization = m_dataFactory.CreateSerialization ("serializer", HttpServer.DefaultEncoding);
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

			IWebEndpoint ep = factory.CreateJsonEndpoint ("", receiver);

			DummyInput inputObject = new DummyInput ();

			IDictionary<string, object> headers = new Dictionary<string, object> ();
			headers ["InputBaz"] = "InputFooBar";

			byte[] input = serialization.Serialize (inputObject);

			byte[]r = ep.Interpret(input, headers);

			dynamic output = serialization.Deserialize (r);

			Assert.AreEqual(response.Data.Foo, output.Foo);
			ep.Metadata ["Baz"] = "FooBar";

		}

		[Test]
		public void TestDeviceRouterInvoke() {
		
			dynamic dummyObject = m_deviceManager.Get ("dummy_device");
			dummyObject.Bar = "XYZ";

			IWebObjectReceiver rec = factory.CreateDeviceObjectReceiver ();
			string jsonString = 
				"{ " +

				"\"Token\": \"no_token\"," +
				" \"Params\": [ \"Foo\", 42, {\"Cat\": \"Dog\"} ]," +
					" \"ActionType\": 2, " +
				" \"Action\": \"GiveMeFooAnd42AndAnObject\", " +
					" \"Identifier\": \"dummy_device\"" +
				" }";

			byte[] serialized = serialization.Encoding.GetBytes(jsonString);
			dynamic deserialized = serialization.Deserialize(serialized);

			Assert.AreEqual("dummy_device", deserialized.Identifier);
			Assert.NotNull (deserialized.Params[2]);
			Assert.AreEqual ("Dog", deserialized.Params [2].Cat);

			var metadata = new Dictionary<string, object> ();

			IWebIntermediate result = rec.OnReceive (deserialized, metadata);

			// Make sure the identifiers are the same.
			Assert.AreEqual (deserialized.Identifier, result.Data.Object.Identifier);

			// This is what the function should return
			Assert.AreEqual (12.34f, result.Data.ActionResponse);

			// The dummy object should now have been changed.
			Assert.AreEqual ("Foo", dummyObject.Bar);

		}

		[Test]
		public void TestDeviceRouterSet() {

			dynamic dummyObject = m_deviceManager.Get ("dummy_device");
			dummyObject.HAHA = 0;

			IWebObjectReceiver rec = factory.CreateDeviceObjectReceiver ();
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

			IWebIntermediate result = rec.OnReceive (deserialized, null);

			// Make sure the identifiers are the same.
			Assert.AreEqual (deserialized.Identifier, result.Data.Object.Identifier);

			// The dummy object should now have been changed.
			Assert.AreEqual (42.1f, dummyObject.HAHA);

		}

		[Test]
		public void TestPackageFactory() {
		
			var packageFactory = factory.CreateTCPPackageFactory ("factorrrr");

			DummyDevice d = new DummyDevice ("dummyXYZ");
			d.HAHA = 42.25f;

			IDictionary<string, object> headers = new Dictionary<string, object>();
		
			headers ["Dog"] = "Mouse";

			TCPPackage p = new TCPPackage ("dummy_path", headers, d);

			byte[] raw = packageFactory.CreateData (p);

			TCPPackage punwrapped = packageFactory.CreatePackage (raw);

			Assert.AreEqual ("Mouse", punwrapped.Headers ["Dog"]);

			Assert.AreEqual (42.25f, punwrapped.Payload.HAHA);

			Assert.AreEqual ("dummyXYZ", punwrapped.Payload.Identifier);

		}

	}

}