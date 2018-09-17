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
using R2Core.Device;
using R2Core.Network;
using System.Threading.Tasks;
using System.Net;
using R2Core.Data;
using MessageIdType = System.String;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace R2Core.Network
{
	/// <summary>
	/// Very specialized class.
	/// Keeps track of remote hosts by polling for TCP-servers available in remote UDP server. Will connect and synchronize devices with 
	/// the current instance of IDeviceManager and thus adding the remote devices to the IDeviceManager.
	/// </summary>
	public class HostManager : DeviceBase
	{

		// Used for polling (UDP-broadcasting) for connections/devices. 
		private Timer m_broadcastTimer;

		// Used to create nessecary stuff
		private WebFactory m_factory;

		// The client used to send broadcast requests
		private INetworkBroadcaster m_broadcaster;

		// Contains the latest broadcast message id.
		private MessageIdType m_messageId;

		// Responsible of keeping track of devices.
		private IDeviceManager m_deviceManager;

		// Contains a list of all hosts ever connected to.
		private IList<IHostConnection> m_hosts;

		// Broadcast port.
		private int m_port;

		// Interval for calling Broadcast.
		private int m_broadcastInterval = 10000;

		/// <summary>
		/// Contains the remote destination (i.e. "/devices")
		/// </summary>
		public string Destination = Settings.Consts.DeviceDestination();

		/// <summary>
		/// The identifier for the remote TCP Server instance (i.e. "tcp_server")
		/// </summary>
		public string TCPServerIdentifier = Settings.Identifiers.TcpServer();

		/// <summary>
		/// The port on which this broadcaster sends messages (remote UDP servers must be listening on this port).
		/// </summary>
		/// <value>The broadcast port.</value>
		public int BroadcastPort { get { return m_port; } }

		/// <summary>
		/// The timeout for any broadcast responses.
		/// </summary>
		public int BroadcastTimeout = 2000;


		/// <summary>
		/// Interval between broadcast events.
		/// </summary>
		public int BroadcastInterval {
			
			set {
			
				m_broadcastInterval = value;
				m_broadcastTimer.Interval = value;
			
			}

			get {
			
				return m_broadcastInterval;

			}

		}

		/// <summary>
		/// Returns the broadcast client used.
		/// </summary>
		/// <value>The broadcaster.</value>
		public INetworkBroadcaster Broadcaster { get { return m_broadcaster; } }

		/// <summary>
		/// `port` is the port the remote host is listening on. 
		/// `Destination` is the path (i.e. /devices) where the broadcast listener expects requests from.
		/// `deviceManager` is responsible for keeping track of all devices...
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="port">Port.</param>
		/// <param name="deviceManager">DeviceManager.</param>
		/// <param name="factory">Factory.</param>
		public HostManager (string id, int port, IDeviceManager deviceManager, WebFactory factory) : base (id) {

			m_port = port;
			m_factory = factory;
			m_broadcaster = factory.CreateUdpClient("udp_broadcaster", port);
			m_deviceManager = deviceManager;
			m_hosts = new List<IHostConnection> ();
			m_broadcastTimer = new Timer (BroadcastInterval);
			m_broadcastTimer.Elapsed += new ElapsedEventHandler(OnBroadcastEvent);
			m_broadcastTimer.Enabled = true;

		}

		/// <summary>
		/// Broadcast for TCPServers. Will try to establish a connection to any server found and synchronize their devices.
		/// </summary>
		public void Broadcast () {
			
			DeviceRequest request = new DeviceRequest () {
				Identifier = TCPServerIdentifier,
				ActionType = DeviceRequest.ObjectActionType.Get
			};

			INetworkMessage message = new NetworkMessage () { Payload = request, Destination = Destination };

			m_messageId = m_broadcaster.Broadcast(message, (response, exception) => {

				if (WebStatusCode.Ok.Is(response?.Code)) {
					
					DeviceResponse deviceResponse = new DeviceResponse(response?.Payload);
					dynamic endpoint = deviceResponse.Object;

					if (endpoint == null) {

						Log.e($"Did not receive an Endpoint in response from {response.Origin.ToString()}. Payload: {response?.Payload}");

					} else {

						string address = (string) endpoint.Address;
						int port = (int) endpoint.Port;

						IHostConnection connection = EstablishConnection(address, port);

						SynchronizeDevices(connection);

					}

				} else if (exception != null) {
					
					Log.w ($"Broadcast message to `{message.Destination}` resulted in exception ({exception}), message: `{exception.Message}` (response code '{response?.Code}'). ");
					Log.x(exception); 

				} else {

					if (response?.Code != WebStatusCode.SameOrigin.Raw()) {
							
						Log.w ($"Broadcast received from {response?.Destination} got response code '{response?.Code}'.");
				
					}

				}

			}, BroadcastTimeout * 1);

		}

		/// <summary>
		/// Fires of a broadcast and initiate the perpetual broadcaster
		/// </summary>
		public override void Start () {

			Broadcast ();
			m_broadcastTimer.Start ();

		}

		/// <summary>
		/// Stop the broadcast timer and disconnect all hosts.
		/// </summary>
		public override void Stop () {

			m_broadcastTimer.Stop ();
			m_hosts.All ( host => {
				host.Stop();
				return true;

			});

		}

		/// <summary>
		/// Establishs the connection to the remote host. Will add a connection to m_host if none was found. Otherwise it will try to reconnect if disconnected.
		/// </summary>
		/// <returns>The connection.</returns>
		/// <param name="address">Address.</param>
		/// <param name="port">Port.</param>
		private IHostConnection EstablishConnection(string address, int port) {

			string id = $"host{address}:{port}";

			IHostConnection connection = m_hosts.FirstOrDefault( (h) => { return h.Identifier == id; });

			if (connection == null) {

				connection = new HostConnection(id, Destination, 
					m_factory.CreateTcpClient($"tcp_client_{address}:{port}", address, port));

				connection.Start();

				m_hosts.Add(connection);

			} else if (!connection.Ready) {

				connection.Start();

			}

			return connection;

		}

		/// <summary>
		/// Timer callback. Will fire a Broadcast
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="e">E.</param>
		private void OnBroadcastEvent(object source, ElapsedEventArgs e) {

			Broadcast ();

		}

		/// <summary>
		/// Will fetch the devices from host ´connection´ and synchronize them with the devices in ´m_deviceManager´.
		/// </summary>
		/// <param name="connection">Connection.</param>
		private void SynchronizeDevices(IHostConnection connection) {

			DeviceRequest deviceManagerRequest = new DeviceRequest () {
				Identifier = Settings.Identifiers.DeviceManager(),
				ActionType = DeviceRequest.ObjectActionType.Get
			};

			// Retrieve a DeviceResponse which should contain the device manager in the Object property.
			DeviceResponse deviceResponse = new DeviceResponse(connection.Send(deviceManagerRequest));

			// Create IRemoteDevice for each device and add it to device manager.
			foreach (dynamic device in deviceResponse.Object.Devices) {

				Guid guid = Guid.Parse((string) device.Guid);

				// Check if a device with the same Guid exists.
				if (m_deviceManager.GetByGuid<IDevice>(guid) == null) {

					// If not, add the to our (local) device manager
					IRemoteDevice remote = new RemoteDevice(device.Identifier, guid, connection);
					Log.t($"Adding device: {device.Identifier}");

					m_deviceManager.Add(remote);

				} else {

					Log.t($"Duplicate/NOT adding device: {device.Identifier}");

				}

			}

		}

	}

}