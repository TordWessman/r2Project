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
using R2Core.Device;
using System.Net;

namespace R2Core.Network
{

	/// <summary>
	/// Wraps a ´IMessageClient´ connection.
	/// </summary>
	public class HostConnection : DeviceBase, IClientConnection, IMessageClientObserver {
		
		private IMessageClient m_connection;

		public event OnReceiveHandler OnReceive;

		public event OnDisconnectHandler OnDisconnect;

        public bool Busy => m_connection.Busy;

        /// <summary>
        /// .`connection` is the connection used for transfers.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="connection">Host.</param>
        public HostConnection(string id, IMessageClient connection) : base(id) {
			
			m_connection = connection;
			m_connection.AddClientObserver(this);

		}

		public string Address { get { return m_connection.Address; } }

        public string LocalAddress { get { return m_connection.LocalAddress; } }

        public int Port { get { return m_connection.Port; } } 
	
		public override bool Ready { get { return m_connection.Ready; } }

		public override void Start() { m_connection.Start(); }

		public override void Stop() { m_connection.Stop(); }

		public void StopListening() {

			m_connection.StopListening();

		}

		public INetworkMessage Send(INetworkMessage message) {
			
			INetworkMessage response = m_connection.Send(message);

			if (response == null) {
			
				Log.e($"ARGH: Got null reply from connection: {m_connection.Identifier}:{m_connection.Guid.ToString()} ({m_connection}).");
				throw new NetworkException($"Got null reply from connection: {m_connection.Identifier}:{m_connection.Guid.ToString()} ({m_connection}).");

			}

			return response;

		}

		public void OnClose(IMessageClient client, Exception ex) {

            OnDisconnect?.Invoke(this, ex);

        }

        public void OnRequest(INetworkMessage request) { }

		public void OnResponse(INetworkMessage response, Exception ex) {

            OnReceive?.Invoke(response, new IPEndPoint(IPAddress.Parse(m_connection.Address), m_connection.Port));

        }

        public void OnBroadcast(INetworkMessage response) {

            OnReceive?.Invoke(response, new IPEndPoint(IPAddress.Parse(m_connection.Address), m_connection.Port));

        }

        public string Destination { get { return null; } }

	}

}

