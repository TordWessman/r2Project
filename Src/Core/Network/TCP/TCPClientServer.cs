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
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace R2Core.Network {

    public class TCPClientServer : ServerBase {

        private static string AddressKey;
        private static string PortKey;
        private static string ServerTypeKey;

        private TcpClient m_client;
        private ITCPPackageFactory<TCPMessage> m_serializer;

        private IDictionary<string, IServer> m_servers;

        // Keeps track of the connectivity of a socket
        private ConnectionPoller m_connectionPoller;

        /// <summary>
        /// Timeout in ms before an operation dies.
        /// </summary>
        public int Timeout = 30000;

        /// <summary>
        /// Timeout before the connection resets (tries to reconnect)
        /// </summary>
        public int ResetTimeout = 30 * 60 * 1000;

        /// <summary>
        /// The identity used to route requests to this server.
        /// The identity is set by calling ´Configure´.
        /// </summary>
        /// <value>The identity.</value>
        public IIdentity Identity { get; private set; }

        /// <summary>
        /// The remote address to the router.
        /// </summary>
        /// <value>The address.</value>
        public string Address { get; private set; }

        public override bool Ready => base.Ready && m_client != null && m_client?.IsConnected() == true;

        public TCPClientServer(string id, ITCPPackageFactory<TCPMessage> serializer) : base(id) {

            m_serializer = serializer;
            m_servers = new Dictionary<string, IServer>();
            AddressKey = Settings.Consts.ConnectionRouterHeaderClientAddressKey();
            PortKey = Settings.Consts.ConnectionRouterHeaderClientPortKey();
            ServerTypeKey = Settings.Consts.ConnectionRouterHeaderServerTypeKey();

        }

        /// <summary>
        /// Set the configuration parameters required for connecting to a remote router.
        /// </summary>
        /// <param name="identity">Identity of this instance.</param>
        /// <param name="address">Address to the router.</param>
        /// <param name="port">Port to the router.</param>
        public void Configure(IIdentity identity, string address, int port) {

            Address = address;
            Identity = identity;
            SetPort(port);

        }

        public void AddServer(string headerIdentifier, IServer server) {

            m_servers[headerIdentifier] = server;

        }

        protected override void Cleanup() {

            m_connectionPoller?.Stop();
            m_connectionPoller = null;
            m_client?.Close();

        }

        public override void Start() {

            Connect();

        }

        public override INetworkMessage Interpret(INetworkMessage request, IPEndPoint source) {

            IWebEndpoint endpoint = GetEndpoint(request.Destination);

            if (endpoint == null) {

                Log.w($"No IWebEndpoint accepts: {request}", Identifier);

                return new NetworkErrorMessage(NetworkStatusCode.NotFound, $"Path not found: {request.Destination}", request);

            }

            try {

                return endpoint.Interpret(request, source);

            } catch (Exception ex) {

                Log.x(ex, Identifier);

                return new NetworkErrorMessage(NetworkStatusCode.ServerError, $"EXCEPTION: {ex.Message}", request);

            }

        }

        protected override void Service() {

            while (ShouldRun) {

                TCPMessage request = default(TCPMessage);
                INetworkMessage response = default(TCPMessage);

                try {

                    if (!m_client.IsConnected()) {

                        Log.i("TcpClient not connected.", Identifier);
                        return;

                    }

                    request = m_serializer.DeserializePackage(new BlockingNetworkStream(m_client.GetSocket()));

                    if (request.IsPing()) {

                        response = new PongMessage();

                    } else if (request.IsPong()) {

                        return;

                    } else if (request.Headers?.ContainsKey(ServerTypeKey) == true) {

                        string serverType = request.Headers[ServerTypeKey] as string;

                        if (m_servers.ContainsKey(serverType)) {

                            IPEndPoint clientEndpoint = null;

                            if (request.Headers.ContainsKey(AddressKey) &&
                                request.Headers.ContainsKey(PortKey)) {

                                IPAddress address = IPAddress.Parse((string)request.Headers[AddressKey]);

                                long port = (long)request.Headers[PortKey];
                                clientEndpoint = new IPEndPoint(address, (int)port);

                            }

                            response = m_servers[serverType].Interpret(request, clientEndpoint);

                        } else {

                            response = new NetworkErrorMessage(NetworkStatusCode.ResourceUnavailable, $"Missing server type: '{serverType}'.", request);

                        }

                    } else {

                        IWebEndpoint endpoint = GetEndpoint(request.Destination);

                        if (endpoint != null) {

                            response = new TCPMessage(Interpret(request, m_client.GetEndPoint()));

                        } else {

                            response = new NetworkErrorMessage(NetworkStatusCode.UnableToProcess, $"Missing header: '{ServerTypeKey}' and no local endpoint for '{request.Destination}'.");

                        }

                    }

                } catch (Exception ex) {

                    response = null;

                    if (ex.IsClosingNetwork()) {

                        Log.i($"Closing network: '{ex.Message}'", Identifier);

                    } else if (ex is SocketException) {

                        Log.i($"Lost connection to {Address}:{Port}. Will reconnect. ", Identifier);
                        Reconnect();
                        return;

                    } else {

                        Log.x(ex, Identifier);
                        response = new NetworkErrorMessage(ex);

                    }

                }

                if (response != null) {

                    response.OverrideHeaders(new Dictionary<string, object> {
                        { Settings.Consts.ConnectionRouterHeaderHostNameKey(), Identity.Name }
                    });

                    response.Destination = request.Destination;
                    byte[] requestData = m_serializer.SerializeMessage(new TCPMessage(response));

                    new BlockingNetworkStream(m_client.GetSocket()).Write(requestData, 0, requestData.Length);

                }

            }

        }

        private INetworkMessage SendAttachMessage() {

            TCPMessage attachMessage = new TCPMessage {
                Destination = Settings.Consts.ConnectionRouterAddHostDestination(),
                Payload = new RoutingRegistrationRequest {
                    HostName = Identity.Name,
                    Address = m_client.GetLocalEndPoint()?.GetAddress(),
                    Port = m_client.GetLocalEndPoint()?.GetPort() ?? 0
                }
            };

            byte[] requestData = m_serializer.SerializeMessage(attachMessage);
            new BlockingNetworkStream(m_client.GetSocket()).Write(requestData, 0, requestData.Length);
            return m_serializer.DeserializePackage(new BlockingNetworkStream(m_client.GetSocket()));

        }

        private bool Reconnect() {

            Stop();
            return Connect();

        }

        private bool Connect() {

            ShouldRun = true;

            m_client = new TcpClient {
                SendTimeout = Timeout,
                ReceiveTimeout = ResetTimeout
            };

            Log.i($"Connecting to {Address}:{Port}.", Identifier);

            m_connectionPoller = new ConnectionPoller(m_client, () => {

                if (ShouldRun) {

                    Log.i("Lost connection. Will reconnect.", Identifier);
                    Reconnect();

                }

            });

            INetworkMessage response = default(TCPMessage);

            try {

                m_client.Connect(Address, Port);

                response = SendAttachMessage();

            } catch (Exception ex) {

                Log.x(ex, Identifier);
                return false;

            }

            if (response.Code == NetworkStatusCode.Ok.Raw()) {

                base.Start();
                Log.i($"Did connect to: {Address}:{Port}.", Identifier);

            } else {

                Log.e($"Got bad reply [{response.Code}]: {response.Payload}", Identifier);
                m_client.Close();
                return false;

            }

            m_connectionPoller.Start();

            return true;

        }

        public override string ToString() => $"TCPClientServer [Connected: {Ready}. Local port: {m_client?.GetLocalEndPoint()?.Port}]";

    }

}

