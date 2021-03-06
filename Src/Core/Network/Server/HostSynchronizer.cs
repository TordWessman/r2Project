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
using R2Core.Device;
using MessageIdType = System.String;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Net;

namespace R2Core.Network
{
	/// <summary>
	/// Very specialized class.
	/// Keeps track of remote hosts by polling for TCP-servers available in remote UDP server. Will connect and synchronize devices with 
	/// the current instance of IDeviceManager and thus adding the remote devices to the IDeviceManager.
	/// </summary>
	public class HostSynchronizer : DeviceBase {
		
		// Used for polling(UDP-broadcasting) for connections/devices. 
		private Timer m_synchronizationTimer;

		// Used to create necessary network related instances
		private WebFactory m_factory;

		// The client used to send broadcast requests
		private INetworkBroadcaster m_broadcaster;

		// Contains the latest broadcast message id.
		private MessageIdType m_messageId;

		// Responsible of keeping track of devices.
		private IDeviceManager m_deviceManager;

		// Contains a list of all hosts ever connected to.
		private IDictionary<string, IClientConnection> m_hosts;

		// Capture the retrie counts for each connection.
		private IDictionary<string, int> m_retries;

		// Broadcast port.
		private int m_port;

		/// <summary>
		/// The identifier for the remote TCP Server instance(i.e. "tcp_server")
		/// </summary>
		public string TCPServerIdentifier = Settings.Identifiers.TcpServer();

		/// <summary>
		/// Returns all available connections.
		/// </summary>
		/// <value>The connections.</value>
		public IEnumerable<IClientConnection> Connections { get { return m_hosts.Values; } }

		/// <summary>
		/// The port on which this broadcaster sends messages(remote UDP servers must be listening on this port).
		/// </summary>
		/// <value>The broadcast port.</value>
		public int BroadcastPort { get { return m_port; } }

		/// <summary>
		/// The timeout for any broadcast responses.
		/// </summary>
		public int BroadcastTimeout = 5000;

		/// <summary>
		/// Number of retries before a previously established connection is removed.
		/// </summary>
		public int MaxRetryCount = 3;

		/// <summary>
		/// Contains the remote destination(i.e. "/devices")
		/// </summary>
		public string Destination = Settings.Consts.DeviceDestination();

		/// <summary>
		/// Interval between broadcast events.
		/// </summary>
		public double SynchronizationInterval {
			
			set {

				m_synchronizationTimer.Interval = value;
				ResetSynchronizationTimer();
				m_synchronizationTimer?.Start();

			}

			get { return m_synchronizationTimer.Interval; }

		}

		/// <summary>
		/// Returns the broadcast client used.
		/// </summary>
		/// <value>The broadcaster.</value>
		public INetworkBroadcaster Broadcaster { get { return m_broadcaster; } }

		/// <summary>
		/// `port` is the UDP port the remote UDP server is listening on.
		/// `deviceManager` is responsible for keeping track of all devices...
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="port">Port.</param>
		/// <param name="deviceManager">DeviceManager.</param>
		/// <param name="factory">Factory.</param>
		public HostSynchronizer(string id, int port, IDeviceManager deviceManager, WebFactory factory) : base(id) {

			m_port = port;
			m_factory = factory;
			m_broadcaster = factory.CreateUdpClient(Settings.Identifiers.UdpBroadcaster(), port);
			m_deviceManager = deviceManager;
			m_hosts = new Dictionary<string, IClientConnection>();
			m_retries = new Dictionary<string, int>();
			ResetSynchronizationTimer();

		}

		~HostSynchronizer() { 
        
            Stop();
            m_synchronizationTimer?.Dispose();
    
        }

		/// <summary>
		/// Broadcast for TCPServers. Will try to establish a connection to any server found and synchronize their devices.
		/// </summary>
		public void Broadcast() {
			
			DeviceRequest request = new DeviceRequest() {
				Identifier = TCPServerIdentifier,
				ActionType = DeviceRequest.ObjectActionType.Get
			};

			INetworkMessage message = new NetworkMessage { Payload = request, Destination = Settings.Consts.DeviceDestination() };

			m_messageId = m_broadcaster.Broadcast(message, (response, address, exception) => {

				if (NetworkStatusCode.Ok.Is(response?.Code)) {

					HandleBroadcastResponse(response, address);

				} else if (exception != null) {
					
					Log.w($"Broadcast message to `{message.Destination}` resulted in exception({exception}), message: `{exception.Message}` (response code '{response?.Code}').", Identifier);
					Log.x(exception, Identifier); 

				} else {

					if (response?.Code != NetworkStatusCode.SameOrigin.Raw()) {
							
						Log.w($"Broadcast received from {response?.Destination} got response code '{response?.Code}'.", Identifier);
				
					}

				}

			}, BroadcastTimeout * 1);

		}

		/// <summary>
		/// Fires of a broadcast and initiate the perpetual broadcaster
		/// </summary>
		public override void Start() {

			m_synchronizationTimer?.Start();

		}

		/// <summary>
		/// Stop the broadcast timer and disconnect all hosts.
		/// </summary>
		public override void Stop() {

			m_synchronizationTimer?.Stop();

			m_hosts.Values.All(host => {
				
				host.Stop();
				return true;
			
			});

			m_hosts = new Dictionary<string, IClientConnection>();
			m_retries = new Dictionary<string, int>();

        }

		/// <summary>
		/// Manually connect to a given address and port
		/// </summary>
		/// <param name="address">Address.</param>
		/// <param name="port">Port.</param>
		public IClientConnection Synchronize(string address, int port) {

			IClientConnection connection = CreateConnection(address, port);

			if (Connect(connection)) {
			
				SynchronizeDevices(connection);
				return connection;
			
			}

			return null;

		}

		/// <summary>
		/// Tells all connected IClientConnection's to connect and synchronize with me.
		/// Returns true if all remote IClientConnection's did synchronize successfully.
		/// </summary>
		/// <returns><c>true</c>, if all remote host was synchronize, <c>false</c> otherwise.</returns>
		/// <param name="deviceServer">Device server.</param>
		public bool ReversedSynchronization(IServer deviceServer) {

			bool success = true;

			foreach (IClientConnection host in m_hosts.Values) {
			
				dynamic remoteHostSynchronizer = new RemoteDevice(Settings.Identifiers.HostSynchronizer(), Guid.Empty, host);
				bool s = remoteHostSynchronizer.RequestSynchronization(deviceServer.Addresses, deviceServer.Port) ?? false;
				Log.d($"Synchronization for {host.Address} succeeded: {s}", Identifier); 
				success &= s;

			}

			return success;

		}

		/// <summary>
		/// Evaluates a list of addresses and tries to connect to the first one available.
		/// This method is typically called by remote, already connected IClientConnection connected to this HostSynchronizer. 
		/// The other ´host is then allowed to make ´self´ to connect to ´host´ as if it was localy requested by ´self´.
		/// Return false if none of the provided ´addresses´ replied on a ping.
		/// </summary>
		/// <returns><c>true</c>, if the remote host was found, <c>false</c> otherwise.</returns>
		/// <param name="addresses">A list of addresses to connect to.</param>
		/// <param name="port">Port.</param>
		public bool RequestSynchronization(IEnumerable<string> addresses, int port) {

			string address = NetworkExtensions.GetAvailableAddress(addresses);

			if (address == null) {
			
				string addressList = String.Join(",", addresses);
				Log.e($"Unable to retrieve address from address list: {addressList}. (Requested port: {port}).", Identifier);
				return false;

			} else {
			
				Synchronize(address, port);
				return true;

			}

		}

		/// <summary>
		/// Creates the connection to the remote host. Will add a connection to m_host if none was found.
		/// </summary>
		/// <returns>The connection.</returns>
		/// <param name="address">Address.</param>
		/// <param name="port">Port.</param>
		private IClientConnection CreateConnection(string address, int port) {

			string id = $"host{address}:{port}";

			IClientConnection connection = m_hosts.ContainsKey(id) ? m_hosts[id] : null;

			if (connection == null) {

				connection = new HostConnection(id, m_factory.CreateTcpClient($"tcp_client_{address}:{port}", address, port));

				m_hosts[id] = connection;

			}

			return connection;

		}

		/// <summary>
		/// Timer callback. Will fire a Broadcast and try to 
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="e">E.</param>
		private void OnConnectionSynchronizerEvent(object source, ElapsedEventArgs e) {

			IList<IClientConnection> failedConnections = new List<IClientConnection>();
			m_hosts.Values.AsParallel().ForAll((host) => {

				if (!host.Ready && host.Ping()) { 

					if (!Connect(host)) { 

						if (m_retries[host.Identifier] > MaxRetryCount) {

							Log.w($"Unable to establish connection to '{host}'. Will remove from list.", Identifier);
							failedConnections.Add(host); 

						}

					}

				}

			});

			foreach (IClientConnection connection in failedConnections) {
			
				if (m_hosts.ContainsKey(connection.Identifier)) { m_hosts.Remove(connection.Identifier); }
				if (m_retries.ContainsKey(connection.Identifier)) { m_retries.Remove(connection.Identifier); }

			}

			Broadcast();

		}

		/// <summary>
		/// Will fetch the devices from host ´connection´ and synchronize them with the devices in ´m_deviceManager´.
		/// </summary>
		/// <param name="connection">Connection.</param>
		private void SynchronizeDevices(IClientConnection connection) {

			DeviceRequest deviceManagerRequest = new DeviceRequest {
				Identifier = Settings.Identifiers.DeviceManager(),
				ActionType = DeviceRequest.ObjectActionType.Get
			};

			INetworkMessage message = new TCPMessage { Destination = Destination, Payload = deviceManagerRequest };

			INetworkMessage response = connection.Send(message);

			if (!NetworkStatusCode.Ok.Is(response.Code)) {
			
				Log.e($"Unable to synchronize. Error from client: {response}.", Identifier);

			} else {
			
				// Retrieve a DeviceResponse which should contain the device manager in the Object property.
				DeviceResponse deviceResponse = new DeviceResponse(response.Payload);

				// Create IRemoteDevice for each device and add it to device manager.
				foreach (dynamic device in deviceResponse.Object.LocalDevices) {

					Guid guid = Guid.Parse((string)device.Guid);

					IRemoteDevice remote = new RemoteDevice(device.Identifier, guid, connection);

					if (m_deviceManager.Has(device.Identifier) && 
						m_deviceManager.Get(device.Identifier) is RemoteDevice &&
						m_deviceManager.Get(device.Identifier).Guid != guid) {

						// Replace any remote device
						m_deviceManager.Remove(device.Identifier);

						m_deviceManager.Add(remote);

					} else if (!m_deviceManager.Has(device.Identifier)) {
					
						m_deviceManager.Add(remote);

					}

				}

			}

		}

		private void ResetSynchronizationTimer() {

            if (m_synchronizationTimer?.Enabled == true) {

                m_synchronizationTimer?.Stop();
                m_synchronizationTimer?.Dispose();

            }

			m_synchronizationTimer = new Timer(m_synchronizationTimer?.Interval ?? Settings.Consts.BroadcastInterval());
			m_synchronizationTimer.Elapsed += new ElapsedEventHandler(OnConnectionSynchronizerEvent);

		}

		private void HandleBroadcastResponse(dynamic response, string address) {

			DeviceResponse deviceResponse = new DeviceResponse(response?.Payload);
			dynamic endpoint = deviceResponse.Object;

			if (endpoint == null) {

				Log.e($"Did not receive an Endpoint in response from ´{response.GetBroadcastAddress()}´. Payload: {response?.Payload}", Identifier);

			} else {

                int port = (int)endpoint.Port;

				if (address != null) {

					Synchronize(address, port);

				} else {

					string addressList = String.Join(",", endpoint.Addresses);
					Log.e($"Unable to connect to host(port {port}). No address in list ´{addressList}´replied on ping.", Identifier);

				}

			}

		}

		/// <summary>
		/// Handles the establishment of a connection. If it fails, the retry counter is increased.
		/// Returns true if the connection was established successfully
		/// </summary>
		/// <param name="connection">Connection.</param>
		private bool Connect(IClientConnection connection) {

			try {

				connection.Start();
				m_retries[connection.Identifier] = 0;
				return true;

			} catch (Exception ex) {

				if (m_retries.ContainsKey(connection.Identifier)) {

					Log.w($"Connection failed ({ex.Message}).", Identifier);
					m_retries[connection.Identifier]++;

				}

			}

			return false;

		}

	}

}