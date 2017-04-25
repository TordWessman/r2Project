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

namespace Core.Network.Web
{
	public class TCPServer : DeviceBase, IWebServer
	{
		
		int m_port;
		private Socket m_socket;
		private TcpListener m_listener;
		private bool m_shouldRun;
		private TCPPackageFactory m_packageFactory;
		private Task m_serviceTask;
		private IDictionary<string,IWebEndpoint> m_endpoints;
		private IR2Serialization m_serialization;

		public TCPServer (string id, int port, IR2Serialization serialization) : base(id)
		{

			m_port = port;
			m_listener = new TcpListener (IPAddress.Any, m_port);
			m_packageFactory = new TCPPackageFactory (serialization);
			m_serviceTask = new Task (Service);
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

		private void Service() {
		
			while (m_shouldRun) {

				try {

					using(m_socket = m_listener.AcceptSocket()) {

						m_socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

						NetworkConnection con = new NetworkConnection(new NetworkStream(m_socket));
						byte [] input = con.ReadData();

						if (con.Status == NetworkConnectionStatus.OK) {

							TCPPackage request = m_packageFactory.CreateTCPPackage(input);

							if (m_endpoints.ContainsKey(request.Path)) {

								con.WriteData(m_endpoints[request.Path].Interpret(request.Payload, request.Headers), true);

							} else {
							
								con.WriteData(m_packageFactory.SerializePayload(new WebErrorMessage(WebStatusCode.NotFound, $"Path not found: {request.Path}")), true);

							}

						} else {

							throw new WebException($"Got bad response during connection: {con.Status}.");

						}
					}

				} catch (Exception ex) {

					Log.x (ex);

				}

			}

		}

		public override void Start ()
		{
			m_shouldRun = true;

			m_serviceTask.Start ();

		}

		public override void Stop ()
		{
			m_shouldRun = false;

		}
	}
}

