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
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Linq;

namespace R2Core.Network
{
	public class TCPProxy : DeviceBase
	{
		private IList<TcpClient> m_clients = new List<TcpClient>();
		private int m_inPort;
		private int m_outPort;
		private TcpListener m_inListener;
		private TcpListener m_outListener;

		private Task m_inService;
		private Task m_outService;
		private bool m_shouldRun = true;
		private readonly object m_clientsLock = new object();

		public IEnumerable<TcpClient> Clients { get { return m_clients; } }
		public int Timeout = 5000;

		public TCPProxy(string id, int inPort, int outPort) : base(id) {

			m_inPort = inPort;
			m_outPort = outPort;
			m_inService = new Task(InService);

		}

		/// <summary>
		/// Start the listener service.
		/// </summary>
		public override void Start() {

			m_inService.Start();

		}

		/// <summary>
		/// Closes all open sockets and stop the listener service. 
		/// </summary>
		public override void Stop() {

			try {

				m_outListener?.Stop();
				m_inListener?.Stop();
				m_inListener = null;
				m_outListener = null;

			} catch (Exception ex) { 

				Log.w($"Problems when closing TCPProxy listeners. Error: {ex.Message}");

			}

			lock (m_clientsLock) {

				m_clients.ToList().ForEach((client) => {

					try {
						
						if (client.IsConnected()) {

							client.Close();
						
						}
						 
					} catch (Exception) { }

				});

				m_clients = new List<TcpClient>();

			}

		}

		private void InService() {

			m_inListener = new TcpListener(IPAddress.Any, m_inPort);
			m_inListener.Start();

			while (m_shouldRun) {

				try {

					TcpClient client = m_inListener.WaitForConnection(Timeout);

					Log.t($"Server connected: {client}");

					lock (m_clientsLock) { 

						m_clients.Remove(c => !c.IsConnected());
						m_clients.Add(client);
					
					}

				} catch (Exception ex) { if (!ex.IsClosingNetwork()) { Log.x(ex); } }

			}

		}

		private void OutService() {

			m_outListener = new TcpListener(IPAddress.Any, m_inPort);
			m_outListener.Start();

			while (m_shouldRun) {

				try {

					TcpClient client = m_outListener.WaitForConnection(Timeout);

					Log.t($"Client connected: {client}");

					lock (m_clientsLock) { 

						m_clients.Remove(c => !c.IsConnected());
						m_clients.Add(client);

					}

				} catch (Exception ex) { if (!ex.IsClosingNetwork()) { Log.x(ex); } }

			}

		}

	}

}

