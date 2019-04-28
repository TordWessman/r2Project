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
using R2Core.Device;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Timers;

namespace R2Core.Network
{
	/// <summary>
	/// TCP transceiver.
	/// </summary>
	public class TCPClient : DeviceBase, IMessageClient {
		
		string m_host;
		int m_port;
		private TcpClient m_client;
		ITCPPackageFactory<TCPMessage> m_serializer;
		private System.Threading.Tasks.Task m_receiverTask;
		private bool m_shouldRun = false;

		private readonly object m_sendLock = new object();
		private AutoResetEvent m_writeLock;
		private AutoResetEvent m_readLock;

		private IList<WeakReference<IMessageClientObserver>> m_observers;

		// Contains the latest previous response
		private TCPMessage m_latestResponse;

		// Contains an Exception from the Received if thrown there.
		private Exception m_readError;

		// Makes sure the connection is alive
		private PingService m_ping;

		// Keeps track of the connectivity of a socket
		private ConnectionPoller m_connectionPoller;

		/// <summary>
		/// Timeout in ms before a send operation dies.
		/// </summary>
		public int Timeout = 10000;

        /// <summary>
        /// Default headers for all requests
        /// </summary>
		public IDictionary<string, object> Headers;

		public TCPClient(string id, ITCPPackageFactory<TCPMessage> serializer, string host, int port) : base(id) {

			m_host = host;
			m_port = port;
			m_serializer = serializer;
			m_observers = new List<WeakReference<IMessageClientObserver>>();

		}

		~TCPClient() {
		
			Log.d($"Deallocating {this} [{Identifier}:{Guid.ToString()}].");
			Stop();
			m_receiverTask?.Dispose();

		}

		public string Address { get { return m_host; } }
		public int Port { get { return m_port; } }
		public override bool Ready { get { return m_shouldRun && m_client?.IsConnected() == true; } }

        public override void Start() {
		
			m_readError = null;
			m_shouldRun = true;
            m_client = new TcpClient();
            m_client.SendTimeout = Timeout;
			m_client.Client.Blocking = true;
			m_client.Connect(m_host, m_port);
			m_receiverTask = Receive();
			m_ping = new PingService(this, Timeout);
			m_connectionPoller = new ConnectionPoller(m_client, () => {

				if (m_shouldRun) { Stop(); }

			});

			//m_ping.Start();

		}

		public override void Stop() {

			if (!m_shouldRun) { return; }

			Log.d($"{this} will Stop.");

			m_shouldRun = false;

			m_connectionPoller?.Stop();
			m_ping?.Stop();

			if (m_client?.Connected == true) {

				m_client.GetStream().Close();
				m_client.Close();

			}

			m_observers.InParallell((observer) => {

				observer.OnClose(this, m_readError);

			});

			m_receiverTask = null;

		}

		public System.Threading.Tasks.Task SendAsync(INetworkMessage message, Action<INetworkMessage> responseDelegate) {

			return System.Threading.Tasks.Task.Factory.StartNew(() => {

				INetworkMessage response;
				Exception exception = null;

				try {

					response = Send(message);

				} catch (Exception ex) {

					response = new NetworkErrorMessage(ex, message);
					exception = ex;

				}

				responseDelegate(response);

				if (exception != null) { throw exception; }

			});

		}

		public INetworkMessage Send(INetworkMessage requestMessage) {

			if (requestMessage.IsBroadcastMessage()) {
			
				throw new ArgumentException($"TCPClient can't send broadcast messages({requestMessage}).");

			}

			if (!Ready) { throw new InvalidOperationException($"{this} is unable to send. Not connected."); }

			lock(m_sendLock) {

				if (!requestMessage.IsPingOrPong()) {
				
					m_observers.InParallell((observer) => observer.OnRequest(requestMessage));

				}
                
                TCPMessage message = requestMessage is TCPMessage ? ((TCPMessage)requestMessage) : new TCPMessage(requestMessage);

				if (message.Headers != null) { Headers?.ToList().ForEach( kvp => message.Headers.Add(kvp)); }

				message.Headers = message.Headers ?? Headers;

				byte[] request = m_serializer.SerializeMessage(message);

				try {

					new BlockingNetworkStream(m_client.GetSocket()).Write(request, 0, request.Length);

					if (message.IsBroadcastMessage() || message.IsPingOrPong()) {

						// Sender does not expect a reply.
						return new OkMessage();

					}

					m_writeLock = new AutoResetEvent(false);

					// Reset the read error. Still an awful lot of race conditions, but wtf.
					m_readError = null;

					// Wait for receiver thread to fetch data.
					if (m_writeLock.WaitOne(Timeout)) { 

						m_writeLock = null;

						// If there was an error during the read process, throw it here.
						if (m_readError != null) {

							throw m_readError; 

						}

						NetworkException exception = m_latestResponse.IsError() ? new NetworkException(m_latestResponse) : null;

						if (m_latestResponse.IsEmpty()) {
						
							Log.w("TCPClient: Got empty message.");

						}

						if (m_latestResponse.IsPingOrPong() ||
							m_latestResponse.IsBroadcastMessage()) {
						
							throw new NetworkException($"TCPClient: Invalid messag type: {m_latestResponse}");


						}

						m_observers.InParallell((observer) => observer.OnResponse(m_latestResponse, exception));

						return m_latestResponse;
					
					} else {
						
						// Request timed out. Closing connection
						m_readError = new SocketException((int)SocketError.TimedOut);
						throw m_readError;

					}

				} catch (Exception ex) {

					Log.x(ex);
					m_observers.InParallell((observer) => observer.OnResponse(null, ex));
					if (m_shouldRun && !Ready) { Stop(); }
					throw ex;

				}

			}

		}

		public override string ToString() {
			
			return $"TCPClient: `{m_client.GetDescription()}`. Ready: {Ready}.";
		
		}

		private System.Threading.Tasks.Task Receive() {

			return System.Threading.Tasks.Task.Factory.StartNew(() => {
	
				m_readLock = new AutoResetEvent(false);

				while(Ready) {

					TCPMessage response =  default(TCPMessage);

					try {

						if (Ready && m_shouldRun && m_connectionPoller?.Ready == false) {
						
							m_connectionPoller?.Start();
						
						}
							
						response = m_serializer.DeserializePackage(new BlockingNetworkStream(m_client.GetSocket()));

						if (response.IsPong()) { 
							
							continue;  
						
						} else if (response.IsPing()) {

							m_ping.Pong();

						} else if (response.IsBroadcastMessage()) {

                            m_observers.InParallell((observer) => {

                                if (observer.Destination == null || 
									Regex.IsMatch(response.Destination, observer.Destination)) {

									observer.OnBroadcast(response);

                                }

                            });

						} else {
							
							m_latestResponse = response;
					
						}

					} catch (Exception ex) {

						if (!m_shouldRun) { /* ignore errors */ }
						else if (ex.IsClosingNetwork()) {

							Log.d($"{this} closed.");
							Stop();

						} else {
						
							m_readError = ex;

							Log.t(ex);

							Log.w($"{this} disconnected. Error: {ex.GetType()}. Message: `{ex.Message}`.");

							Stop();

						}

					} finally {


						if (!response.IsPingOrPong() && !response.IsBroadcastMessage()) {
						
							m_writeLock?.Set();

						}

					}

				} 

			},System.Threading.Tasks.TaskCreationOptions.LongRunning);

		}

		public void AddObserver(IMessageClientObserver receiver) {

			m_observers.Add(new WeakReference<IMessageClientObserver>(receiver));

		}

	}

}