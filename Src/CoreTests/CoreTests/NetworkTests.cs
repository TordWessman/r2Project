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
using Core.Network;
using Core.Data;
using System.Collections.Generic;
using Core.Device;
using System.Threading;
using System.Net;
using Core.Scripting;

namespace Core.Tests
{
	class DummyReceiver: IWebObjectReceiver {

		public IWebIntermediate Response;

		public INetworkMessage OnReceive (INetworkMessage input, IPEndPoint endpoint = null) {
		
			Assert.AreEqual ("Bar", input.Payload.Foo);
			Assert.AreEqual (42, input.Payload.Bar);
			Assert.AreEqual ("InputFooBar", input.Headers ["InputBaz"]);
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

	}

}