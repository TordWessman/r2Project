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
using R2Core.Device;
using R2Core.Scripting;
using R2Core.Common;
using System.Linq;

namespace R2Core.Tests
{
	[TestFixture]
	public class HostSynchronizerTests : NetworkTests
	{
		const int tcp_port = 4444;
		const int udp_port = 4445;

		/// <summary>
		/// Test the synchronization of devices between two HostSynchronizer instances(and thus two tcp & udp servers)
		/// </summary>
		[Test]
		public void NoLocalDuplicates() {
			PrintName();

			var devices1 = factory.SetUpServers(tcp_port, udp_port, udp_port - 43);
			var devices2 = factory.SetUpServers(tcp_port - 43, udp_port - 43, udp_port);

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
			Thread.Sleep(((int)devices1.HostSynchronizer.SynchronizationInterval) * 3);

			devices2.Stop();
			devices1.Stop();
			Thread.Sleep(500);

		}

		/// <summary>
		/// Test if the HostSynchronizer's ´RequestSynchronization´ & ´Synchronize´ works.
		/// </summary>
		[Test]
		public void ManualSynchronization() {
			PrintName();

			int remoteTCPPort = tcp_port - 40;
			int localTCPPort = tcp_port - 41;
			// Set up remote. It can use it's broadcast, but that's not what to test 
			var remote = factory.SetUpServers(remoteTCPPort, udp_port, udp_port - 44);
			var remoteDummy = new DummyDevice("remoteDummy");

			var pythonScriptFactory = new PythonScriptFactory("rf", Settings.Instance.GetPythonPaths(), m_deviceManager);
			pythonScriptFactory.AddSourcePath(Settings.Paths.TestData());
			pythonScriptFactory.AddSourcePath(Settings.Paths.Common());

			var remoteScript = pythonScriptFactory.CreateScript("python_test");
			remote.DeviceManager.Add(remoteScript);

			remote.DeviceManager.Add(remoteDummy);
			remote.Start();

			// Setup local instance. We'll only need DeviceManager and TCPServer, though
			var local = factory.SetUpServers(localTCPPort, udp_port, udp_port - 44);
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
			Assert.IsTrue(local.DeviceManager.Has("python_test"));

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
			Thread.Sleep(500);

		}

		/// <summary>
		/// Test the synchronization of devices between two HostSynchronizer instances(and thus two tcp & udp servers)
		/// </summary>
		[Test]
		public void BroadcastSynchronization() {
			PrintName();

			var devices1 = factory.SetUpServers(tcp_port, udp_port, udp_port - 42);
			var devices2 = factory.SetUpServers(tcp_port - 42, udp_port - 42, udp_port);

			var dummy1 = new DummyDevice("dummy1");
			devices1.DeviceManager.Add(dummy1);

			var dummy2 = new DummyDevice("dummy2");
			devices2.DeviceManager.Add(dummy2);

			devices2.Start();
			devices1.Start();
			Thread.Sleep(1000);

			devices1.HostSynchronizer.Start();
			devices2.HostSynchronizer.Start();

			devices1.HostSynchronizer.SynchronizationInterval = 2000;
			// Sleep enough time for both host managers to be synchronized
			Thread.Sleep(((int)devices1.HostSynchronizer.SynchronizationInterval) * 3);

			// The device should be available
			Assert.NotNull(devices1.DeviceManager.Get("dummy2"));
			Assert.NotNull(devices2.DeviceManager.Get("dummy1"));

			// Change the value of a property @ instance 1
			dummy1.Bar = "KATT";

			// Add a new device to instance 2
			var dummy3 = new DummyDevice("dummy3");
			devices2.DeviceManager.Add(dummy3);

			// They should be synchronized after this...
			Thread.Sleep(((int)devices1.HostSynchronizer.SynchronizationInterval) * 3);

			// The new device should be available
			Assert.NotNull(devices1.DeviceManager.Get("dummy3"));

			// Check the changed property of dummy1
			dynamic dummy1_remote = devices2.DeviceManager.Get("dummy1");

			Assert.AreEqual("KATT", dummy1_remote.Bar);

			devices2.Stop();
			devices1.Stop();
			Thread.Sleep(500);

		}

		/// <summary>
		/// Test the failing connections
		/// </summary>
		[Test]
		public void FailedConnections() {
			PrintName();
			var devices1 = factory.SetUpServers(tcp_port - 151, udp_port, udp_port - 152);
			var devices2 = factory.SetUpServers(tcp_port - 152, udp_port - 152, udp_port);
			devices2.Start();
			devices1.Start();
			Thread.Sleep(200);
			devices1.HostSynchronizer.Synchronize("localhost", tcp_port - 152);
			devices1.HostSynchronizer.MaxRetryCount = 3;
			Assert.AreEqual(1, devices1.HostSynchronizer.Connections.Count());
			devices2.Stop();
			Thread.Sleep(100);
			devices1.HostSynchronizer.SynchronizationInterval = 250;
			Thread.Sleep(500);
			Assert.False(devices2.TCPServer.Ready);
			Assert.AreEqual(1, devices1.HostSynchronizer.Connections.Count()); // should still be in the retry-list
			Thread.Sleep(750);
			Assert.AreEqual(0, devices1.HostSynchronizer.Connections.Count()); // now it should have been removed
			
		}

		/// <summary>
		/// Test if the host synchronizer can reconnect to failed TcpConnections
		/// </summary>
		[Test]
		public void Reconnection() {
			PrintName();

			var devices1 = factory.SetUpServers(tcp_port - 141, udp_port, udp_port - 142);
			var devices2 = factory.SetUpServers(tcp_port - 142, udp_port - 142, udp_port);
			devices2.Start();
			devices1.Start();
			Thread.Sleep(200);
			devices1.HostSynchronizer.SynchronizationInterval = 1000;
			devices1.HostSynchronizer.Start();
			Thread.Sleep(200);
			devices2.HostSynchronizer.SynchronizationInterval = 1000;
			devices2.HostSynchronizer.Start();
			Thread.Sleep(5000);

			// By now, the connections should be established.
			Assert.AreEqual(1, devices1.HostSynchronizer.Connections.Count());
			Assert.AreEqual(1, devices2.HostSynchronizer.Connections.Count());

			foreach (IClientConnection connection in devices1.HostSynchronizer.Connections) {

				Assert.True(connection.Ready);

			}

			devices1.Stop();
			devices2.Stop();

		}

	}
}

