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
using System.Threading;
using Core.Network;
using Core.Data;
using System.Threading.Tasks;
using Core.Device;

namespace Core.Tests
{
	[TestFixture]
	public class UDPTests: NetworkTests
	{

		const int tcp_port = 4444;
		const int udp_port = 4445;

		[TestFixtureSetUp]
		public override void Setup() {

			base.Setup ();

		}

		[Test]
		public void TestSendReceive() {

			var s = factory.CreateUdpServer ("s", udp_port);
			DummyEndpoint ep = new DummyEndpoint ("/dummy");
			UDPBroadcaster client = factory.CreateUdpClient ("c", udp_port);

			(s as UDPServer).AllowLocalRequests = true;

			s.AddEndpoint (ep);

			s.Start ();
			Thread.Sleep (100);
			Assert.True (s.Ready);
			s.Stop ();
			Thread.Sleep (100);
			Assert.False (s.Ready);
			s.Start ();
			Thread.Sleep (100);
			Assert.True (s.Ready);

			bool didReceiveResponse = false;

			var guid = client.Broadcast (new TCPMessage () { Destination = "should not be found" }, (response, error) => {
				
				Assert.IsNull(error);
				Assert.AreEqual (WebStatusCode.NotFound.Raw(), (response.Code));
				didReceiveResponse = true;

			});

			client.BroadcastTask.Wait ();

			Assert.True (didReceiveResponse);
			didReceiveResponse = false;

			dynamic payload = new R2Dynamic ();
			payload.Input = 10;

			TCPMessage request = new TCPMessage () {
			
				Payload = payload,
				Destination = "/dummy"
			};

			ep.MessingUp = new Func<INetworkMessage, INetworkMessage> (msg => {

				dynamic returnPayload = new R2Dynamic();
				returnPayload.PaybackTime = msg.Payload.Input * 42;

				return new TCPMessage() {Code = 4242, Payload = returnPayload};

			});

			guid = client.Broadcast (request, (response, error) => {

				Assert.IsNull(error);
				Assert.AreEqual (4242, response.Code);
				Assert.AreEqual (420, response.Payload.PaybackTime);
				didReceiveResponse = true;
			
			});

			client.BroadcastTask.Wait ();

			Assert.True (didReceiveResponse);

			//TCPMessage request = new TCPMessage() {


		}

		[Test]
		public void TestHostManager() {

			// Set up tcp-server
			var path = "/devices";
			var tcps = factory.CreateTcpServer ("tcp_server", tcp_port);
			var udps = (UDPServer) factory.CreateUdpServer ("udp_server", udp_port);
			udps.AllowLocalRequests = true;

			tcps.Start ();
			udps.Start ();

			DeviceRouter deviceRouter = (DeviceRouter) factory.CreateDeviceObjectReceiver ();
			deviceRouter.AddDevice (tcps);
			var dummy = new DummyDevice ("dummyX");
			deviceRouter.AddDevice (dummy);
			deviceRouter.AddDevice (m_deviceManager);
			m_deviceManager.Add (dummy);
			m_deviceManager.Add (tcps);

			var endpoint = factory.CreateJsonEndpoint (path, deviceRouter);
			udps.AddEndpoint (endpoint);
			tcps.AddEndpoint (endpoint);

			Thread.Sleep (500);

			DeviceManager remoteDeviceManager = new DeviceManager ("remote_dm");

			HostManager h = factory.CreateHostManager ("h", udp_port, path, remoteDeviceManager);

			h.Broadcast ("tcp_server");

			Thread.Sleep (1000);

			Log.t (((UDPBroadcaster) h.Broadcaster).BroadcastTask.Exception?.StackTrace);

			Thread.Sleep (1000);

			dynamic d = remoteDeviceManager.Get ("dummyX");

			Assert.IsTrue (d is IRemoteDevice);
			Assert.AreEqual (420, d.MultiplyByTen (42));

			d.HAHA = 42.42d;

			Assert.AreEqual (dummy.HAHA, d.HAHA);

			udps.Stop ();
			tcps.Stop ();

		}

		[Test]
		public void TestMultipleBroadcasts() {

			var s = factory.CreateUdpServer ("s", udp_port);
			DummyEndpoint ep = new DummyEndpoint ("/dummy");
			UDPBroadcaster client = factory.CreateUdpClient ("c", udp_port);

			(s as UDPServer).AllowLocalRequests = true;

			s.AddEndpoint (ep);

			s.Start ();
			Thread.Sleep (100);
			Assert.True (s.Ready);

			bool didReceiveResponse = false;

			for (int i = 0; i < 6; i++) {
			
				var guid = client.Broadcast (new TCPMessage () { Destination = "should not be found" }, (response, error) => {

					Assert.IsNull(error);
					Assert.AreEqual (WebStatusCode.NotFound.Raw(), (response.Code));
					didReceiveResponse = true;

				}, 500);

				client.BroadcastTask.Wait ();

				Assert.True (didReceiveResponse);
				didReceiveResponse = false;

			}

		}
	}
}

