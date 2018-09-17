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

	//Represent an "instance" of a remote system (with servers, devices etc)
	struct ServerInstanceContainer {

		public IServer UDPServer;
		public IServer TCPServer;
		public IDeviceManager DeviceManager;
		public DeviceRouter DeviceRouter;
		public HostManager HostManager;

		public void Start() {
			UDPServer.Start ();
			TCPServer.Start ();
		}

		public void Stop() {
			UDPServer.Stop ();
			TCPServer.Stop ();
			HostManager.Stop ();

		}
	}

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
		public void TestMultipleBroadcasts() {

			var s = factory.CreateUdpServer ("s", udp_port + 999);
			DummyEndpoint ep = new DummyEndpoint ("/dummy");
			UDPBroadcaster client = factory.CreateUdpClient ("c", udp_port + 999);

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

				}, 2000);

				client.BroadcastTask.Wait ();

				Assert.True (didReceiveResponse);
				didReceiveResponse = false;

			}

		}

		[Test]
		public void TestHostManager() {

			var devices1 = SetUpServers (tcp_port, udp_port, udp_port);

			var dummy = new DummyDevice ("dummyX");
			devices1.DeviceRouter.AddDevice (dummy);
			devices1.DeviceManager.Add (dummy);

			Thread.Sleep (500);

			DeviceManager remoteDeviceManager = new DeviceManager ("remote_dm");

			HostManager h = factory.CreateHostManager ("h", udp_port, remoteDeviceManager);

			h.Broadcast ();

			Thread.Sleep (1000);

			Log.t (((UDPBroadcaster) h.Broadcaster).BroadcastTask.Exception?.StackTrace);

			Thread.Sleep (1000);

			dynamic d = remoteDeviceManager.Get ("dummyX");

			Assert.IsTrue (d is IRemoteDevice);
			// TODO: This times out sometimes. Too many other Tasks?
			Assert.AreEqual (420, d.MultiplyByTen (42));

			d.HAHA = 42.42d;

			Assert.AreEqual (dummy.HAHA, d.HAHA);

			devices1.TCPServer.Stop ();
			devices1.UDPServer.Stop ();

		}

		private ServerInstanceContainer SetUpServers(int tcpPort, int udpPort, int remoteUdpPort) {

			var path =  Settings.Consts.DeviceDestination();

			IServer tcps = factory.CreateTcpServer (Settings.Identifiers.TcpServer(), tcpPort);
			IServer udps = factory.CreateUdpServer ("udp_server", udpPort);
			((UDPServer) udps).AllowLocalRequests = true;

			// Will keep track of the available devices for this "instance" 
			DeviceManager deviceManager = new DeviceManager (Settings.Identifiers.DeviceManager ());

			DeviceRouter deviceRouter = (DeviceRouter) factory.CreateDeviceRouter ();
			deviceRouter.AddDevice (tcps);
			deviceRouter.AddDevice (deviceManager);

			//deviceManager.Add (tcps);

			var endpoint = factory.CreateJsonEndpoint (path, deviceRouter);
			udps.AddEndpoint (endpoint);
			tcps.AddEndpoint (endpoint);

			HostManager hostManager = factory.CreateHostManager ("host_manager", remoteUdpPort, deviceManager);

			hostManager.BroadcastInterval = 2000;

			return new ServerInstanceContainer () {
				UDPServer = udps,
				TCPServer = tcps,
				DeviceManager = deviceManager,
				DeviceRouter = deviceRouter,
				HostManager = hostManager
			};

		}

		/// <summary>
		/// Test the synchronization of devices between two HostManager instances (and thus two tcp & udp servers)
		/// </summary>
		[Test]
		public void TestHostManager_DeviceSynchronization() {

			var devices1 = SetUpServers (tcp_port, udp_port, udp_port - 42);
			var devices2 = SetUpServers (tcp_port - 42, udp_port - 42, udp_port);

			var dummy1 = new DummyDevice ("dummy1");
			devices1.DeviceRouter.AddDevice (dummy1);
			devices1.DeviceManager.Add (dummy1);

			var dummy2 = new DummyDevice ("dummy2");
			devices2.DeviceRouter.AddDevice (dummy2);
			devices2.DeviceManager.Add (dummy2);

			devices2.Start ();
			devices1.Start ();
			Thread.Sleep (1000);

			//devices1.HostManager.Broadcast ();
			devices1.HostManager.Start ();
			devices2.HostManager.Start ();

			// Sleep enough time for both host managers to be synchronized
			Thread.Sleep (4000);

			Assert.NotNull (devices1.DeviceManager.Get ("dummy2"));
			Assert.NotNull (devices2.DeviceManager.Get ("dummy1"));

			devices2.Stop ();
			devices1.Stop ();

		}
	}
}

