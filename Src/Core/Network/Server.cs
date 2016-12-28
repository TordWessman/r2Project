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
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Core.Network.Data;
using Core.Device;
using System.Text;
using System.Linq;

namespace Core.Network
{

	public class Server : DeviceBase, IBasicServer<IPEndPoint>
	{

		public const int DEFAULT_PORT = 1234;
		public const int DEFAULT_MAX_INSTANCES = 2;
		
		private int m_port;
		private bool m_isRunning;
		private TcpListener m_tcpListener;
		private int m_maxClients;
		private Socket[] m_sockets;
		private Task[] m_serviceTasks;
		private string[] m_serviceTaskIds;
		
		//Dictionary containing references to observers
		private IDictionary<DataPackageType,IDataReceived<byte[], IPEndPoint>> m_observers;
		private IClientMessageObserver m_clientObserver;
		private RawPackageFactory m_packageFactory;

		public override bool Ready { get { return m_isRunning; } }
	
		public int Port { get { return m_port; } }
		
		public string Ip { 

			get {
				
				return Dns.GetHostEntry (Dns.GetHostName ()).AddressList.Where (ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault ()?.ToString ();
			 
			}

		}
		
		public void PrintConnections ()
		{
			for (int i = 0; i < m_serviceTasks.Length; i++) {

				if (m_sockets [i] != null && m_serviceTaskIds [i] != null && m_serviceTasks [i] != null) {
					
					string ip = "[" + (m_sockets [i].Connected ? m_sockets [i].RemoteEndPoint.ToString () : "") + "]";
					Log.d (  ip + ":" + m_serviceTaskIds [i] + " " + m_serviceTasks [i].Status.ToString () + " " + m_sockets [i].Connected.ToString ());
			
				}

			}

		}
		
		public IPEndPoint LocalEndPoint { 
		
			get {
		
				return new IPEndPoint (IPAddress.Parse (Ip), Port);
			
			}
		
		}
 
		
		public Server (string id, int port = -1) : base (id)
		{
			
			m_packageFactory = new RawPackageFactory ();
			m_observers = new Dictionary<DataPackageType,IDataReceived<byte[], IPEndPoint>> ();

			m_maxClients = DEFAULT_MAX_INSTANCES;

			m_serviceTasks = new Task[m_maxClients];
			m_serviceTaskIds = new string[m_maxClients];
			
			if (port == -1) {

				m_port = DEFAULT_PORT;
			
			} else {
			
				m_port = port;
			
			}

			m_sockets = new Socket[m_maxClients];
			m_tcpListener = new TcpListener (IPAddress.Any, m_port);
			
			for (int i = 0; i < m_maxClients; i++) {
				
				int tempInstance = i;

				m_serviceTasks [tempInstance] =
					new Task (() => {
					
						Service (tempInstance);
				
					});
				
				m_serviceTaskIds [i] = "Server thread: " + i;
				
			}

		}

		public void AddObserver (DataPackageType  type, IDataReceived<byte[], IPEndPoint> observer)
		{

			if (m_observers.ContainsKey (type)) {

				throw new NotImplementedException ("Support for multiple observers for single type not implemented. Type: " + type.ToString ());
			
			}

			m_observers.Add (type, observer);
		
		}

		private void Service (int instance)
		{

			while (m_isRunning) {

				NetworkConnection con = null;

				try {	

					bool keepalive = true;

					using (m_sockets [instance] = m_tcpListener.AcceptSocket ()) { //TODO: <- Invalid arguments här

						m_sockets [instance].SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
						//m_sockets [instance].NoDelay = true; // Disable the Nagle Algorithm for this tcp socket.
						m_sockets [instance].SendTimeout = 1000;
						//m_sockets [instance].ReceiveTimeout = 10000;

						//DOES NOT WORK WITH ANDROID-SOCKETS: m_sockets [instance].SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

						Log.t ("STARTING NEW CONNECTION: " + m_sockets [instance].RemoteEndPoint.ToString () + " instance: " + instance);

						while (m_sockets [instance].Connected && keepalive) {

							con = new NetworkConnection (new NetworkStream (m_sockets [instance]));

							byte [] input = con.ReadData ();
							keepalive = con.Keepalive;

							//Log.t ("Got data from: + " + m_sockets [instance].RemoteEndPoint.ToString () + " is keepalive:: " + keepalive);

							if (con.Status == NetworkConnectionStatus.Hash1Failed ||
								con.Status == NetworkConnectionStatus.Hash2Failed) {

								keepalive = false;
								con.WriteData (m_packageFactory.CreateDefaultPackage (PackageFactoryDefaults.BadChecksum), keepalive);

							} else if (con.Status == NetworkConnectionStatus.BadContentLength) {

								keepalive = false;
								con.WriteData (m_packageFactory.CreateDefaultPackage (PackageFactoryDefaults.BadLength), keepalive);

							} else if (con.Status == NetworkConnectionStatus.OK) {

								byte [] output = HandleInput (input, instance);

								if (output != null) {

									con.WriteData (output, keepalive);
									//Log.t ("Did write data to: + " + m_sockets [instance].RemoteEndPoint.ToString () + " is connected:: " + m_sockets [instance].Connected);

								} else {

									Log.w ("NO KNOWN RESPONSE FOR INSTANCE: " + instance);
									keepalive = false;
									con.WriteData (m_packageFactory.CreateDefaultPackage (PackageFactoryDefaults.NoKnownResponse), keepalive);

								}

							} else {

								throw new NotImplementedException ("con.Status not identified");
							
							}

						}

						//Log.d ("DEATH TO CONNECTION: + " + m_sockets [instance].RemoteEndPoint.ToString () + " instance: " + instance);

					}

				} catch (AccessViolationException ex) {

					// Security error

					if (con != null) {

						con.WriteData (m_packageFactory.CreateDefaultPackage (PackageFactoryDefaults.Error), false);

					}

				} catch (Exception ex) {

					if (ex is SocketException && 
						((ex as SocketException).NativeErrorCode == 10022 || //InvalidArgument
						(ex as SocketException).NativeErrorCode == 10004)) { //Interrupted

						Log.d ("Server: [socket death]");
					
					} else if (ex is IOException) {
					
						Log.d ("[io death]");
					
					} else {
					
						Log.w ("Friendly exception: " + ex.ToString ());
					
					}
				
				}

			}

		}
		
		private byte [] HandleInput (byte[] input, int instance)
		{

			byte [] output = null;

			try {
					
				PackageTypes baseType = NetworkUtils.GetBasePackageType (input);
				//byte subType = NetworkUtils.GetSubPackageType (input);
				
				switch (baseType) {

				case PackageTypes.DefaultPackage:
					
					//if (subType == (byte)PackageFactoryDefaults.RegisterMe) {
					//	output = GetObserverOutput (input, (IPEndPoint)m_sockets [instance].RemoteEndPoint);
					//} else {
					throw new NotImplementedException ("Default package not implemented for server. Instance: " + instance);
				//}
				//break;
				case PackageTypes.DataPackage:

					output = GetObserverOutput (input, (IPEndPoint)m_sockets [instance].RemoteEndPoint);
							
					if (output == null) {
						output = m_packageFactory.CreateDefaultPackage (PackageFactoryDefaults.NoReceiver);
					} 
					break;

				case PackageTypes.ClientPackage:
					
					byte [] stringData = new byte[input.Length - 1];
					Array.Copy (input, 1, stringData, 0, input.Length - 1);
					string outString = System.Text.Encoding.UTF8.GetString (stringData);
					//Log.t ("input: " + outString);
					
					string result = m_clientObserver.MessageReceived ((IPEndPoint)m_sockets [instance].RemoteEndPoint, outString);
					//Log.t ("output: " + result);
					output = Encoding.UTF8.GetBytes (result);
					break;

				default:

					string msg = "Package of type: " + baseType + " not implemented. Instance: " + instance;
					Log.e (msg);
				
					output = m_packageFactory.CreateDefaultPackage (
						PackageFactoryDefaults.Error,
						msg
					);

					break;
				}
						
			} catch (Exception ex) {

				Log.x (ex);

				output = m_packageFactory.CreateDefaultPackage (
					PackageFactoryDefaults.Error,
					ex.Message + " " + ex.StackTrace
				);

			}
			
			return output;

		}
		
		private byte[] GetObserverOutput (byte[] input, IPEndPoint remote)
		{
			Console.WriteLine ("HIHI: " + NetworkUtils.GetSubPackageType (input).ToString () + " from: " + remote.ToString ());
				
			byte [] output = null;
			
			if (m_observers.ContainsKey (NetworkUtils.GetSubPackageType (input))) {

				output = m_observers [NetworkUtils.GetSubPackageType (input)].DataReceived (
					NetworkUtils.GetSubPackageType (input),
					input,
					remote);

				if (output == null) {

					output = m_packageFactory.CreateDefaultPackage (PackageFactoryDefaults.DataReceived);
				
				}
			
			}
			
			return output;
		
		}

		public override void Start ()
		{
		
			if (m_isRunning) {

				throw new InvalidOperationException ("Server already started!");
			
			}
			
			m_isRunning = true;
			
			m_tcpListener.Start ();
			
			for (int i = 0; i < m_maxClients; i++) {
			
				m_serviceTasks [i].Start ();
			
			}

		}
		
		
		public override void Stop ()
		{
			
			Log.d ("Stopping Server. No network operations will be permitted...");
			m_isRunning = false;
			m_tcpListener.Stop ();
			
			for (int i = 0; i < m_sockets.Length; i++) {

				if (m_sockets [i] != null) { 

					//if (m_sockets [i].IsBound) {
					m_sockets [i].Close ();
					//}
						
				}
				
			}

			Log.d ("Stopping server");
			
			for (int i = 0; i < m_sockets.Length; i++) {

				//Console.WriteLine (m_sockets [i].
			
			}

		}

		#region ITaskMonitored implementation

		public IDictionary<string,Task> GetTasksToObserve ()
		{
		
			IDictionary<string,Task> tasks = new Dictionary<string, Task> ();
			
			for (int i = 0; i < m_serviceTasks.Length; i++) {
		
				tasks.Add (m_serviceTaskIds [i], m_serviceTasks [i]);
			
			}
			
			return tasks;
		
		}

		#endregion

		#region IBasicServer implementation

		public void SetClientObserver (IClientMessageObserver observer)
		{

			m_clientObserver = observer;
		
		}

		#endregion
	
	}

}

