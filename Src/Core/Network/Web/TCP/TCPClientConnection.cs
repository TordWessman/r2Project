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
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Core.Device;
using System.Threading;

namespace Core.Network
{
	public class TCPClientConnection : DeviceBase, IClientConnection
	{
		private TcpClient m_client;
		private ITCPPackageFactory<TCPMessage> m_packageFactory;
		private Task m_listener;
		private readonly object m_writeLock = new object();
		private AutoResetEvent m_readLock;

		public bool ShouldRun = false;
		public event OnReceiveHandler OnReceive;
		public event OnDisconnectHandler OnDisconnect;

		// Contains an Exception from the Received if thrown there.
		private Exception m_readError;

		// Contains the latest previous response
		TCPMessage m_latestResponse;

		public TCPClientConnection (string id, ITCPPackageFactory<TCPMessage> factory, TcpClient client) : base (id) {
			
			m_client = client;
			m_packageFactory = factory;
		
		}

		public override bool Ready { get { return ShouldRun && m_client.Connected; } }

		public IPEndPoint Endpoint { get { return  (IPEndPoint)m_client.Client.RemoteEndPoint; } }

		public override void Start() {

			ShouldRun = true;

			m_listener = Task.Factory.StartNew(() => {
			
				using (m_client) {
				
					while (ShouldRun && m_client.Connected) { Connection(); }

				}

			});

		}

		public override void Stop() { Disconnect (this); }

		public INetworkMessage Send(INetworkMessage message) {

			m_readLock = new AutoResetEvent (false);

			Write (new BroadcastMessage(message));

			// Wait for receiver thread to fetch data.
			if (!m_readLock.WaitOne (m_client.SendTimeout)) { throw new SocketException ((int)SocketError.TimedOut); }

			try {

				// If there was an error during the read process, throw it here.
				if (m_readError != null) { throw m_readError; }

				return m_latestResponse;

			} finally {

				m_readError = null;

			}

		}

		private void Disconnect(IClientConnection connection, Exception ex = null) {
		
			ShouldRun = false;
			if (m_client.Connected) { m_client.Close (); }
			if (OnDisconnect != null) { OnDisconnect (this, ex); }

		}

		private void Connection() {
			
			try {
				 
				m_latestResponse = m_packageFactory.DeserializePackage (new BlockingNetworkStream(m_client.Client));

				if (!m_latestResponse.IsBroadcastMessage()) {

					// Only write directly back to stream if not a broadcast message. 
					Write(new TCPMessage(OnReceive(m_latestResponse, (IPEndPoint) m_client.Client.RemoteEndPoint)));

				}

			} catch (Exception ex) {

				m_readError = ex;

				if (ex.InnerException is SocketException) {
				
					Log.d($"TCP connection aborted ( code{(ex.InnerException as SocketException).SocketErrorCode} ).");
					Disconnect (this);

				} else {

					if (Ready && !m_latestResponse.IsBroadcastMessage()) {
						
						Write (new TCPMessage () { Code = WebStatusCode.ServerError.Raw (),
							#if DEBUG
							Payload = ex.ToString ()
							#endif
						});
					
					} else {
					
						Disconnect (this, ex);

					}

				}
			
			} finally {
			
				m_readLock?.Set();
				m_readLock = null;

			}

		}

		private void Write (INetworkMessage message) {

			lock (m_writeLock) {

				byte[] response = m_packageFactory.SerializeMessage (new TCPMessage (message));
				m_client.GetStream ().Write (response, 0, response.Length);

			}

		}

	}

}

