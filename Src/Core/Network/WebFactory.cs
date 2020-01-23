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

﻿using System;
using R2Core.Device;
using System.Linq;
using System.Dynamic;
using R2Core.Data;
using System.Collections.Generic;

namespace R2Core.Network
{
	/// <summary>
	/// Creates various http components
	/// </summary>
	public class WebFactory : DeviceBase {
		
		private ISerialization m_serialization;
		private ITCPPackageFactory<TCPMessage> m_packageFactory;

		// IMessageClient header header field for host name
		private static string HeaderHostName = Settings.Consts.ConnectionRouterHeaderHostNameKey();

		public WebFactory(string id, ISerialization serialization) : base(id) {

			m_serialization = serialization;
			m_packageFactory = CreateTcpPackageFactory();
	
		}

		public ISerialization Serialization { get { return m_serialization; } }

		public HttpServer CreateHttpServer(string id, int port) {

			return new HttpServer(id, port, m_serialization);

		}

		public IMessageClient CreateHttpClient(string id, string hostName = null) {

			IMessageClient client = new HttpClient(id, m_serialization);

			client.SetHeader(HeaderHostName, hostName);
			client.SetHeader(Settings.Consts.ConnectionRouterHeaderServerTypeKey(), Settings.Consts.ConnectionRouterHeaderServerTypeHTTP());

			return client;

		}

		public HttpMessage CreateHttpMessage(string url, string hostName = null) {
		
			HttpMessage message = new HttpMessage() { Destination = url };
			message.SetHostName(hostName);
			return message;

		}

		/// <summary>
		/// Creates a receiver capable of routing device access.
		/// </summary>
		/// <returns>The device object receiver.</returns>
		/// <param name="security">Security.</param>
		public IWebObjectReceiver CreateDeviceRouter(IDeviceContainer container) {
		
			return new DeviceRouter(container);

		}

		/// <summary>
		/// Creates an endpoit that serves files. localPath is the path where files are stored, uriPath is the uri path (i.e. /images) and contentType is the contentType returned to the client(if contentType="images", the content-type header will be image/[file extension of the file being served.])
		/// </summary>
		/// <returns>The file endpoint.</returns>
		/// <param name="localPath">Local path.</param>
		/// <param name="uriPath">URI path.</param>
		/// <param name="contentType">Content type.</param>
		public IWebEndpoint CreateFileEndpoint(string localPath, string uriPath) {

			return new WebFileEndpoint(localPath, uriPath);

		}

		/// <summary>
		/// Creates an endpoint capable of receiving and returning Json-data..
		/// </summary>
		/// <returns>The json endpoint.</returns>
		/// <param name="deviceListenerPath">Device listener path.</param>
		/// <param name="receiver">Receiver.</param>
		public IWebEndpoint CreateJsonEndpoint(IWebObjectReceiver receiver) {

			return new WebJsonEndpoint(receiver, m_serialization);

		}

		public TCPMessage CreateTCPMessage(string path, dynamic payload, IDictionary<string, object> headers = null) {
		
			TCPMessage message = new TCPMessage() {
				Destination = path,
				Payload = payload,
				Headers = headers
			};

			return message;

		}

		public ITCPPackageFactory<TCPMessage> CreateTcpPackageFactory() {

			return new TCPPackageFactory(m_serialization);

		}

		public TCPClient CreateTcpClient(string id, string host, int port, string hostName = null) {
		
			TCPClient client = new TCPClient(id, m_packageFactory, host, port);
			client.SetHeader(HeaderHostName, hostName);
			client.SetHeader(Settings.Consts.ConnectionRouterHeaderServerTypeKey(), Settings.Consts.ConnectionRouterHeaderServerTypeTCP());

			return client;

		}

		public TCPServer CreateTcpServer(string id, int port) {
		
			return new TCPServer(id, port, m_packageFactory);

		}

		public TCPClientServer CreateTcpClientServer(string id) {

			return new TCPClientServer(id, m_packageFactory);

		}

		public TCPRouterEndpoint CreateTcpRouterEndpoint(TCPServer server) {

			return new TCPRouterEndpoint(server);

		}

		public UDPServer CreateUdpServer(string id, int port) {
		
			return new UDPServer(id, port, m_packageFactory);
	
		}

		public UDPBroadcaster CreateUdpClient(string id, int port) {
		
			UDPBroadcaster client = new UDPBroadcaster(id, port, m_packageFactory);
			return client;

		}

		/// <summary>
		/// `port` denotes the porth which the broadcast receiver (i.e. UDPServer) listen to whereas `destination` is the endpoint path (i.e. "/devices").
		/// </summary>
		/// <returns>The host manager.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="port">Port.</param>
		/// <param name="destination">Destination.</param>
		public HostSynchronizer CreateHostSynchronizer(string id, int port, IDeviceManager deviceManager) {
		
			return new HostSynchronizer(id, port, deviceManager, this);
		
		}

		/// <summary>
		/// Creates a dynamic seralizer/deserializer
		/// </summary>
		/// <returns>The serialization.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="encoding">Encoding.</param>
		public ISerialization CreateSerialization(string id, System.Text.Encoding encoding) {

			return new JsonSerialization(id, encoding);

		}

	}

}