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
using System.Collections.Generic;
using System.Linq;
using R2Core.Device;

namespace R2Core.Network {

	/// <summary>
	/// "Catch all" endpoint used to redirect requests to the ´IClientConnection´s of a
	/// ´TCPServer´. This includes the handleing of a ´RegistrationRequest´ in order to keep track of
	/// the registered ´IClientConnection´s..
	/// </summary>
	public class TCPRouterEndpoint : DeviceBase, IWebEndpoint {

		private readonly TCPServer m_tcpServer;
		public IDictionary<string, WeakReference<IClientConnection>> Connections { get; private set; }
		private static readonly string HeaderHostName = Settings.Consts.ConnectionRouterHeaderHostNameKey();

        private IDictionary<string, PingService> m_pingServices;

		private readonly object m_lock = new object();

        public int PingInterval = 30000;

		public TCPRouterEndpoint(TCPServer tcpServer) : base(Settings.Identifiers.TCPRouterEndpoint()) {
			
			m_tcpServer = tcpServer;
			Connections = new Dictionary<string, WeakReference<IClientConnection>>();
            m_pingServices = new Dictionary<string, PingService>();

        }

		public INetworkMessage Interpret(INetworkMessage request, System.Net.IPEndPoint source) {

			if (request.Destination == Settings.Consts.ConnectionRouterAddHostDestination()) {

				IClientConnection connection = m_tcpServer.Connections.FirstOrDefault(c => 
					c.Address == source.Address.ToString() &&
					c.Port == source.Port);

                System.Diagnostics.Debug.Assert(connection != null);

                string hostName = request.Payload.HostName;

                lock (m_lock) {

                    Connections[hostName] = new WeakReference<IClientConnection>(connection);

                    Log.i($"Setting host '{hostName}' to connection: {connection}", Identifier);
					connection.StopListening();

                    Connections = Connections.Where(c => c.Value.GetTarget().Ready).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    if (m_pingServices.ContainsKey(hostName)) {

                        m_pingServices[hostName].Stop();

                    }

                    m_pingServices[hostName] = new PingService(connection, PingInterval);
                    m_pingServices[hostName].Start();

                }

				return new TCPMessage {
					Code = NetworkStatusCode.Ok.Raw(),
                    Destination = Settings.Consts.ConnectionRouterAddHostDestination(),
                    Payload = new RoutingRegistrationResponse {
                        Address = source.Address.ToString(),
                        Port = source.Port
                    }
                };

			} 

            if (request.Headers.ContainsKey(HeaderHostName)) {

				string hostName = request.Headers[HeaderHostName] as string;

				IClientConnection connection;

				lock(m_lock) {
					
					connection = Connections.ContainsKey(hostName) ? Connections[hostName].GetTarget() : null;

				}
				 
				if (connection != null && connection.Ready) {
				
					request.Headers[Settings.Consts.ConnectionRouterHeaderClientAddressKey()] = connection.Address;
					request.Headers[Settings.Consts.ConnectionRouterHeaderClientPortKey()] = connection.Port;

					return connection.Send(request);

				}

                return connection?.Ready == false
                    ? new NetworkErrorMessage(NetworkStatusCode.ResourceUnavailable, $"Host with name '{hostName}' has been disconnected.")
                    : new NetworkErrorMessage(NetworkStatusCode.ResourceUnavailable, $"Host with name '{hostName}' was not found.");
            }

            return new NetworkErrorMessage(NetworkStatusCode.BadRequest, $"Missing '{HeaderHostName}' parameter.");

		}

        public override void Stop() {

            foreach(PingService ping in m_pingServices.Values) {

                ping.Stop();

            }

        }

        public string UriPath { get { return ".*"; } }

        public override string ToString() {

            string description = $"{Identifier}: \n";

            foreach (var kvp in Connections) {

                description += $"\t[{kvp.Key}:{kvp.Value.GetTarget()}]";
            
            }

            return description;

        }

    }

}

