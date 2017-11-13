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
using Core.Network.Web;
using System.Net.Sockets;
using System.Net;
using System.IO;

//TODO: add support for UDP signatures (create a new UDP package factory capable of distinguishing package identifiers)
using System.Collections.Generic;


namespace Core.Network
{
	public class UDPServer: ServerBase
	{

		private UdpClient m_listener;
		private IPEndPoint m_groupEndpoint;
		private ITCPPackageFactory<TCPMessage> m_packageFactory;

		public UDPServer (string id, int port, ITCPPackageFactory<TCPMessage> packageFactory) : base (id, port) {
			
			m_groupEndpoint = new IPEndPoint(IPAddress.Any, Port);
			m_packageFactory = packageFactory;

		}

		public override bool Ready { get { return ShouldRun && m_listener != null; } }

		public override void Start () {
			
			m_listener = new UdpClient (m_groupEndpoint);
			base.Start ();

		}

		protected override void Service() {
		
			while (ShouldRun) {
			
				TCPMessage response = new TCPMessage();
				IPEndPoint client = new IPEndPoint(IPAddress.Any, 0);

				// Any request should contain this identifier. It will automatically be bundled with the reply.
				string broadcastMessageUniqueIdentifierHeaderValue = ""; 

				try {

					byte [] requestData = m_listener.Receive (ref client);

					using (MemoryStream requestDataStream = new MemoryStream (requestData)) {

						TCPMessage request = m_packageFactory.DeserializePackage (requestDataStream);

						broadcastMessageUniqueIdentifierHeaderValue = 
								(request.Headers?.ContainsKey(BroadcastMessage.BroadcastMessageUniqueIdentifierHeaderKey) == true ?
								request.Headers[BroadcastMessage.BroadcastMessageUniqueIdentifierHeaderKey] : "(No Unique Identifier Header provided)")
								as string;

						IWebEndpoint ep = GetEndpoint (request.Destination);

						if (ep != null) {

							response = new TCPMessage(ep.Interpret (request, client));

						} else {

							response = new TCPMessage() {
								Code = (int) WebStatusCode.NotFound,
								Payload =  new WebErrorMessage((int) WebStatusCode.NotFound, $"Path not found: {request.Destination}")
							};

						}
					}

				} catch (Exception ex) {
				
					if (ShouldRun) {

						Log.x(ex);

						response = new TCPMessage() {
							Code = (int) WebStatusCode.ServerError,

							#if DEBUG
							Payload = ex.ToString()
							#endif

						};

					}

				}

				if (response.Headers == null) { response.Headers = new Dictionary<string, object> (); }

				response.Headers.Add (BroadcastMessage.BroadcastMessageUniqueIdentifierHeaderKey, broadcastMessageUniqueIdentifierHeaderValue);

				byte[] responseData = m_packageFactory.SerializeMessage (response);
				m_listener.Send (responseData, responseData.Length, client);

			}
		
		}

		public override void Stop () {
			
			base.Stop ();

			m_listener.Close ();
			m_listener = null;

		}

	}

}