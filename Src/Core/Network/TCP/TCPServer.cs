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
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using R2Core.Network;
using System.IO;
using MessageIdType = System.String;

namespace R2Core.Network
{
	public class TCPServer : ServerBase, IServer, INetworkBroadcaster
	{

		// Ensure therad safety for connection access
		private readonly object m_connectionsLock = new object();

		private TcpListener m_listener;

		private ITCPPackageFactory<TCPMessage> m_packageFactory;
		private IList<IClientConnection> m_connections;

		/// <summary>
		/// Default timeout for Broadcasts
		/// </summary>
		public const int DefaultBroadcastTimeout = 2000;

		/// <summary>
		/// Returns all current connections
		/// </summary>
		/// <value>The connections.</value>
		public IEnumerable<IClientConnection> Connections { get { return m_connections; } }

		public TCPServer (string id, int port, ITCPPackageFactory<TCPMessage> packageFactory) : base(id, port)
		{

			m_packageFactory = packageFactory;
			m_connections = new List<IClientConnection> ();

		}

		public override bool Ready { get { return ShouldRun && m_listener != null; } }

		public MessageIdType Broadcast (INetworkMessage message, Action<BroadcastMessage, Exception> responseDelegate = null, int timeout = DefaultBroadcastTimeout) {
		
			INetworkMessage request = new BroadcastMessage (message);

			m_connections.AsParallel ().ForAll ((connection) => {

				try {
				
					INetworkMessage response = connection.Send(request);
					if (responseDelegate != null) { responseDelegate(new BroadcastMessage(response, connection.Endpoint), null); }

				} catch (Exception ex) {

					if (responseDelegate != null) { responseDelegate(null, ex); }

				}

			});

			return null;
		
		}

		/// <summary>
		/// Represents a single client connection.
		/// </summary>
		/// <param name="client">Client.</param>
		private void Connection(TcpClient client) {
		
			TCPMessage responseMessage =  new TCPMessage() {Code = WebStatusCode.NotDefined.Raw(), Headers = new Dictionary<string, object>()};

		}

		/// <summary>
		/// Represents the server side listener.
		/// </summary>
		protected override void Service() {
			
			m_listener = new TcpListener (IPAddress.Any, Port);
			m_listener.Start ();

			while (ShouldRun) {

				try {

					lock(m_connectionsLock) {
						
						TcpClient client = m_listener.AcceptTcpClient();
						client.Client.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
						TCPClientConnection connection = new TCPClientConnection(client.Client.RemoteEndPoint.ToString(), m_packageFactory, client);
						connection.OnReceive += OnReceive;
						connection.OnDisconnect += OnDisconnect;

						m_connections.Add(connection);
						connection.Start();

					}

				} catch (System.Net.Sockets.SocketException ex) {

					if (ex.SocketErrorCode != SocketError.Interrupted) {
						
						Log.w ($"Connection failure: '{ex.Message}'. Error code: {ex.ErrorCode}. Socket error type: {ex.SocketErrorCode}.");

					}

				} catch (Exception ex) { Log.x (ex); }

			}

		}

		private void OnDisconnect(IClientConnection connection, Exception ex) {
		
			if (ex != null) { Log.x(ex); }

			lock (m_connectionsLock) {

				m_connections.Remove (connection);

			}

		}

		private INetworkMessage OnReceive(INetworkMessage request, IPEndPoint address) {
		
			IWebEndpoint endpoint = GetEndpoint (request.Destination);

			if (endpoint != null) { 
				
				var response = endpoint.Interpret (request, address);
				return response;

			} 

			return new TCPMessage() {
				Code = WebStatusCode.NotFound.Raw(),
				Payload =  new WebErrorMessage(WebStatusCode.NotFound.Raw(), $"Path not found: {request.Destination}")
			};

		}

		protected override void Cleanup () {
			
			m_listener.Stop ();
			m_listener = null;

			// Avouid conncurrency issues.

			lock (m_connectionsLock) {

				m_connections.ToList ().ForEach ((client) => {

					client.Stop ();

				});

			}

		}

	}

}