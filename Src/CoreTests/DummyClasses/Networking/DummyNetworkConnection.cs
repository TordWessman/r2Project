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
using System.Threading;

namespace R2Core.Tests {

    public class DummyNetworkConnection: DeviceBase, INetworkConnection {

        public int Delay;
        public INetworkMessage NextResponse = new NetworkMessage();
        public DummyNetworkConnection() : base ("dummy_network_connection") {

        }

        public INetworkMessage Send(INetworkMessage request) {

            try {

                m_busy = true;
                Thread.Sleep(Delay);
                return NextResponse;
            
            } finally {

                m_busy = false;
            
            }
        
        }

        private bool m_busy;

        public string Address => "localhost";

        public int Port => 4242;

        public void StopListening() { throw new NotImplementedException(""); }

        public bool Busy => m_busy;
    }
}
