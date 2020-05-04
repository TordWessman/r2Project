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
using R2Core.Network;
using R2Core.Device;

namespace R2Core.Tests
{
	public class ClientConnectionWebEndpoint : DeviceBase, INetworkConnection {
		
		private IWebObjectReceiver m_endpoint;

        public bool Busy { get; private set; }

        public ClientConnectionWebEndpoint(IWebObjectReceiver endpoint) : base("dummy_client_connection") {
			
			m_endpoint = endpoint;
		
		}

		public INetworkMessage Send(INetworkMessage message) {

            try {

                Busy = true;
                return m_endpoint.OnReceive(message, new System.Net.IPEndPoint(0, 1));

            } finally {

                Busy = false;

            }

		}

        public string LocalAddress => "localhost";
        public string Address => "localhost";
        public int Port => 4242;

		public void StopListening() {

			throw new NotImplementedException();

		}

	}

}

