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
using R2Core.Network;
using R2Core.Data;
using System.Threading.Tasks;
using R2Core.Device;
using System.Collections.Generic;

namespace R2Core.Tests
{


	[TestFixture]
	public class UDPTests : NetworkTests {

		const int tcp_port = 4444;
		const int udp_port = 4445;

		[Test]
		public void TestSendReceive() {
			PrintName();

			var s = factory.CreateUdpServer("s", udp_port);
			DummyEndpoint ep = new DummyEndpoint("/dummy");
			UDPBroadcaster client = factory.CreateUdpClient("c", udp_port);

			(s as UDPServer).AllowLocalRequests = true;

			s.AddEndpoint(ep);

			s.Start();
			Thread.Sleep(100);
			Assert.True(s.Ready);
			s.Stop();
			Thread.Sleep(100);
			Assert.False(s.Ready);
			s.Start();
			Thread.Sleep(100);
			Assert.True(s.Ready);

			bool didReceiveResponse = false;

			var guid = client.Broadcast(new TCPMessage() { Destination = "should not be found" }, (response, error) => {
				
				Assert.IsNull(error);
				Assert.AreEqual(NetworkStatusCode.NotFound.Raw(), (response.Code));
				didReceiveResponse = true;

			});

			client.BroadcastTask.Wait();

			Assert.True(didReceiveResponse);
			didReceiveResponse = false;

			dynamic payload = new R2Dynamic();
			payload.Input = 10;

			TCPMessage request = new TCPMessage() {
			
				Payload = payload,
				Destination = "/dummy"
			};

			ep.MessingUp = new Func<INetworkMessage, INetworkMessage>(msg => {

				dynamic returnPayload = new R2Dynamic();
				returnPayload.PaybackTime = msg.Payload.Input * 42;

				return new TCPMessage() {Code = 4242, Payload = returnPayload};

			});

			guid = client.Broadcast(request, (response, error) => {

				Assert.IsNull(error);
				Assert.AreEqual(4242, response.Code);
				Assert.AreEqual(420, response.Payload.PaybackTime);
				didReceiveResponse = true;
			
			});

			client.BroadcastTask.Wait();

			Assert.True(didReceiveResponse);

			client.Stop();
			s.Stop();

		}

		[Test]
		public void TestMultipleBroadcasts() {
			PrintName();

			var s = factory.CreateUdpServer("s", udp_port + 999);
			DummyEndpoint ep = new DummyEndpoint("/dummy");
			UDPBroadcaster client = factory.CreateUdpClient("c", udp_port + 999);

			(s as UDPServer).AllowLocalRequests = true;

			s.AddEndpoint(ep);

			s.Start();
			Thread.Sleep(100);
			Assert.True(s.Ready);

			bool didReceiveResponse = false;

			for (int i = 0; i < 6; i++) {
			
				var guid = client.Broadcast(new TCPMessage() { Destination = "should not be found" }, (response, error) => {

					Assert.IsNull(error);
					Assert.AreEqual(NetworkStatusCode.NotFound.Raw(), (response.Code));
					didReceiveResponse = true;

				}, 2000);

				client.BroadcastTask.Wait();

				Assert.True(didReceiveResponse);
				didReceiveResponse = false;

			}

			s.Stop();
			client.Stop();

		}

		[Test]
		public void TestHostSynchronizer() {
			PrintName();

			var udpp = udp_port - 51;
			var tcpp = tcp_port - 50;

			var devices1 = factory.SetUpServers(tcpp, udpp, udpp);

			var dummy = new DummyDevice("dummyX");
			devices1.DeviceManager.Add(dummy);
			devices1.Start();
			Thread.Sleep(500);

			// Represent the remote host which want to retrieve device "dummyX"
			DeviceManager remoteDeviceManager = new DeviceManager(Settings.Identifiers.DeviceManager());
			HostSynchronizer h = factory.CreateHostSynchronizer("h", udpp, remoteDeviceManager);

			// Remote host: I wan't your devices
			h.Broadcast();

			// Wait in order for the broadcast task to finish 
			Thread.Sleep(2000);

			Thread.Sleep(2000);

			dynamic d = remoteDeviceManager.Get("dummyX");

			Assert.IsTrue(d is IRemoteDevice);
			// TODO: This times out sometimes. Too many other Tasks?
			Assert.AreEqual(420, d.MultiplyByTen(42));

			d.HAHA = 42.42d;

			Assert.AreEqual(dummy.HAHA, d.HAHA);

			h.Stop();
			devices1.Stop();

		}


	}
}

