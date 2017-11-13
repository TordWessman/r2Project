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
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace Core.Network
{
	public class UDPClient: DeviceBase
	{
		ITCPPackageFactory<TCPMessage> m_serializer;
		private IPEndPoint m_host;
		private Socket m_socket;
		private Task m_task;
		private CancellationTokenSource m_cancelationToken;

		// Used to uniquely identify broadcast messages sent by this client. This value will be appended to the headers any message sent.
		private string m_currentMessageId;

		/// <summary>
		/// The maximum size of the packages sent over UDP.
		/// </summary>
		public const int MaximumPackageSize = 1024 * 10;

		public UDPClient (string id, int port, ITCPPackageFactory<TCPMessage> serializer, string address = null) : base(id)
		{

			m_serializer = serializer;
			m_host = new IPEndPoint (address != null ? IPAddress.Parse (address) : IPAddress.Any, port);
			m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
				ProtocolType.Udp);
			
			m_socket.EnableBroadcast = true;
			m_socket.MulticastLoopback = false;

		}

		public override bool Ready {
			
			get {
				
				return m_task?.Status != TaskStatus.Running && m_task?.Status != TaskStatus.WaitingToRun;
			
			}
		
		}

		/// <summary>
		/// Broadcast the specified `message`. The `timeout` determines for how many milliseconds the client should wait for responses. `responseDelegate` is called if `timout` is specified for each response to this specific broadcast request.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="timout">Timout.</param>
		/// <param name="responseDelegate">Response delegate.</param>
		public Task Broadcast(TCPMessage message, int timeout = 0, Action<BroadcastMessage, Exception> responseDelegate = null) {

			if (!Ready) {
			
				throw new InvalidOperationException ("Unable to broadcast. Previous broadcast is not completed.");

			}

			if (message.Headers == null) { message.Headers = new Dictionary<string, object> (); }

			m_currentMessageId = Guid.NewGuid ().ToString();
			message.Headers.Add (BroadcastMessage.BroadcastMessageUniqueIdentifierHeaderKey, m_currentMessageId);

			m_cancelationToken = new CancellationTokenSource();
			m_socket.ReceiveTimeout = timeout;
			m_cancelationToken.CancelAfter (timeout);

			m_task = new Task(() => {
				
				byte[] requestData = m_serializer.SerializeMessage (message);

				if (requestData.Length > MaximumPackageSize) {

					throw new ArgumentException ($"UDP message is to lage ({requestData.Length} bytes). Maximum size is {MaximumPackageSize} bytes. ");
				}

				if (requestData.Length != m_socket.SendTo (requestData, m_host)) {

					throw new System.Net.WebException ($"Bytes sent to host '{m_host.ToString()}' mismatch.");

				}

				try {
				
					WaitForResponse(responseDelegate);

				} catch (System.Net.Sockets.SocketException ex) {

					// Ignore timeouts, since they are expected...

					if (ex.SocketErrorCode != SocketError.TimedOut) {
						
						Log.x(ex);
						responseDelegate.Invoke(null, ex);

					}

				} catch (Exception ex) {
				
					Log.x(ex);
					responseDelegate.Invoke(null, ex);

				}

			});

			m_task.Start ();
			return m_task;

		}

		/// <summary>
		/// Waits for responses until m_cancelationToken is set.
		/// </summary>
		/// <param name="responseDelegate">Response delegate.</param>
		private void WaitForResponse(Action<BroadcastMessage, Exception> responseDelegate) {

			byte[] buffer = new byte[MaximumPackageSize];

			EndPoint remoteHost = (EndPoint)m_host;

			while (!m_cancelationToken.Token.IsCancellationRequested) {

				int length = m_socket.ReceiveFrom (buffer, ref remoteHost);

				INetworkMessage response = m_serializer.DeserializePackage (new MemoryStream (buffer,0, length));
			
				if (response.Headers == null ||
				    !response.Headers.ContainsKey (BroadcastMessage.BroadcastMessageUniqueIdentifierHeaderKey) ||
					m_currentMessageId != response.Headers [BroadcastMessage.BroadcastMessageUniqueIdentifierHeaderKey] as string) {
				
					// Invalid message id header fields. This message was not a reply for something I recently sent.
					continue;
				}

				responseDelegate?.BeginInvoke(new BroadcastMessage(response, (IPEndPoint) remoteHost), null, (asyncResult) => {
					
					try {

						// Make sure we log exceptions in delegates, at least...
						((asyncResult as AsyncResult).AsyncDelegate as Action<BroadcastMessage, Exception>).EndInvoke(asyncResult);

					} catch (Exception ex) {
						
						Log.w("Broadcast client failed to call response deleage with the following exception:");
						Log.x(ex);

					}

				}, null);

			}

		}

	}

}