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
using R2Core.Memory;
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
	public class WebFactory : DeviceBase
	{
		
		private IDeviceManager m_deviceManager;

		private ISerialization m_serialization;

		public WebFactory (string id, IDeviceManager deviceManager, ISerialization serialization) : base (id) {
		
			m_deviceManager = deviceManager;
			m_serialization = serialization;

		}

		public ISerialization Serialization { get { return m_serialization; } }

		public IServer CreateHttpServer (string id, int port) {

			return new HttpServer (id, port, m_serialization);

		}

		public IMessageClient CreateHttpClient(string id) {

			return new HttpClient(id, m_serialization);

		}

		public R2Core.Network.HttpMessage CreateHttpMessage(string url) {
		
			return new R2Core.Network.HttpMessage () { Destination = url };

		}

		/// <summary>
		/// Creates a receiver capable of routing device access.
		/// </summary>
		/// <returns>The device object receiver.</returns>
		/// <param name="security">Security.</param>
		public IWebObjectReceiver CreateDeviceObjectReceiver() {
		
			return new DeviceRouter ();

		}

		/// <summary>
		/// Creates an endpoit that serves files. localPath is the path where files are stored, uriPath is the uri path (i.e. /images) and contentType is the contentType returned to the client (if contentType="images", the content-type header will be image/[file extension of the file being served.])
		/// </summary>
		/// <returns>The file endpoint.</returns>
		/// <param name="localPath">Local path.</param>
		/// <param name="uriPath">URI path.</param>
		/// <param name="contentType">Content type.</param>
		public IWebEndpoint CreateFileEndpoint (string localPath, string uriPath) {

			return new WebFileEndpoint (localPath, uriPath);

		}

		/// <summary>
		/// Creates an endpoint capable of receiving and returning Json-data..
		/// </summary>
		/// <returns>The json endpoint.</returns>
		/// <param name="deviceListenerPath">Device listener path.</param>
		/// <param name="receiver">Receiver.</param>
		public IWebEndpoint CreateJsonEndpoint(string uriPath, IWebObjectReceiver receiver) {

			return new WebJsonEndpoint (uriPath, receiver, m_serialization);

		}

		public TCPMessage CreateTCPMessage(string path, dynamic payload, IDictionary<string, object> headers = null) {
		
			TCPMessage message = new TCPMessage () {
				Destination = path,
				Payload = payload,
				Headers = headers
			};

			return message;

		}

		public ITCPPackageFactory<TCPMessage> CreateTcpPackageFactory() {

			return new TCPPackageFactory (m_serialization);

		}

		public IMessageClient CreateTcpClient(string id, string host, int port) {
		
			return new TCPClient (id, CreateTcpPackageFactory (), host, port);

		}

		public IServer CreateTcpServer(string id, int port) {
		
			return new TCPServer (id, port, new TCPPackageFactory (m_serialization));

		}

		public IServer CreateUdpServer(string id, int port) {
		
			return new UDPServer (id, port, new TCPPackageFactory (m_serialization));
		}

		public UDPBroadcaster CreateUdpClient(string id, int port) {
		
			return new UDPBroadcaster(id, port, new TCPPackageFactory (m_serialization));

		}

		/// <summary>
		/// `port` denotes the porth which the broadcast receiver (i.e. UDPServer) listen to whereas `destination` is the endpoint path (i.e. "/devices").
		/// </summary>
		/// <returns>The host manager.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="port">Port.</param>
		/// <param name="destination">Destination.</param>
		public HostManager CreateHostManager(string id, int port, string destination, IDeviceManager deviceManager) {
		
			return new HostManager (id, port, destination, deviceManager, this);
		
		}

	}

}