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
using System.Collections.Generic;
using System.Linq;
using Core.Network.Web;

namespace Core.Network
{
	public class TCPClient: DeviceBase, IMessageClient
	{
		string m_host;
		int m_port;
		private TcpClient m_client;
		ITCPPackageFactory<TCPMessage> m_serializer;

		public IDictionary<string, object> Headers;

		public TCPClient (string id, ITCPPackageFactory<TCPMessage> serializer, string host, int port) : base(id) {

			m_host = host;
			m_port = port;
			m_client = new TcpClient ();
			m_serializer = serializer;

		}

		public override bool Ready {
			get {
				return m_client.Connected;
			}
		}

		public override void Start() {
		
			m_client.Connect (m_host, m_port);

		}

		public override void Stop () {

			m_client.Close ();

		}

		public System.Threading.Tasks.Task SendAsync(INetworkMessage message, Action<INetworkMessage> responseDelegate) {

			return System.Threading.Tasks.Task.Factory.StartNew ( () => {

				INetworkMessage response;
				Exception exception = null;

				try {

					response = Send(message);

				} catch (Exception ex) {

					response = new TCPMessage() { Code = (int) WebStatusCode.NetworkError, Payload = ex.ToString()};
					exception = ex;
				}

				responseDelegate(response);

				if (exception != null) { throw exception; }

			});

		}


		public INetworkMessage Send(INetworkMessage requestMessage) {

			TCPMessage message = requestMessage is TCPMessage ? ((TCPMessage)requestMessage) : new TCPMessage (requestMessage);
			
			if (message.Headers != null && Headers != null) {

				Headers.ToList().ForEach( kvp => message.Headers.Add(kvp));

			}

			message.Headers = message.Headers ?? Headers;

			byte[] request = m_serializer.SerializeMessage (message);

			m_client.GetStream ().Write (request, 0, request.Length);

			return m_serializer.DeserializePackage (m_client.GetStream ());

		}

	}
}

