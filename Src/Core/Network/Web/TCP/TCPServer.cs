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

namespace Core.Network.Web
{
	public class TCPServer : DeviceBase, IWebServer
	{
		
		int m_port;
		private Socket m_socket;
		private TcpListener m_listener;
		private bool m_shouldRun;

		public TCPServer (string id, int port) : base (id)
		{

			m_port = port;
			m_listener = new TcpListener (IPAddress.Any, m_port);

		}

		public int Port { get { return m_port; } }

		public string Ip { 

			get {

				return Dns.GetHostEntry (Dns.GetHostName ()).AddressList.Where (ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault ()?.ToString ();

			}

		}

		public void AddEndpoint(IWebEndpoint interpreter) {
		
		}

		public override void Start ()
		{
			m_shouldRun = true;
			while (m_shouldRun) {

				try {

					//using(TcpClient client = m_listener.AcceptSocket()) {
					

					//}

				} catch (Exception ex) {
				
					Log.x (ex);

				}

			}

		}

		public override void Stop ()
		{
			m_shouldRun = false;

		}
	}
}

