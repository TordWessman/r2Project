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
using System;
using System.Net;
using System.Net.Sockets;
using R2Core.Network;

namespace R2Core.GPIO.Tests
{
    public class DummyTCPHost : ServerBase
    {
        private TcpListener m_listener;
        private ISerialConnection m_dummyConnection;

        public DummyTCPHost(ISerialConnection dummyConnection, int port) : base("dummy_server") {

            m_dummyConnection = dummyConnection;
            SetPort(port);

        }

        private TcpClient client;

        public override INetworkMessage Interpret(INetworkMessage request, IPEndPoint source) { throw new NotImplementedException(); }

        protected override void Cleanup() { m_listener.Stop(); }

        public override bool Ready => ShouldRun && m_listener?.Server?.IsBound == true;

        public void Broadcast(byte[] data) {

            BlockingNetworkStream stream = new BlockingNetworkStream(client.GetSocket());

            // Sending size first
            stream.Write(new byte[] { (byte)data.Length }, 0, 1);
            stream.Write(data, 0, data.Length);

        }

        protected override void Service() {

            m_listener = new TcpListener(IPAddress.Any, Port);
            m_listener.Start();

            client = m_listener.WaitForConnection(1000);

            while (ShouldRun) {
                BlockingNetworkStream stream = new BlockingNetworkStream(client.GetSocket());
                int responseSize = stream.ReadByte();

                byte[] readBuffer = new byte[responseSize];

                for (int i = 0; i < readBuffer.Length; i++) {

                    readBuffer[i] = (byte)stream.ReadByte();

                }

                // Sending size first
                stream.Write(new byte[] { (byte)readBuffer.Length }, 0, 1);

                byte[] responseBuffer = m_dummyConnection.Send(readBuffer);
                stream.Write(responseBuffer, 0, responseBuffer.Length);

            }

        }

    }

}
