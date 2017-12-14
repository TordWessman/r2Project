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
using Core.Device;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using Core.Network;
using System.Threading;
using System.IO;

namespace Core.Network
{
	/// <summary>
	/// TCP transceiver.
	/// </summary>
	public class TCPClient: DeviceBase, IMessageClient
	{
		string m_host;
		int m_port;
		private TcpClient m_client;
		ITCPPackageFactory<TCPMessage> m_serializer;
		private System.Threading.Tasks.Task m_receiverTask;
		private bool m_shouldRun = false;

		private readonly object m_sendLock = new object();
		private AutoResetEvent m_mutex;

		// Contains the latest previous response
		TCPMessage m_latestResponse;

		// Contains an Exception from the Received if thrown there.
		Exception m_readError;

		/// <summary>
		/// Timeout in ms before a send operation dies.
		/// </summary>
		public int Timeout = 10000;

		public IDictionary<string, object> Headers;
		public IList<IMessageClientObserver> m_observers;

		public TCPClient (string id, ITCPPackageFactory<TCPMessage> serializer, string host, int port) : base(id) {

			m_host = host;
			m_port = port;
			m_client = new TcpClient ();
			m_client.SendTimeout = Timeout;
			m_serializer = serializer;
			m_observers = new List<IMessageClientObserver> ();

		}

		~TCPClient() {
		
			Stop ();
			m_receiverTask?.Dispose ();

		}

		public string Host { get { return m_host; } }
		public int Port { get { return m_port; } }
		public override bool Ready { get { return m_shouldRun && m_client.Connected; } }

		static int apa = 0;

		public override void Start() {
		
			m_readError = null;
			m_shouldRun = true;
			m_client.Connect (m_host, m_port);
			m_receiverTask = Receive (apa++);

		}

		public override void Stop () {

			m_shouldRun = false;

			if (m_client.Connected) {
			
				m_client.GetStream ().Close();
				m_client.Close ();

			}

			m_receiverTask = null;

		}

		public System.Threading.Tasks.Task SendAsync(INetworkMessage message, Action<INetworkMessage> responseDelegate) {

			return System.Threading.Tasks.Task.Factory.StartNew ( () => {

				INetworkMessage response;
				Exception exception = null;

				try {

					response = Send(message);

				} catch (Exception ex) {

					response = new TCPMessage() { Code = WebStatusCode.NetworkError.Raw(), Payload = ex.ToString()};
					exception = ex;

				}

				responseDelegate(response);

				if (exception != null) { throw exception; }

			});

		}


		public INetworkMessage Send(INetworkMessage requestMessage) {

			lock (m_sendLock) {
	
				TCPMessage message = requestMessage is TCPMessage ? ((TCPMessage)requestMessage) : new TCPMessage (requestMessage);

				if (message.Headers != null) { Headers?.ToList().ForEach( kvp => message.Headers.Add(kvp)); }

				message.Headers = message.Headers ?? Headers;

				byte[] request = m_serializer.SerializeMessage (message);

				m_client.GetStream ().Write (request, 0, request.Length);

				m_mutex = new AutoResetEvent (false);

				// Reset the read error. Still an awful lot of race conditions, but wtf.
				m_readError = null;

				// Wait for receiver thread to fetch data.
				if (!m_mutex.WaitOne (Timeout)) { throw new SocketException ((int)SocketError.TimedOut); }

				m_mutex = null;

				try {

					// If there was an error during the read process, throw it here.
					if (m_readError != null) { throw m_readError; }

					return m_latestResponse;

				} finally {
				
					m_readError = null;

				}


			}
	
		}

		public override string ToString () {
			
			return string.Format ("[TCPClient: Connected={0}, Host={1}, Ip={2}]", Ready, m_host, m_port);
		
		}

		private System.Threading.Tasks.Task Receive (int hund) {

			return System.Threading.Tasks.Task.Factory.StartNew (() => {

				while (Ready) {

					try {
						
						m_latestResponse = m_serializer.DeserializePackage (new BlockingNetworkStream(m_client.Client));

					} catch (Exception ex) {

						m_readError = ex;
						Log.e($"TCP Read error. Message: {ex.Message}");
						Stop();

						m_observers.AsParallel()
							.ForAll(observer => observer.OnReceive(null, ex));

					} finally {

						if (m_mutex == null) {

							// No mutex is set. This message should be propagated to observers

							m_observers.AsParallel()
								.Where(observer => observer.Destination == null || System.Text.RegularExpressions.Regex.IsMatch (m_latestResponse.Destination, observer.Destination))
								.ForAll(observer => observer.OnReceive(m_latestResponse, m_readError));
	
						} 

						m_mutex?.Set();

					}

				} 	

			});

		}
		public void AddObserver(IMessageClientObserver receiver) {

			m_observers.Add(receiver);

		}

	}

}