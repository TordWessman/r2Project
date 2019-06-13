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

namespace R2Core.Tests
{

	public class NetworkTests : TestBase {
		
		protected ISerialization serialization;

		protected WebFactory factory;

		[TestFixtureSetUp]
		public override void Setup() {
		
			base.Setup();
			serialization = new JsonSerialization("serializer", System.Text.Encoding.UTF8);
			factory = new WebFactory("wf", serialization);

			DummyDevice d = new DummyDevice("dummy_device");
			m_deviceManager.Add(d);

		}

		[TestFixtureTearDown]
		public override void Teardown() {
			base.Teardown();
		
			// Makes it more probable that all the clients/servers has been closed
			Thread.Sleep(1000);

		}

	}

}