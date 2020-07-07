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
using System.Net;
using System.Linq;
using System.Collections.Generic;
using MessageIdType = System.String;

namespace R2Core.Network
{
	/// <summary>
	/// Default implementation for all TCP related traffic.
	/// </summary>
	public class TCPServer : ServerBase, INetworkBroadcaster {

		// Ensure therad safety for connection access
		private readonly object m_connectionsLock = new object();

		private TcpListener m_listener;

		private readonly ITCPPackageFactory<TCPMessage> m_packageFactory;
		private IList<IClientConnection> m_connections;

        /// <summary>
        /// Default timeout for Broadcasts
        /// </summary>
        public const int DefaultBroadcastTimeout = 2000;

		public int Timeout = 30000;

        /// <summary>
        /// Returns all current connections
        /// </summary>
        /// <value>The connections.</value>
        public IEnumerable<IClientConnection> Connections => m_connections;

		public TCPServer(string id, int port, ITCPPackageFactory<TCPMessage> packageFactory) : base(id) {

			m_packageFactory = packageFactory;
			m_connections = new List<IClientConnection>();
			SetPort(port);

		}

        public override bool Ready => ShouldRun && m_listener?.Server?.IsBound == true;

		public MessageIdType Broadcast(INetworkMessage message, Action<INetworkMessage, string, Exception> responseDelegate = null, int timeout = DefaultBroadcastTimeout) {

			m_connections.AsParallel().ForAll((connection) => {

				try {
				
					INetworkMessage response = connection.Send(new BroadcastMessage(message, connection.Address, connection.Port));

					if (response.IsError()) {
					
						responseDelegate?.Invoke(response, connection.LocalAddress, new NetworkException(response)); 

					} else { responseDelegate?.Invoke(response, connection.LocalAddress, null); }

				} catch (Exception ex) {

                    responseDelegate?.Invoke(null, null, ex);

                }

            });

			return null;
		
		}

		/// <summary>
		/// Represents the server side listener.
		/// </summary>
		protected override void Service() {
			
			m_listener = new TcpListener(IPAddress.Any, Port);
			m_listener.Start();

			while(ShouldRun) {

				try {
						
					TcpClient client = m_listener.WaitForConnection(Timeout);

					TCPServerConnection connection = 
						new TCPServerConnection(
							$"TCPServerConnection: {client.Client.RemoteEndPoint.ToString()}", 
							m_packageFactory,
							client, 
							Interpret);
					
					connection.OnDisconnect += OnDisconnect;

					lock(m_connectionsLock) {
						
						m_connections.Add(connection);
						connection.Start();

					}

				} catch (Exception ex) {

					if (ex.IsClosingNetwork()) {

						Log.i("Closing a connection.", Identifier);

					} else {
					
						Log.x(ex, Identifier);

					}
					 
				}

			}

		}

		private void OnDisconnect(IClientConnection connection, Exception ex) {
		
			if (ex != null) { Log.x(ex, Identifier); }

			lock(m_connectionsLock) {

                if (connection != null) {

                    foreach (IClientConnection removed in m_connections.Remove(c => c.Address == connection.Address && c.Port == connection.Port)) {

                        Log.i($"TCPServer removed client connection: {removed}", Identifier);

                    }

                }

			}

		}

		public override INetworkMessage Interpret(INetworkMessage request, IPEndPoint source) {

            if (request.IsPing()) {

                return new PongMessage();
            
            }

            if (request.Destination == null) {

                return new NetworkErrorMessage(NetworkStatusCode.BadRequest, "Hehehehehehehe");
            
            }

            IWebEndpoint endpoint = GetEndpoint(request.Destination);

			if (endpoint != null) {

				try {

                    INetworkMessage response = endpoint.Interpret(request, source);
                    return response;

                } catch (Exception ex) {

                    Log.x(ex, Identifier);
                    return new NetworkErrorMessage(NetworkStatusCode.ServerError, $"Interpretation caused an exception: {ex.Message}", request);

                }

			}

			return new NetworkErrorMessage(NetworkStatusCode.NotFound, $"Path not found: {request.Destination}", request);

		}

		protected override void Cleanup() {
			
			m_listener?.Stop();
			m_listener = null;

			lock(m_connectionsLock) {

				m_connections.ToList().ForEach((client) => {

					client.Stop();

				});

				m_connections = new List<IClientConnection>();

			}

		}

	}

}