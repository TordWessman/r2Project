﻿// This file is part of r2Poject.
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
using System.Threading;
using R2Core.Device;
using System.Threading.Tasks;

namespace R2Core.Tests
{
	
	[TestFixture]
	public class TestAsyncRemoteDevice: NetworkTests
	{

		const int tcp_port = 4444;

		[Test]
		public void TestRemoteDevice() {
		
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

			IHostConnection connection = new HostConnection ("hc", "/test", client);

			RemoteDevice remoteDummy = new RemoteDevice ("dummy_device", Guid.Empty, connection);

			// Test method with result
			Task invokeTask = remoteDummy.Async ((result, ex) => { 
				
				Assert.IsNull (ex);
				Assert.AreEqual (100, result);

			}).MultiplyByTen(10);

			invokeTask.Wait ();

			// Test invoking non-returning method
			Task invokeTask2 = remoteDummy.Async ((result, ex) => { 

				Assert.IsNull (ex);

			}).NoParamsNoNothing();

			invokeTask2.Wait ();

			// Test get property:
			Task retrieveTask = remoteDummy.Async ((response, ex) => {
				Assert.IsNull(ex);
				Assert.AreEqual(dummyObject.Bar, response);
			}).Bar;

			retrieveTask.Wait ();

			// Test set property
			remoteDummy.Async ((response, ex) => {
				Assert.IsNull(ex);

			}).HAHA = 1111;

			Thread.Sleep (100);

			Assert.AreEqual(dummyObject.HAHA, 1111);

			Task failTask = remoteDummy.Async ((response, ex) => {

				Assert.NotNull(ex);

			}).ThisMethodDoesNotExist();
		
			failTask.Wait ();

		}

	}

}
