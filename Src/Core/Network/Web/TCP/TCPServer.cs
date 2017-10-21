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

namespace Core.Network
{
	public class TCPServer : DeviceBase, IWebServer
	{
		
		int m_port;
		private TcpListener m_listener;
		private bool m_shouldRun;
		private TCPPackageFactory m_packageFactory;
		private Task m_service;
		private IDictionary<TcpClient, Task> m_connections;

		private IDictionary<string,IWebEndpoint> m_endpoints;

		public TCPServer (string id, int port, ISerialization serialization) : base(id)
		{

			m_port = port;
			m_packageFactory = new TCPPackageFactory (serialization);
			m_endpoints = new Dictionary<string, IWebEndpoint> ();

		}

		public int Port { get { return m_port; } }

		public string Ip { 

			get {

				return Dns.GetHostEntry (Dns.GetHostName ()).AddressList.Where (ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault ()?.ToString ();

			}

		}

		public void AddEndpoint(IWebEndpoint interpreter) {

			m_endpoints.Add (interpreter.UriPath, interpreter);

		}

		/// <summary>
		/// Represents a single client connection.
		/// </summary>
		/// <param name="client">Client.</param>
		private void Connection(TcpClient client) {
		
			TCPMessage responseMessage =  new TCPMessage() {Code = (int) WebStatusCode.NotDefined};

			while (m_shouldRun && client.Connected) {
			
				try {
					
					TCPMessage requestMessage = m_packageFactory.DeserializePackage (client.GetStream ());
					Log.t($"Server got message for: {requestMessage.Destination}");
					if (m_endpoints.ContainsKey(requestMessage.Destination)) {

						responseMessage = new TCPMessage() {
							Code = (int) WebStatusCode.Ok,
							Payload = m_endpoints [requestMessage.Destination].Interpret (requestMessage.Payload, requestMessage.Destination, requestMessage.Headers)
						};

					} else {

						responseMessage = new TCPMessage() {
							Code = (int) WebStatusCode.NotFound,
							Payload =  new WebErrorMessage((int) WebStatusCode.NotFound, $"Path not found: {requestMessage.Destination}")
						};

					}

				} catch (Exception ex) {

					if (m_shouldRun) {
					
						Log.x(ex);

						responseMessage = new TCPMessage() {
							Code = (int) WebStatusCode.ServerError,

							#if DEBUG
							Payload = ex.ToString()
							#endif

						};

					}

				}

				if (m_shouldRun && client.Connected) {
				
					try {

						byte[] response = m_packageFactory.SerializeMessage(responseMessage);
						Log.t($"Server will send response now {response.Length}!");
						client.GetStream ().Write (response, 0, response.Length);

					} catch (Exception ex) {
					
						Log.x (ex);
						client.Close ();

					}

				}

			}

			Log.t($"Now disconnecting client: {client.Client.RemoteEndPoint.ToString()}.");

		}

		/// <summary>
		/// Represents the server side listener.
		/// </summary>
		private void Service() {
		
			Log.t ($"Starting TCP Server on port {m_port}");

			m_listener = new TcpListener (IPAddress.Any, m_port);
			m_listener.Start ();

			while (m_shouldRun) {

				try {

					TcpClient client = m_listener.AcceptTcpClient();
					Log.t($"Got connection from: {client.Client.RemoteEndPoint.ToString()}");
					client.Client.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
					m_connections[client] = Task.Run( () => Connection(client));

				} catch (Exception ex) { Log.x (ex); }

			}

		}

		public override bool Ready {
			get {
				return m_listener != null;
			}
		}

		public override void Start () {
			
			m_shouldRun = true;
			m_connections = new Dictionary<TcpClient, Task> ();
			m_service = Task.Factory.StartNew (Service);

		}

		public override void Stop () {
			
			m_shouldRun = false;
			m_listener.Stop ();
			m_listener = null;

			foreach (TcpClient client in m_connections.Keys) {
			
				client.Close ();

			}

		}
	}
}

