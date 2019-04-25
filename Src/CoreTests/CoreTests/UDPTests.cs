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

	//Represent an "instance" of a remote system(with servers, devices etc)
	struct ServerInstanceContainer {

		public IServer UDPServer;
		public IServer TCPServer;
		public IDeviceManager DeviceManager;
		public DeviceRouter DeviceRouter;
		public HostSynchronizer HostSynchronizer;

		public void Start() {
			UDPServer.Start();
			TCPServer.Start();
		}

		public void Stop() {
			UDPServer.Stop();
			TCPServer.Stop();
			HostSynchronizer.Stop();

		}
	}

	[TestFixture]
	public class UDPTests: NetworkTests {

		const int tcp_port = 4444;
		const int udp_port = 4445;


		private ServerInstanceContainer SetUpServers(int tcpPort, int udpPort, int remoteUdpPort) {

			IServer tcps = factory.CreateTcpServer(Settings.Identifiers.TcpServer(), tcpPort);
			IServer udps = factory.CreateUdpServer(Settings.Identifiers.UdpServer(), udpPort);
			((UDPServer) udps).AllowLocalRequests = true;

			// Will keep track of the available devices for this "instance" 
			DeviceManager deviceManager = new DeviceManager(Settings.Identifiers.DeviceManager());

			deviceManager.Add(tcps);
			deviceManager.Add(udps);
			DeviceRouter deviceRouter = (DeviceRouter)factory.CreateDeviceRouter(deviceManager);
			deviceRouter.AddDevice(deviceManager);

			var endpoint = factory.CreateJsonEndpoint(deviceRouter);
			udps.AddEndpoint(endpoint);
			tcps.AddEndpoint(endpoint);

			HostSynchronizer hostManager = factory.CreateHostSynchronizer(Settings.Identifiers.HostSynchronizer(), remoteUdpPort, deviceManager);

			hostManager.BroadcastInterval = 2000;
			deviceRouter.AddDevice(hostManager);

			return new ServerInstanceContainer() {
				UDPServer = udps,
				TCPServer = tcps,
				DeviceManager = deviceManager,
				DeviceRouter = deviceRouter,
				HostSynchronizer = hostManager
			};

		}

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
			//TCPMessage request = new TCPMessage() {


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

			var devices1 = SetUpServers(tcpp, udpp, udpp);

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

			Log.t(((UDPBroadcaster) h.Broadcaster).BroadcastTask.Exception?.StackTrace);

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

		/// <summary>
		/// Test the synchronization of devices between two HostSynchronizer instances(and thus two tcp & udp servers)
		/// </summary>
		[Test]
		public void TestHostSynchronizer_BroadcastSynchronization() {
			PrintName();

			var devices1 = SetUpServers(tcp_port, udp_port, udp_port - 42);
			var devices2 = SetUpServers(tcp_port - 42, udp_port - 42, udp_port);

			var dummy1 = new DummyDevice("dummy1");
			devices1.DeviceManager.Add(dummy1);

			var dummy2 = new DummyDevice("dummy2");
			devices2.DeviceManager.Add(dummy2);

			devices2.Start();
			devices1.Start();
			Thread.Sleep(1000);

			devices1.HostSynchronizer.Start();
			devices2.HostSynchronizer.Start();

			// Sleep enough time for both host managers to be synchronized
			Thread.Sleep(devices1.HostSynchronizer.BroadcastInterval * 3);

			// The device should be available
			Assert.NotNull(devices1.DeviceManager.Get("dummy2"));
			Assert.NotNull(devices2.DeviceManager.Get("dummy1"));

			// Change the value of a property @ instance 1
			dummy1.Bar = "KATT";

			// Add a new device to instance 2
			var dummy3 = new DummyDevice("dummy3");
			devices2.DeviceManager.Add(dummy3);

			// They should be synchronized after this...
			Thread.Sleep(devices1.HostSynchronizer.BroadcastInterval * 3);

			// The new device should be available
			Assert.NotNull(devices1.DeviceManager.Get("dummy3"));

			// Check the changed property of dummy1
			dynamic dummy1_remote = devices2.DeviceManager.Get("dummy1");

			Assert.AreEqual("KATT", dummy1_remote.Bar);

			devices2.Stop();
			devices1.Stop();

		}

		/// <summary>
		/// Test the synchronization of devices between two HostSynchronizer instances(and thus two tcp & udp servers)
		/// </summary>
		[Test]
		public void TestHostSynchronizer_NoLocalDuplicates() {
			PrintName();

			var devices1 = SetUpServers(tcp_port, udp_port, udp_port - 43);
			var devices2 = SetUpServers(tcp_port - 43, udp_port - 43, udp_port);

			var dummy1 = new DummyDevice("dummy");
			devices1.DeviceManager.Add(dummy1);

			var dummy2 = new DummyDevice("dummy");
			devices2.DeviceManager.Add(dummy2);

			devices2.Start();
			devices1.Start();
			Thread.Sleep(1000);

			devices1.HostSynchronizer.Start();
			devices2.HostSynchronizer.Start();

			Assert.True(dummy1.Guid == devices1.DeviceManager.Get("dummy").Guid);
			Assert.True(dummy2.Guid == devices2.DeviceManager.Get("dummy").Guid);

			// Sleep enough time for both host managers to be synchronized
			Thread.Sleep(devices1.HostSynchronizer.BroadcastInterval * 3);
		
			devices2.Stop();
			devices1.Stop();

		}

		/// <summary>
		/// Test if the HostSynchronizer's ´RequestSynchronization´ & ´Synchronize´ works.
		/// </summary>
		[Test]
		public void TestHostSynchronizer_ManualSynchronization() {
			PrintName();

			int remoteTCPPort = tcp_port - 40;
			int localTCPPort = tcp_port - 41;
			// Set up remote. It can use it's broadcast, but that's not what to test 
			var remote = SetUpServers(remoteTCPPort, udp_port, udp_port - 44);
			var remoteDummy = new DummyDevice("remoteDummy");
			remote.DeviceManager.Add(remoteDummy);
			remote.Start();

			// Setup local instance. We'll only need DeviceManager and TCPServer, though
			var local = SetUpServers(localTCPPort, udp_port, udp_port - 44);
			var localDummy = new DummyDevice("localDummy");
			local.DeviceManager.Add(localDummy);

			// Start only local TCPServer. Device synchronization through 2 HostSynchronizer instances is not to test here.
			local.TCPServer.Start();

			Assert.IsFalse(local.DeviceManager.Has("remoteDummy"));
			Assert.IsFalse(remote.DeviceManager.Has("localDummy"));

			Thread.Sleep(200);

			// Manually connect to remote
			IClientConnection remoteHostConnection = local.HostSynchronizer.Synchronize("localhost", remoteTCPPort);

			Thread.Sleep(200);

			Assert.IsTrue(local.DeviceManager.Has("remoteDummy"));

			// Manually create a RemoteDevice representing the remote HostSynchronizer
			dynamic remoteHostSynchronizer = new RemoteDevice(Settings.Identifiers.HostSynchronizer(), Guid.Empty, remoteHostConnection);
			bool success = remoteHostSynchronizer.RequestSynchronization(local.TCPServer.Addresses, localTCPPort);
			Assert.IsTrue(success);
			remoteHostSynchronizer.RequestSynchronization(local.TCPServer.Addresses, localTCPPort);

			Thread.Sleep(200);

			// Remote has connected to ´śelf´ and should have my devices
			Assert.IsTrue(remote.DeviceManager.Has("localDummy"));

			var localDummy2 = new DummyDevice("localDummy2");
			local.DeviceManager.Add(localDummy2);

			success = local.HostSynchronizer.ReversedSynchronization(local.TCPServer);
			Assert.IsTrue(success);

			// Remote has connected to ´śelf´ and should have my devices
			Assert.IsTrue(remote.DeviceManager.Has("localDummy"));
			Assert.IsTrue(remote.DeviceManager.Has("localDummy2"));

			local.TCPServer.Stop();
			remote.Stop();

		}
	}
}

