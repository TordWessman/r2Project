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
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Core.Data;
using Core.Network.Web;
using System.IO;

namespace Core.Network
{
	public class TCPServer : ServerBase, IWebServer
	{
		
		private TcpListener m_listener;

		private ITCPPackageFactory<TCPMessage> m_packageFactory;
		private IDictionary<TcpClient, Task> m_connections;

		public TCPServer (string id, int port, ITCPPackageFactory<TCPMessage> packageFactory) : base(id, port)
		{

			m_packageFactory = packageFactory;

		}

		public override bool Ready { get { return ShouldRun && m_listener != null; } }

		/// <summary>
		/// Represents a single client connection.
		/// </summary>
		/// <param name="client">Client.</param>
		private void Connection(TcpClient client) {
		
			TCPMessage responseMessage =  new TCPMessage() {Code = (int) WebStatusCode.NotDefined, Headers = new Dictionary<string, object>()};

			using (client) {
				
				while (ShouldRun && client.Connected) {

					try {

						TCPMessage requestMessage = m_packageFactory.DeserializePackage (client.GetStream ());
						Log.t($"Server got message for: {requestMessage.Destination}");
						IWebEndpoint endpoint = GetEndpoint (requestMessage.Destination);

						if (endpoint != null) {
							
							responseMessage = new TCPMessage(endpoint.Interpret (requestMessage, (IPEndPoint) client.Client.RemoteEndPoint));

						} else {

							responseMessage = new TCPMessage() {
								Code = (int) WebStatusCode.NotFound,
								Payload =  new WebErrorMessage((int) WebStatusCode.NotFound, $"Path not found: {requestMessage.Destination}")
							};

						}

					} catch (Exception ex) {

						Log.t($"I died {ex.ToString()}.");

						if (ShouldRun) {

							Log.x(ex);

							responseMessage = new TCPMessage() {
								Code = (int) WebStatusCode.ServerError,

								#if DEBUG
								Payload = ex.ToString()
								#endif

							};

						}

					}

					if (ShouldRun && client.Connected && responseMessage.Code != (int)WebStatusCode.NotDefined) {

						try {
							
							byte[] response = m_packageFactory.SerializeMessage (responseMessage);
							Log.t ($"Server will send response now {response.Length}!");
							client.GetStream ().Write (response, 0, response.Length);

						} catch (IOException ex) {
						
							if (ex.InnerException is SocketException) {
							
								// Disconnected....

							} else {
							
								Log.x (ex);

							}
							if (client.Connected) { client.Close (); }
							break;

						} catch (Exception ex) {

							Log.x (ex);
							if (client.Connected) { client.Close (); }
							break;

						}

					} else if (ShouldRun) {

						Log.w($"TCPServer not sending reply to client {client.Client.RemoteEndPoint}. Reason: " + (!client.Connected ? "Disconnected" : responseMessage.Code == (int)WebStatusCode.NotDefined ? "Response message not defined" : ""));

					}

				}
			
			}


			Log.t($"Now disconnecting client: {client.Client.RemoteEndPoint.ToString()}.");

		}

		/// <summary>
		/// Represents the server side listener.
		/// </summary>
		protected override void Service() {

			Log.t ($"Starting TCP Server on port {Port}");

			m_connections = new Dictionary<TcpClient, Task> ();
			m_listener = new TcpListener (IPAddress.Any, Port);
			m_listener.Start ();

			while (ShouldRun) {

				try {
					
					TcpClient client = m_listener.AcceptTcpClient();
					Log.t($"Got connection from: {client.Client.RemoteEndPoint.ToString()}");
					client.Client.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
					m_connections[client] = Task.Run( () => Connection(client));

				} catch (System.Net.Sockets.SocketException ex) {

					if (ex.SocketErrorCode != SocketError.Interrupted) {
					
						Log.w ($"Connection failure. Error code: {ex.ErrorCode}. Socket error type: {ex.SocketErrorCode}.");

					}

				} catch (Exception ex) { Log.x (ex); }

			}

		}

		protected override void Cleanup () {
			
			m_listener.Stop ();
			m_listener = null;

			foreach (TcpClient client in m_connections.Keys) {
			
				try {

					client.Close ();

				} catch (SocketException) {} 
					
			}

		}

	}

}