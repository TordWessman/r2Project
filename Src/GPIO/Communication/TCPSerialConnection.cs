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
using System.Net.Sockets;
using R2Core.Device;
using R2Core.Network;

namespace R2Core.GPIO
{

    /// <summary>
    /// As with the ArduinoSerialConnector it's used as a connection to R2I2CDeviceRouter devices, but
    /// through a TCP connection.
    /// </summary>
    public class TCPSerialConnection : DeviceBase, ISerialConnection {

        private TcpClient m_client;

        /// <summary>
        /// Remote host address
        /// </summary>
        /// <value>The address.</value>
        public string Address { get; private set; }

        /// <summary>
        /// Remote host port
        /// </summary>
        /// <value>The port.</value>
        public int Port { get; private set; }

        /// <summary>
        /// Timeout in ms before a send operation dies.
        /// </summary>
        public int Timeout;

        public TCPSerialConnection(string id, string host) : this(id, host, Settings.Consts.TCPSerialConnectionDefaultPort()) { }

        public TCPSerialConnection(string id, string host, int port) : base(id) {

            Address = host;
            Port = port;
            Timeout = Settings.Consts.TCPSerialConnectionTimeout();

        }

        public bool ShouldRun { get; private set; } = true;

        public override bool Ready => m_client != null && m_client.IsConnected() && m_client.Connected;

        public byte[] Read() {

            return Read(new BlockingNetworkStream(m_client.GetSocket()));

        }

        public byte[] Send(byte[] data) {

            if (!Ready) { throw new InvalidOperationException($"Unable to send. {this} is not connected."); }

            if (data.Length > 0xFF) { throw new ArgumentException("Can't send packages larger than 255 bytes."); }

            lock (this) {

                BlockingNetworkStream stream = new BlockingNetworkStream(m_client.GetSocket());
                stream.Write(new byte[1] { (byte)data.Length }, 0, 1);
                stream.Write(data, 0, data.Length);

                return Read(stream);

            }

        }

        public override void Start() {

            Log.i($"TCPSerial Connecting to {Address}:{Port}", Identifier);

            m_client = new TcpClient {
                SendTimeout = Timeout,
                ReceiveTimeout = Timeout,
            };

            m_client.NoDelay = true;

            m_client.Client.Blocking = true;
            m_client.Connect(Address, Port);
            ShouldRun = true;
            Log.i("TCPSerial Connected", Identifier);

        }

        public override void Stop() {

            ShouldRun = false;
            if (m_client?.Connected == true) { m_client.Close(); }

        }

        private byte[] Read(NetworkStream stream) {
        
            // First byte should contain the size of the rest of the transaction.
            int responseSize = stream.ReadByte();

            byte[] responseData = new byte[responseSize];

            stream.Read(responseData, 0, responseSize);
            return responseData;

        }

    }

}
