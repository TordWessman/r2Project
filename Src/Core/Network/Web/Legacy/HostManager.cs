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
using System.Collections.Generic;
using Core.Network.Data;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using Core.Device;
using System.IO;

namespace Core.Network
{
	public class HostManager : DeviceBase, IHostManager<IPEndPoint>
	{
		private static readonly int MAXIMUM_BROADCAST_PACKAGE_SIZE = 1024;

		private ICollection<IPEndPoint> m_hosts;
		private NetworkPackageFactory m_networkPackageFactory;
		private IBasicServer<IPEndPoint> m_server;
		private bool m_started;
		private int m_broadcastPort;
		private int m_directPort;
		private ICollection<IHostManagerObserver> m_observers;
		private static readonly object m_lock = new object();
		private Task[] m_broadcastReceiverTasks;
		private string[] m_broadcastReceiverTaskIds;
		private TcpListener m_tcpListener;
	
		public IBasicServer<IPEndPoint> Server { get { return m_server; } }
		public bool IsRunning { get { return m_started; } }
		public override bool Ready { get { return m_started && AnySocketReady(); } }
		public int ConnectedHostsCount { get { return m_hosts.Count; } }

		public bool Has (IPEndPoint endpoint) {  

			return m_hosts.Contains (endpoint); 
		
		}

		public HostManager (string id, NetworkPackageFactory dataPackageFactory, IBasicServer<IPEndPoint> server, int udpBroadcastPort, int tcpDirectPort) 
			: base (id)
		{

			m_hosts = new List<IPEndPoint> ();// {server.LocalEndPoint};
			m_observers = new List<IHostManagerObserver> ();
			m_networkPackageFactory = dataPackageFactory;

			m_broadcastReceiverTasks = new Task[2];
			m_broadcastReceiverTaskIds = new string[2];

			m_broadcastPort = udpBroadcastPort;
			m_directPort = tcpDirectPort;

			m_tcpListener = new TcpListener (IPAddress.Any, m_directPort);

			m_server = server;
			m_server.AddObserver (DataPackageType.RegisterThisHost, this);
			m_server.AddObserver (DataPackageType.RemoveThisHost, this);
		
			m_broadcastReceiverTaskIds [0] = "Broadcast receiver UDP: ";
			m_broadcastReceiverTaskIds [1] = "Broadcast receiver TCP: ";


			m_broadcastReceiverTasks[0] = new Task (() => {

				BroadcastReceiveService (true);

			});

			m_broadcastReceiverTasks[1] = new Task (() => {

				BroadcastReceiveService (false);

			});

		}
		
		public void AddObserver (IHostManagerObserver observer)
		{

			m_observers.Add (observer);
		
		}
		

		private bool AnySocketReady ()
		{
			return true;//(m_udpSocket != null && m_udpSocket.IsBound);
		}

		private Socket CreateSocket(bool udp = true) {

			Socket socket = null;

			if (udp) {

				socket = new Socket (AddressFamily.InterNetwork,                                     
					SocketType.Dgram,  ProtocolType.Udp); 
				socket.EnableBroadcast = true;
				socket.MulticastLoopback = false;

				} else {

				socket = m_tcpListener.AcceptSocket();
					//new Socket (AddressFamily.InterNetwork,                                     
					//SocketType.Dgram,  ProtocolType.Tcp); 

			}
				
			return socket;

		}

		private void BroadcastReceiveService (bool udp)
		{
		
			while (m_started) {
			
				try {		

					IPEndPoint iep = null;

					iep = new IPEndPoint (IPAddress.Any, udp ? m_broadcastPort : m_directPort);  

					Log.d ("Ready to receive registration packages for: " + (udp ? "UDP" : "TCP"));  
					Socket socket = CreateSocket(udp);

					if (udp) {

						socket.Bind (iep);
					
					}

					EndPoint ep = (EndPoint)iep;  

					byte [] buffer = new byte[MAXIMUM_BROADCAST_PACKAGE_SIZE];
					int bytesReceived = socket.ReceiveFrom (buffer, ref ep);
					byte [] input = new byte[bytesReceived];
					Array.Copy (buffer, input, bytesReceived);
								
					if (input != null) {

						IPEndPoint newIp = GetIpEndPointFromDataPackage (input, iep);

						if (newIp != null) {

							AddHost (newIp);
							ClientConnection con = new ClientConnection (newIp);
							con.Send (GetSerializedRegisterMePackage ());
						
						} else {

							Log.d ("Not adding null ip from: " + iep.ToString());
						
						}

					} else {

						throw new ApplicationException ("Null-response from RegisterThisHost. What to do!)!)(!)!");
					
					}
						
					socket.Close (); 

				} catch (SystemException ex) {

					if (ex is SocketException && 
						((ex as SocketException).NativeErrorCode == 10022 || //InvalidArgument
						(ex as SocketException).NativeErrorCode == 10004)) { //Interrupted

						Log.d ("HostManager: [socket death]");
					
					} else if (ex is IOException) {
					
						Log.d ("[io death]");
				
					} else {
					
						Log.w ("Friendly exception: " + ex.ToString ());
					
					}
				
				} catch (Exception ex) {

					Log.x (ex);

				}
			
			}
			
		}
		
		private void AddHost (IPEndPoint newIp)
		{

			lock (m_lock) {
				
				if (!m_hosts.Contains (newIp)) {
					
					Log.d ("Found and adding host: " + newIp.ToString ());
					
					m_hosts.Add (newIp);
					
				} else {

					Log.d ("Will not add duplicated host host: " + newIp.ToString ());	
				
				}
				
				foreach (IHostManagerObserver observer in m_observers) {

					observer.NewHostIdentified (newIp);
				
				}
			
			}
		
		}
		
		public void HostDropped (IPEndPoint newIp)
		{

			lock (m_lock) {
				
				if (m_hosts.Contains (newIp)) {
					
					Log.d ("Removing host: " + newIp.ToString ());
					
					m_hosts.Remove (newIp);
					
				} else {
				
					Log.d ("Will not Remove host, since it was not found: " + newIp.ToString ());	
			
				}
				
				foreach (IHostManagerObserver observer in m_observers) {

					observer.HostDropped (newIp);

				}
			
			}

		}
		
		public override void Stop ()
		{
			Log.d ("HostManager is broadcasting its dismissal..");
			
			IDataPackage removeMe = m_networkPackageFactory.CreateRemoveHostPackage (m_server.Ip, m_server.Port.ToString ());
			
			//Sends its dismissal to all hosts.
			SendToAll (removeMe, false);
			
			m_started = false;

			m_tcpListener.Stop ();
			/*
			foreach (Socket socket in m_udpSocket) {
				if (socket != null) {
					socket.Close ();
				}
			}*/
		}
		
		public override void Start ()
		{

			m_started = true;

			m_tcpListener.Start ();

			for (int i = 0; i < m_broadcastReceiverTasks.Length; i++) {

				m_broadcastReceiverTasks [i].Start ();
			
			}

		}
		
		private string ParseIP (EndPoint host) {
		
			return Regex.Replace (host.ToString (), @"\:[\d]+", String.Empty);
		
		}
		
		public void Broadcast (string ip, int port) {
		
			Broadcast(new IPEndPoint(IPAddress.Parse(ip), port));
		
		}

		public void RegisterMe (string ip, int port) {

			RegisterMe (new IPEndPoint (IPAddress.Parse (ip), port));
		
		}

		public bool RegisterMe (IPEndPoint host) {

			try {

				TcpClient client = new TcpClient ();
				//client. = 10000;

				Log.t ("Connecting to: " + host.ToString ());
				client.Connect (host);

				Log.t ("I woild like to be registered: " + host.ToString ());

				byte[] registerMePackage = GetSerializedRegisterMePackage ();
				Stream stream = client.GetStream ();

				stream.Write (registerMePackage, 0, registerMePackage.Length);
				Log.t ("Sent my info to host and will close " + host.ToString ());
				stream.Close ();
				client.Close ();
				Log.t ("Done registering");
			
			} catch (Exception ex) {

				Log.x (ex);
				return false;
			
			}

			return true;

		}

		public void Broadcast (IPEndPoint host = null)
		{

			if (host != null) {
			
				Log.d ("broadcast explicitly to host: " + host.ToString ());
			
			} else {

				Log.d ("broadcast to everyone");
			
			}
			
			IPEndPoint ep = host != null ? host :
				new IPEndPoint (IPAddress.Broadcast, m_broadcastPort);

			if (ParseIP (ep).Equals (m_server.Ip)) {

				Log.w ("Trying to broadcast to self. Aborting.");
				return;
			
			}

			Socket sock = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, 
				ProtocolType.Udp);  
			
			sock.EnableBroadcast = true;

			sock.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);  

			Log.d ("Sending register me packet");
			sock.SendTo (GetSerializedRegisterMePackage(), ep);  
			
			sock.Close ();  
			
		}
		
		private byte[] GetSerializedRegisterMePackage() {

			IDataPackage registerMePackage = m_networkPackageFactory.CreateRegisterHostPackage (
				m_server.Ip,
				m_server.Port.ToString ());

			return m_networkPackageFactory.Serialize (registerMePackage);

		}
		
		public void SendToAll (IDataPackage package, bool async = true)
		{
		
			foreach (IPEndPoint host in m_hosts) {

				IPEndPoint hostCopy = host;
				Log.d ("Sending package: " + package.GetType ().ToString () + " to host: " + host.ToString ());

				if (async) {
				
					Task.Factory.StartNew (() => {
					
						SendToHost (hostCopy, package);
					
					});

				} else {

					SendToHost (hostCopy, package);
				
				}
				
			}
			
		}
		
		private void SendToHost (IPEndPoint host, IDataPackage package)
		{
			try {

				ClientConnection c = new ClientConnection (host);
				c.SendTimeout = 1000;
				c.Send (m_networkPackageFactory.Serialize (package));
				c.Close ();
			
			} catch (Exception ex) {
			
				Log.w ("Unable to send: " + package.GetType ().ToString () + " to host: " + host.ToString () + " since it was offline ("+ ex.Message + ")"); 			
			
			}
		
		}


		#region ITaskMonitored implementation
		public IDictionary<string,Task> GetTasksToObserve ()
		{
			
			IDictionary<string,Task> tasks = new Dictionary<string, Task> ();
			
			for (int i = 0; i < m_broadcastReceiverTasks.Length; i++) {

				tasks.Add (m_broadcastReceiverTaskIds [i], m_broadcastReceiverTasks [i]);
			
			}
			
			return tasks;

		}
		#endregion

		/// <summary>
		/// Determines whether this instance is a simple (mobile) client by checking the first byte of the package
		/// </summary>
		/// <returns><c>true</c> if this instance is client return true</returns>
		/// <param name="input">Input.</param>
		private bool IsClientPackage(byte[]input) {
			return false;
		}

		private IPEndPoint GetIpEndPointFromDataPackage (byte[] input, IPEndPoint ep)
		{

			IDataPackageHeader header = m_networkPackageFactory.UnserializeHeader (input);
					
			string ip = header.GetValue (HeaderFields.Ip.ToString ());
			int port = int.Parse (header.GetValue (HeaderFields.Port.ToString ()));
					
			//string epIp = ParseIP (ep);
			
			//Console.WriteLine ("vill prata: " + ip + ":" + port + " jag är: " + m_server.LocalIP + ":" + m_server.Port);
			
			if (ip == m_server.Ip && port == m_server.Port) {

				return null;
				
				//if (!epIp.Equals (ip)) {
				//	Log.w ("Found host: " + ip + " with port: " + port + " from strange endpoint: " + epIp);
				//}
			}
			
			return new IPEndPoint (IPAddress.Parse (ip), port);

		}

		#region IDataReceived implementation

		public byte[] DataReceived (DataPackageType type, byte[] input, IPEndPoint ep)
		{
			
			IPEndPoint newIp = GetIpEndPointFromDataPackage (input, ep);
			
			if (newIp != null) {

				switch (type) {

				case DataPackageType.RegisterThisHost:
				
					AddHost (newIp);
					break;

				case DataPackageType.RemoveThisHost:
				
					HostDropped (newIp);
					break;
				
				default:
				
					throw new NotImplementedException ("HostManager got strange package: " + type.ToString ());
				
				}
			
			} else {
			
				Log.d ("Ignoring message from local ip: " + m_server.Ip);
			
			}
			
			return null;

		}

		#endregion

	}

}

