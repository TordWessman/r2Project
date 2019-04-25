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
using R2Core.Device;
using System.Threading;

namespace R2Core.Network
{

	/// <summary>
	/// Represents the TCPServer's connection to a TCPClient 
	/// </summary>
	public class TCPServerConnection : DeviceBase, IClientConnection {

		// Delegate called when a message has been received
		private Func<INetworkMessage,IPEndPoint,INetworkMessage> m_responseDelegate;

		// Accept-client provided by the server
		private TcpClient m_client;

		// Used to serialize/deserialize packages
		private ITCPPackageFactory<TCPMessage> m_packageFactory;

		// Thread listening on incomming data
		private Task m_listener;

		// Lock for network writes
		private readonly object m_writeLock = new object();

		// Keeps track of weither this instance has been stopped or nor
		private bool m_shouldRun = false;

		// Contains an Exception from the Received if thrown there.
		private Exception m_readError;

		// Contains the latest previous broadcast response
		private TCPMessage m_broadcastResponse;

		// Re-do every failed poll.
		private bool m_previousPollSuccess = true;

		// Responsible for making sure the connection has not been lost
		PingService m_ping;

		private System.Timers.Timer m_connectionCheckTimer;

		public event OnReceiveHandler OnReceive;
		public event OnDisconnectHandler OnDisconnect;

		public TCPServerConnection(
			string id, 
			ITCPPackageFactory<TCPMessage> factory, 
			TcpClient client,
			Func<INetworkMessage,IPEndPoint,INetworkMessage> responseDelegate) : base(id) {

			m_responseDelegate = responseDelegate;
			m_client = client;
			m_packageFactory = factory;
			m_ping = new PingService(this, m_client.SendTimeout);

			m_connectionCheckTimer = new System.Timers.Timer(m_client.SendTimeout);
			m_connectionCheckTimer.Elapsed += ConnectionCheckEvent;

		}

		~TCPServerConnection() {
		
			Log.d($"Deallocating {this}.");
		
		}

		public string Address { get { return m_client.GetEndPoint()?.GetAddress(); } }

		public int Port { get { return m_client.GetEndPoint()?.GetPort() ?? 0; } }

		public override bool Ready { get { return m_shouldRun && m_client?.IsConnected() == true; } }

		public override void Start() {

			m_shouldRun = true;

			Log.d($"Server accepted connection from {m_client.GetDescription()}.");

			m_listener = Task.Factory.StartNew(() => {
			
				using(m_client) {
				
					while(m_shouldRun && m_client.Connected) { Connection(); }

				}

			});

			m_connectionCheckTimer.Start();
			//m_ping.Start();

		}

		public override void Stop() { Disconnect(); }

		/// <summary>
		/// Sends a BroadcastMessage
		/// </summary>
		/// <param name="message">Message.</param>
		public INetworkMessage Send(INetworkMessage message) {
			
			if (!Ready) {

				throw new InvalidOperationException($"Unable to send ´{message}´. TCPServerConnection ´{this} is´ not running.");

			}

			Write(new BroadcastMessage(message, Address, Port));

			return new OkMessage();

		}

		public override string ToString() {

			return $"TCPServerConnection [`{m_client.GetDescription()}`. Ready: {Ready}]."; //string.Format("[TCPClient: Connected={0}, Host={1}, Ip={2}]", Ready, m_host, m_port);

		}

		private void Disconnect(Exception ex = null) {

			m_shouldRun = false;

			Log.d($"{this} disconnected.");

			m_connectionCheckTimer?.Stop();

			//m_ping.Stop();

			try {

				if (m_client.IsConnected()) { m_client.Close(); }

			} catch (Exception exception) { 

				Log.e($"Client disconnection [{this}] crashed:");
				Log.x(exception); 
			
			}

			if (OnDisconnect != null) { OnDisconnect(this, ex); } 

		}

		private void Reply(TCPMessage clientRequest) {
		
			try {

				INetworkMessage responseToClient = m_responseDelegate(clientRequest, (IPEndPoint) m_client.GetEndPoint());

				// Only write directly back to stream if not a broadcast message. 
				Write(new TCPMessage(responseToClient));

			} catch (Exception ex) {

				Write(new NetworkErrorMessage(ex, clientRequest));

			}

		}

		private void Connection() {

			TCPMessage clientRequest = default(TCPMessage);

			try {
				
				clientRequest = m_packageFactory.DeserializePackage(new BlockingNetworkStream(m_client.GetSocket()));

				if (!m_client.IsConnected()) {

					if (m_shouldRun) { Stop(); }
					return;

				} else if (clientRequest.IsPong()) { return; }

				else if (clientRequest.IsPing()) {

					m_ping.Pong();

				} else if (!clientRequest.IsBroadcastMessage()) {
					
					Reply(clientRequest);

				} else if (clientRequest.IsBroadcastMessage()) {
				
					m_broadcastResponse = clientRequest;

				}

				if (!clientRequest.IsPingOrPong()) {
				
					if (OnReceive != null) { OnReceive(clientRequest, m_client.GetEndPoint()); }

				}

			} catch (Exception ex) {

				m_readError = ex;

				if (m_shouldRun) {

					if (!ex.IsClosingNetwork()) {
					
						Log.x(ex);

					}

					Disconnect(m_shouldRun ? ex : null);

				}
			
			} 

		}

		private void Write(INetworkMessage message) {
			
			lock(m_writeLock) {

				try {

					Socket socket = m_client.GetSocket();

					if (socket == null) {

						Log.w($"{this} was unable to connect to socket.");
						if (m_shouldRun) { Stop(); }

					} else {

						byte[] response = m_packageFactory.SerializeMessage(new TCPMessage(message));
						new BlockingNetworkStream(m_client.Client).Write(response, 0, response.Length);

					}

				} catch (Exception ex) {

					Log.x(ex);
					throw ex;

				}
			
			}

			if (m_shouldRun && !Ready) {
			
				// I'm disconnected, but not quite aware of it yet.
				Stop();

			}

		}

		private void ConnectionCheckEvent(object sender, System.Timers.ElapsedEventArgs e) {
		
			if (!m_shouldRun) { return; }

			bool pollSuccessful = m_client.GetSocket()?.Poll(m_client.ReceiveTimeout * 1000, SelectMode.SelectError) ?? false;

			if (Ready || !(pollSuccessful && m_previousPollSuccess)) {

				Disconnect();

			}

			m_previousPollSuccess = pollSuccessful;

		}

	}

}

