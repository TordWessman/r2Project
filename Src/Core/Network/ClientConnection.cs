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
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Timers;

namespace Core.Network
{

	public class ClientConnection
	{
		
		private TcpClient m_client;
		private bool m_ok;
		private IPEndPoint m_host;
		private bool m_keepalive;
		NetworkConnection m_con;

		/// <summary>
		/// Timer for checking connection status
		/// </summary>
		private Timer m_connectionTimer;

		private readonly object m_lock = new object ();

		/// <summary>
		/// Timeout for sending data before closing connection.
		/// </summary>
		private const int SEND_TIMEOUT = 2000;

		/// <summary>
		/// The frequency to check connection status in ms.
		/// </summary>
		private const int CONNECTION_INTERVAL = 1000;

		private IHostManager<IPEndPoint> m_hostManager;

		/// <summary>
		/// Creates a connection upon initialization. If hostManager is specified and keepAlive = true, it will be notified whenever the connection closes (HostDropped).
		/// </summary>
		/// <param name="host">Host.</param>
		/// <param name="hostManager">Host manager.</param>
		/// <param name="keepalive">If set to <c>true</c> keepalive.</param>
		public ClientConnection (IPEndPoint host, IHostManager<IPEndPoint> hostManager = null, bool keepalive = false)
		{
			
			m_keepalive = keepalive;
			m_host = host;
			m_hostManager = hostManager;

			m_client = Connect ();

			if (keepalive && hostManager != null) {
			
				m_connectionTimer = new Timer (CONNECTION_INTERVAL);
				m_connectionTimer.AutoReset = true;
				m_connectionTimer.Elapsed += CheckConnectionStatus;
				m_connectionTimer.Enabled = true;

			}

		}

		private void CheckConnectionStatus (Object source, System.Timers.ElapsedEventArgs e) {

			if (!Connected) {

				m_connectionTimer.Stop ();
				m_connectionTimer.Enabled = false;
				m_client.Close();

				if (m_keepalive) {
			
					m_hostManager.HostDropped(m_host);
					Log.w("Connection unexpectedly lost to host: " + m_host.ToString() + " . Will notify HostManager about this.");

				}
			
			}

		}

		public int SendTimeout { 

			get {
			
				return m_client.SendTimeout;
			
			}

			set {
			
				m_client.SendTimeout = value;
			
			}
		
		}
		
		public int ReceiveTimeout { 
		
			get {
			
				return m_client.ReceiveTimeout;
			
			}

			set {
			
				m_client.ReceiveTimeout = value;
			
			}
		
		}
		
		private TcpClient Connect ()
		{

			TcpClient client = new TcpClient ();
			
			if (m_keepalive) {
			
				client.Client.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
			
			} 
			
			client.SendTimeout = SEND_TIMEOUT;
			
			client.Connect (m_host);
			
			return client;

		}
		
		public bool Connected {

			get { 

				lock (m_lock) {

					if (m_client.Client.Poll (0, SelectMode.SelectRead)) {

						byte[] buff = new byte[1];

						if (m_client.Client.Receive (buff, SocketFlags.Peek) == 0) {

							return false;

						}

					}

					return true;

				}

			} 

		}
		
		public void Close ()
		{

			m_keepalive = false;

			if (m_client.Connected) {
			
				Log.d ("Closing connection actively to: " + m_host.ToString ());
				m_client.Close ();
			
			}
		
		}
		
		public bool Ok {
		
			get {
			
				return m_ok;
			
			}
		
		}
		
		public byte[] Send (byte[]output)
		{

			lock (m_lock) {

				m_con = new NetworkConnection (m_client.GetStream ());

				m_con.WriteData (output, m_keepalive);
				byte [] input = m_con.ReadData ();
				
				m_keepalive = m_con.Keepalive;
				
				if (!m_keepalive && m_client.Connected) {
				
					//Force closing if still connected without keepAlive
					m_client.Close ();
					
				}
				
				if (m_con.Status != NetworkConnectionStatus.OK) {

					throw new NotImplementedException ("Implement reconnection etc here.");
				
				}
				
				if ((PackageTypes)input [RawPackageFactory.PACKAGE_TYPE_POSITION] == PackageTypes.DefaultPackage) {

					PackageFactoryDefaults defaultType = (PackageFactoryDefaults)input [RawPackageFactory.PACKAGE_SUB_TYPE_POSITION];
					
					if (defaultType == PackageFactoryDefaults.BadChecksum) {
						
						throw new NotImplementedException ("Implement resend etc here.");
						
					} else if (defaultType == PackageFactoryDefaults.NoReceiver) {
						
						Log.w ("Got Package of type: " + defaultType.ToString ());
						
					} else if (defaultType == PackageFactoryDefaults.Error) {
						
						if (input.Length > RawPackageFactory.PACKAGE_HEADER_SIZE) {
							
							Log.w ("Received error package");
							
							using (MemoryStream m = new MemoryStream(input)) {
								
								m.Seek (RawPackageFactory.PACKAGE_HEADER_SIZE, SeekOrigin.Current); //Skip the header size part
								
								string errorMessage = "Unknown error message.";
								
								using (BinaryReader reader = new BinaryReader(m)) {

									errorMessage = reader.ReadString ();
								
								}
								
								Log.e ("ERROR PACKAGE: " + errorMessage);
								throw new ApplicationException ("Received error package: " + errorMessage + " from: " + m_host.Address.ToString ());
								
							}
							
							//TODO: UNREACHABLE... what to do with errors? nobody knows.
							byte [] message = new byte[input.Length - sizeof(byte)];
							
							Array.Copy (input, 
							           RawPackageFactory.PACKAGE_HEADER_SIZE, 
							           message,
							           0, 
							           input.Length - RawPackageFactory.PACKAGE_HEADER_SIZE);
							
							return message;
							
						} else {

							throw new ApplicationException ("Received error with no description from: " + m_host.Address.ToString ());
						
						}
					
					}
					
				} else {

					m_ok = true;
					return input;

				}

				return null;
			
			}

		}

	}

}