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
using Core.Device;
using Core.Network;
using System.Threading.Tasks;
using System.Net;
using Core.Data;

namespace Core.Network
{
	/// <summary>
	/// Binds the network to the DeviceManager
	/// </summary>
	public class HostManager : DeviceBase, IDeviceManagerObserver
	{
		// Used to create nessecary stuff
		private WebFactory m_factory;

		// The client used to send broadcast requests
		private INetworkBroadcaster m_broadcaster;

		// Contains the latest broadcast message id.
		private Guid m_messageId;

		// Responsible of keeping track of devices.
		private IDeviceManager m_devices;

		// Contains the remote destination (i.e. "/devices")
		private string m_destination;

		/// <summary>
		/// The timeout for any broadcast responses.
		/// </summary>
		public int BroadcastTimeout = 2000;

		/// <summary>
		/// Returns the broadcast client used.
		/// </summary>
		/// <value>The broadcaster.</value>
		public INetworkBroadcaster Broadcaster { get { return m_broadcaster; } }

		/// <summary>
		/// `port` is the port the remote host is listening on. 
		/// `destination` is the path (i.e. /devices) where the broadcast listener expects requests from.
		/// `deviceManager` is responsible for keeping track of all devices...
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="port">Port.</param>
		/// <param name="destination">Destination.</param>
		/// <param name="factory">Factory.</param>
		public HostManager (string id, int port, string destination, IDeviceManager deviceManager, WebFactory factory) : base (id)
		{

			m_factory = factory;
			m_destination = destination;
			m_broadcaster = factory.CreateUdpClient("udp_broadcaster", port);
			m_devices = deviceManager;

		}

		/// <summary>
		/// Broadcast the identity of my serverver. This allows receivers to connect. `destination` is the path where the broadcast servers endpoint expects DeviceRequests.
		/// </summary>
		/// <param name="serverId">Server identifier.</param>
		/// <param name="serverPort">Server port.</param>
		/// <param name="destination">Remote server destination path.</param>
		public void Broadcast (string serverId) {

			DeviceRequest request = new DeviceRequest () {
				Identifier = serverId,
				ActionType = DeviceRequest.ObjectActionType.Get
			};

			INetworkMessage message = new NetworkMessage () { Payload = request, Destination = m_destination };

			m_messageId = m_broadcaster.Broadcast(message, BroadcastTimeout, (response, exception) => {

				if (WebStatusCode.Ok.Is(response?.Code)) {

					DeviceResponse deviceResponse = new DeviceResponse(response?.Payload);
					dynamic endpoint = deviceResponse.Object;

					if (endpoint == null) {

						Log.e($"Did not receive an Endpoint in response from {response.Origin.ToString()}. Payload: {response?.Payload}");

					} else {

						string address = (string) endpoint.Address;
						int port = (int) endpoint.Port;
						string id = $"client_{address}:{port}";

						var connection = new NetworkConnection(id, m_destination, 
							m_factory.CreateTcpClient("tcp_client", address, port));

						connection.Start();

						DeviceRequest deviceManagerRequest = new DeviceRequest () {
							Identifier = Settings.Identifiers.DeviceManager(),
							ActionType = DeviceRequest.ObjectActionType.Get
						};

						// Retrieve a DeviceResponse which should contain the device manager in the Object property.
						deviceResponse = new DeviceResponse(connection.Send(deviceManagerRequest));

						// Create IRemoteDevice for each device and add it to device manager.
						foreach (dynamic device in deviceResponse.Object.Devices) {

							Guid guid = Guid.Parse((string) device.Guid);
							IRemoteDevice remote = new RemoteDevice(device.Identifier, guid, connection);

							m_devices.Add(remote);

						}

					}

				} else if (exception != null) {
					
					Log.w ($"Broadcast message to `{message.Destination}` resulted in exception ({exception}), message: `{exception.Message}` (response code '{response?.Code}'). ");
					Log.x(exception); 

				} else {

					if (response?.Code != WebStatusCode.SameOrigin.Raw()) {
							
						Log.w ($"Broadcast received from {response?.Destination} got response code '{response?.Code}'.");
				
					}

				}

			});

		}

		public void DeviceAdded(IDevice device) {
		
		}

		public void DeviceRemoved(IDevice device) {
		

		}

	}

}