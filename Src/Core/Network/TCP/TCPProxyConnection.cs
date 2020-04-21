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
using System.Threading.Tasks;
using R2Core.Device;

namespace R2Core.Network
{
	public class TCPProxyConnection : DeviceBase {

		private TcpClient m_client;
		private Task m_listener;
		private bool m_shouldRun;

		/// <summary>
		/// Returns <c>true</c> if the connection is alive.
		/// </summary>
		/// <value><c>true</c> if this instance is connected; otherwise, <c>false</c>.</value>
		public override bool Ready { get { return m_client.IsConnected(); } }

		/// <summary>
		/// Returns the remote address.
		/// </summary>
		/// <value>The address.</value>
		public string Address { get { return m_client.GetEndPoint().GetAddress(); } }

		/// <summary>
		/// Returns the remote port.
		/// </summary>
		/// <value>The port.</value>
		public int Port { get { return m_client.GetEndPoint().GetPort(); } }

		public TCPProxyConnection ProxyClient;

		public TCPProxyConnection(string id, TcpClient client) : base(id) {

			m_client = client;

		}

		public override void Start() {
		
			m_shouldRun = true;
			m_listener = Task.Factory.StartNew(Connection);

		}

		public override void Stop() {

			m_shouldRun = false;
		
        }

		private void Connection() {
		
            if (m_shouldRun) { }
		}
	}
}

