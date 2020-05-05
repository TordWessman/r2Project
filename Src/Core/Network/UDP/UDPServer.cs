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
using System.IO;
using System.Linq;

//TODO: add support for UDP signatures(create a new UDP package factory capable of distinguishing package identifiers)

namespace R2Core.Network
{

    /// <summary>
    /// IServer implementation listening to broadcast UDP packages.
    /// </summary>
	public class UDPServer: ServerBase {

		private UdpClient m_listener;
		private IPEndPoint m_groupEndpoint;
		private ITCPPackageFactory<TCPMessage> m_packageFactory;

		public UDPServer(string id, int port, ITCPPackageFactory<TCPMessage> packageFactory) : base(id) {

			SetPort(port);
			m_groupEndpoint = new IPEndPoint(IPAddress.Any, Port);
			m_packageFactory = packageFactory;

		}

		public override bool Ready { get { return ShouldRun && m_listener != null; } }

		/// <summary>
		/// If true, server NOT ignore requests sent from the same IP.
		/// </summary>
		public bool AllowLocalRequests = false;

		protected override void Service() {

            m_listener = new UdpClient {
                EnableBroadcast = true
            };
            m_listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			m_listener.Client.Bind(m_groupEndpoint);

			while(ShouldRun) {
			
				INetworkMessage response = new TCPMessage();
				IPEndPoint client = null;

				// Any request should contain this identifier. It will automatically be bundled with the reply.
				string broadcastMessageUniqueIdentifierHeaderValue = ""; 

				try {
					
					byte[] requestData = m_listener.Receive(ref client);

					if (!AllowLocalRequests && (IPAddress.IsLoopback(client.Address) || Addresses.Contains(client.Address.ToString()))) {
					
						// Ignore requests sent by this computer
						continue;

					}

					using(MemoryStream requestDataStream = new MemoryStream(requestData)) {

						TCPMessage request = m_packageFactory.DeserializePackage(requestDataStream);

						broadcastMessageUniqueIdentifierHeaderValue = request.GetBroadcastMessageKey();

						response = Interpret(request, client);

					}

				} catch (Exception ex) {

					if (ShouldRun) {

						if (!ex.IsClosingNetwork()) {
						
							Log.x(ex);

						}

						response = new NetworkErrorMessage(ex);

					}

				}

				if (ShouldRun) {
				
					// Make sure the request returns the expected broadcast key.
					BroadcastMessage responseBroadcast = new BroadcastMessage(response, Addresses.Aggregate((a1,a2) => a1 + ";" + a2) , Port);
					responseBroadcast.Identifier = broadcastMessageUniqueIdentifierHeaderValue;

					byte[] responseData = m_packageFactory.SerializeMessage(new TCPMessage(responseBroadcast)); 
					m_listener.Send(responseData, responseData.Length, client);

				}

			}
		
		}

		public override INetworkMessage Interpret(INetworkMessage request, System.Net.IPEndPoint source) {

			IWebEndpoint ep = GetEndpoint(request.Destination);

			if (ep != null) {
				
				return new TCPMessage(ep.Interpret(request, source));

			} else {

				return new NetworkErrorMessage(NetworkStatusCode.NotFound, $"Path not found: {request.Destination}", request);

			}

		}

		protected override void Cleanup() {
			
			m_listener?.Close();
			m_listener = null;

		}

	}

}