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
using R2Core.Network;
using R2Core.Device;

namespace R2Core.Tests
{
	/// <summary>
	/// Represent an "instance" of a remote system(with servers, devices etc)
	/// </summary>
	public struct ServerInstanceContainer
	{

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

	public static class WebFactoryExtensions {
	
		public static ServerInstanceContainer SetUpServers(this WebFactory factory, int tcpPort, int udpPort, int remoteUdpPort) {

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

	}
}

