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

        /// <summary>
        /// Will be <see langword="true"/> if a transmission is in progress;
        /// </summary>
        /// <value><c>true</c> if is sending; otherwise, <c>false</c>.</value>
        public bool IsSending { get; private set; }

        /// <summary>
        /// Returns true if the client is in a process of connecting
        /// </summary>
        /// <value><c>true</c> if is connecting; otherwise, <c>false</c>.</value>
        public bool IsConnecting { get; private set; }

        /// <summary>
        /// The interval the Start() method waits following a successful connection. This gives the node time to configure before any requests are being made.
        /// </summary>
        public int ConnectionWaitInterval = 2000;

        public bool ShouldRun { get; private set; } = true;

        public override bool Ready => m_client != null && m_client.IsConnected() && m_client.Connected && !IsSending;

        public TCPSerialConnection(string id, string host) : this(id, host, Settings.Consts.TCPSerialConnectionDefaultPort()) { }

        public TCPSerialConnection(string id, string host, int port) : base(id) {

            Address = host;
            Port = port;
            Timeout = Settings.Consts.TCPSerialConnectionTimeout();

        }

        public byte[] Read() {

            if (IsSending) { throw new InvalidOperationException("Unable to Read, due to ongoing transmission."); }

            lock (this) {

                return Read(new BlockingNetworkStream(m_client.GetSocket()));

            }

        }

        public byte[] Send(byte[] data) {

            if (!Ready) { throw new InvalidOperationException($"Unable to send. {this} is not connected."); }

            if (data.Length > 0xFF) { throw new ArgumentException("Can't send packages larger than 255 bytes."); }

            if (IsSending) { throw new InvalidOperationException("Unable to Send, due to ongoing transmission."); }

            if (IsConnecting) { throw new InvalidOperationException("Unable to Send, due to ongoing connection establishment."); }

            lock (this) {

                try {

                    BlockingNetworkStream stream = new BlockingNetworkStream(m_client.GetSocket());

                    Flush(stream);

                    stream.Write(new byte[1] { (byte)data.Length }, 0, 1);
                    stream.Write(data, 0, data.Length);

                    var bytes = Read(stream);

                    return bytes;

                } catch (Exception ex) {

                    Log.d(ex.Message);

                    throw ex;

                } finally {

                    IsSending = false;

                }

            }

        }

        public override void Start() {

            if (IsConnecting) { throw new InvalidOperationException("Unable to Start, due to ongoing connection establishment."); }

            lock (this) {

                IsConnecting = true;

                try {

                    Log.i($"TCPSerial Connecting to {Address}:{Port}", Identifier);

                    m_client = new TcpClient {
                        SendTimeout = Timeout,
                        ReceiveTimeout = Timeout,
                    };

                    m_client.NoDelay = true;

                    m_client.Client.Blocking = true;
                    m_client.Connect(Address, Port);
                    ShouldRun = true;

                    System.Threading.Thread.Sleep(ConnectionWaitInterval);

                    Log.i("TCPSerial Connected", Identifier);

                } finally {

                    IsConnecting = false;
                
                }

            }

        }

        public override void Stop() {

            ShouldRun = false;
            if (m_client?.Connected == true) { m_client.Close(); }
            m_client = null;
            System.Threading.Thread.Sleep(ConnectionWaitInterval);

        }

        private byte[] Read(NetworkStream stream) {
        
            // First byte should contain the size of the rest of the transaction.
            int responseSize = stream.ReadByte();

            byte[] responseData = new byte[responseSize];

            stream.Read(responseData, 0, responseSize);
            stream.Flush();
            return responseData;

        }

        private void Flush(BlockingNetworkStream stream) {

            if (m_client.GetSocket().Available > 0) {

                int receiveTimeout = m_client.GetSocket().ReceiveTimeout;
                m_client.GetSocket().ReceiveTimeout = 100;

                int i = 1000;

                try {
                    while (i != -1) { i = stream.ReadByte(); }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                } catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body

                m_client.GetSocket().ReceiveTimeout = receiveTimeout;

            }

        }

    }

}
