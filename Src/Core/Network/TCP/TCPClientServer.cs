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
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net;

namespace R2Core.Network
{
	public struct RegistrationRequest {
	
		public string HostName { get; set; }
		public string Address { get; set; }
		public int Port { get; set; }
	}

    public class TCPClientServer : ServerBase {

        private static string AddressKey;
        private static string PortKey;
        private static string ServerTypeKey;

        private string m_address;
        private TcpClient m_client;
        private ITCPPackageFactory<TCPMessage> m_serializer;

		private IDictionary<string,IServer> m_servers;

		// Keeps track of the connectivity of a socket
		private ConnectionPoller m_connectionPoller;

		/// <summary>
		/// Timeout in ms before an operation dies.
		/// </summary>
		public int Timeout = 30000;

        /// <summary>
        /// The identity used to route requests to this server.
        /// The identity is set by calling ´Configure´.
        /// </summary>
        /// <value>The identity.</value>
        public IIdentity Identity { get; private set; }

        /// <summary>
        /// The remote address to the router.
        /// </summary>
        /// <value>The address.</value>
        public string Address { get; private set; }

		public override bool Ready {
			
			get { return base.Ready && (m_client?.IsConnected() == true); }

		}

		public TCPClientServer(string id, ITCPPackageFactory<TCPMessage> serializer) : base(id) {

			m_serializer = serializer;
			m_servers = new Dictionary<string, IServer>();
			AddressKey = Settings.Consts.ConnectionRouterHeaderClientAddressKey();
			PortKey = Settings.Consts.ConnectionRouterHeaderClientPortKey();
			ServerTypeKey = Settings.Consts.ConnectionRouterHeaderServerTypeKey();

		}

        /// <summary>
        /// Set the configuration parameters required for connecting to a remote router.
        /// </summary>
        /// <param name="identity">Identity of this instance.</param>
        /// <param name="address">Address to the router.</param>
        /// <param name="port">Port to the router.</param>
		public void Configure(IIdentity identity, string address, int port) {
			
			Address = address;
            Identity = identity;
            SetPort(port);
		
		}

		public void AddServer(string headerIdentifier, IServer server) {
		
			m_servers[headerIdentifier] = server;
		
		}

		protected override void Cleanup() {

			m_client.Close();
		
		}

		public override void Start() {

			Connect();

		}

		public override INetworkMessage Interpret(INetworkMessage request, System.Net.IPEndPoint source) {

			IWebEndpoint endpoint = GetEndpoint(request.Destination);

			if (endpoint == null) {
				
				Log.w($"No TCPClientServer IWebEndpoint accepts: {request}");

				return new NetworkErrorMessage(NetworkStatusCode.NotFound, $"Path not found: {request.Destination}", request); 
	
			}

			try {

				return endpoint.Interpret(request, source);

			} catch (Exception ex) {

				Log.x(ex);

				return new NetworkErrorMessage(NetworkStatusCode.ServerError, $"EXCEPTION: {ex.Message}", request); 

			}

		}

		protected override void Service() {

			while(ShouldRun) {
			
				TCPMessage request = default(TCPMessage);
				INetworkMessage response = default(TCPMessage);

				try {

					if (m_client.Available < 1) {
						
						System.Threading.Thread.Yield();
						continue;

					} 

					request = m_serializer.DeserializePackage(new BlockingNetworkStream(m_client.GetSocket()));

					m_client.GetStream().Flush();

					if (request.Headers?.ContainsKey(ServerTypeKey) == true) {

						string serverType = request.Headers[ServerTypeKey] as string;

						if (m_servers.ContainsKey(serverType)) {

							IPEndPoint clientEndpoint = null;

							if (request.Headers.ContainsKey(AddressKey) &&
								request.Headers.ContainsKey(PortKey)) {
							
								IPAddress address = IPAddress.Parse((string)request.Headers[AddressKey]);

								Int64 port = (Int64)request.Headers[PortKey];
								clientEndpoint = new IPEndPoint(address, (int)port);

							} 

							response = m_servers[serverType].Interpret(request, clientEndpoint);

						} else {

							response = new NetworkErrorMessage(NetworkStatusCode.ResourceUnavailable, $"Missing server type: '{serverType}'.", request); 

						}

					} else {

						IWebEndpoint endpoint = GetEndpoint(request.Destination);

						if (endpoint != null) {

							response = new TCPMessage(Interpret(request, m_client.GetEndPoint()));

						} else {
							
							response = new NetworkErrorMessage(NetworkStatusCode.UnableToProcess, $"Missing header: '{ServerTypeKey}' and no local endpoint for '{request.Destination}'."); 

						}

					}

				} catch (Exception ex) {

					Log.x (ex);
					response = new NetworkErrorMessage(ex);

				}

				response.OverrideHeaders(new Dictionary<string, object>() { 
					{ Settings.Consts.ConnectionRouterHeaderHostNameKey(), Identity.Name }
				});

				response.Destination = request.Destination;
				byte[] requestData = m_serializer.SerializeMessage(new TCPMessage(response));

				new BlockingNetworkStream(m_client.GetSocket()).Write(requestData, 0, requestData.Length);

			}

		}

		private INetworkMessage SendAttachMessage() {

			TCPMessage attachMessage = new TCPMessage () {
				Destination = Settings.Consts.ConnectionRouterAddHostDestination(),
				Payload = new RegistrationRequest() { 
					HostName = Identity.Name,
					Address = m_client.GetLocalEndPoint()?.GetAddress(),
					Port = m_client.GetLocalEndPoint()?.GetPort() ?? 0
				}
			};

			byte[] requestData = m_serializer.SerializeMessage(attachMessage);
			new BlockingNetworkStream(m_client.GetSocket()).Write(requestData, 0, requestData.Length);
			return m_serializer.DeserializePackage(new BlockingNetworkStream(m_client.GetSocket()));

		}

		private void Connect() {

			m_client = new TcpClient();
			m_client.SendTimeout = Timeout;
			m_client.Client.Blocking = true;

			m_connectionPoller = new ConnectionPoller(m_client, () => {

				if (ShouldRun) { Connect(); }

			});

			m_connectionPoller.Start();

			m_client.Connect(Address, Port);

			INetworkMessage response = SendAttachMessage();

			if (response.Code == NetworkStatusCode.Ok.Raw()) {

				base.Start();

			} else {

				Log.e($"TCPClientServer got bad reply [{response.Code}]: {response.Payload}");
				m_client.Close();

			}

		}

	}

}

