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

namespace Core.Network
{
	public class UDPServer: ServerBase
	{

		private UdpClient m_listener;
		private IPEndPoint m_groupEndpoint;
		private ITCPPackageFactory m_packageFactory;

		public UDPServer (string id, int port, ITCPPackageFactory packageFactory) : base (id, port) {

			m_listener = new UdpClient (Port);
			m_groupEndpoint = new IPEndPoint(IPAddress.Any, Port);
			m_packageFactory = packageFactory;

		}

		protected override void Service() {
		
			while (ShouldRun) {
			
				byte [] bytes = m_listener.Receive (ref m_groupEndpoint);


			}
		}
	}
}

