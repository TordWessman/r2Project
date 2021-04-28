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
using System.Threading;
using System.Text.RegularExpressions;

namespace R2Core.Network
{
	/// <summary>
	/// TCP transceiver.
	/// </summary>
	public class TCPClient : DeviceBase, IMessageClient {
		
		private TcpClient m_client;
		private ITCPPackageFactory<TCPMessage> m_serializer;
		private System.Threading.Tasks.Task m_receiverTask;
		private bool m_shouldRun;

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

		// if false, nothing will be read from remote host.
		private bool m_shouldListen = true;

        public string LocalAddress => m_client?.GetLocalEndPoint()?.GetAddress();
        public string Address { get; private set; }
        public int Port { get; private set; }
        public override bool Ready { get { return m_shouldRun && m_client?.IsConnected() == true; } }
        public bool Busy { get; private set; }

		/// <summary>
		/// Timeout in ms before a send operation dies.
		/// </summary>
		public int Timeout = 30000;

        public IDictionary<string, object> Headers { get; set; }

		public TCPClient(string id, ITCPPackageFactory<TCPMessage> serializer, string host, int port) : base(id) {

			Address = host;
			Port = port;
			m_serializer = serializer;
			m_observers = new List<WeakReference<IMessageClientObserver>>();

		}

		~TCPClient() {
		
			Log.i($"Deallocating TCPClient [{Guid.ToString()}].", Identifier);
			Stop();
			m_receiverTask?.Dispose();

		}

        public override void Start() {
		
			Log.i($"Will connect to {Address}:{Port}", Identifier);

			m_readError = null;
			m_shouldRun = true;
            m_client = new TcpClient {
                SendTimeout = Timeout
            };
            m_client.Client.Blocking = true;
			m_client.Connect(Address, Port);

            m_receiverTask = Receive();
			m_ping = new PingService(this, Timeout);
			m_connectionPoller = new ConnectionPoller(m_client, () => {

				if (m_shouldRun) { Stop(); }

			});

            m_connectionPoller.Start();

        }

		public override void Stop() {

			if (!m_shouldRun) { return; }

			Log.i($"Will Stop ({this}).", Identifier);

			m_shouldRun = false;

			m_connectionPoller?.Stop();
			m_ping?.Stop();

			if (m_client?.Connected == true) {

				m_client.GetStream().Close();
				m_client.Close();

			}

			m_receiverTask = null;

			m_observers.InParallell((observer) => {

				observer.OnClose(this, m_readError);

			});

		}

		public void StopListening() {

			m_shouldListen = false;

		}

		public System.Threading.Tasks.Task SendAsync(INetworkMessage request, Action<INetworkMessage> responseDelegate) {

			return System.Threading.Tasks.Task.Factory.StartNew(() => {

				INetworkMessage response;
				Exception exception = null;

				try {

					response = Send(request);

				} catch (Exception ex) {

					response = new NetworkErrorMessage(ex, request);
					exception = ex;

				}

				responseDelegate(response);

				if (exception != null) { throw exception; }

			});

		}

		public INetworkMessage Send(INetworkMessage request) {

			if (request.IsBroadcastMessage()) {
			
				throw new ArgumentException($"TCPClient can't send broadcast messages ({request}).");

			}

			if (!Ready) { throw new InvalidOperationException($"{this} is unable to send. Not connected."); }

			try {

				Busy = true;

    			lock(m_sendLock) {

    				if (!request.IsPingOrPong()) {
    				
    					m_observers.InParallell((observer) => observer.OnRequest(request));

    				}

                    TCPMessage message = request is TCPMessage ? ((TCPMessage)request) : new TCPMessage(request);

    				message.Headers = message.OverrideHeaders(Headers);

    				byte[] requestData = m_serializer.SerializeMessage(message);

    				try {

    					new BlockingNetworkStream(m_client.GetSocket()).Write(requestData, 0, requestData.Length);

    					if (message.IsBroadcastMessage() || message.IsPingOrPong() || !m_shouldListen) {

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
    						
    							Log.w("Got empty message.", Identifier);

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

    					Log.x(ex, Identifier);
    					m_observers.InParallell((observer) => observer.OnResponse(null, ex));
    					if (m_shouldRun && !Ready) { Stop(); }
    					throw ex;

    				}

    			}

			} finally {

				Busy = false;

			}

		}

		public override string ToString() {
			
			return $"TCPClient: `{m_client?.GetDescription()}`. Ready: {Ready}.";
		
		}

		private System.Threading.Tasks.Task Receive() {

			return System.Threading.Tasks.Task.Factory.StartNew(() => {
	
				m_readLock = new AutoResetEvent(false);

				while(Ready && m_shouldListen) {

					TCPMessage response = default(TCPMessage);

					try {
							
						response = m_serializer.DeserializePackage(new BlockingNetworkStream(m_client.GetSocket()));

						if (response.IsPong()) { continue; } 

                        if (response.IsPing()) {

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

							Log.d($"{this} closed.", Identifier);
							Stop();

						} else {
						
							m_readError = ex;

							Log.w($"{this} disconnected. Error: {ex.GetType()}. Message: `{ex.Message}`.", Identifier);

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

		public void AddClientObserver(IMessageClientObserver observer) {

			m_observers.Add(new WeakReference<IMessageClientObserver>(observer));

		}

	}

}
