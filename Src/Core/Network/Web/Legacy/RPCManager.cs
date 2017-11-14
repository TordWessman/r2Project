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
using Core.Network.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Device;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

namespace Core.Network
{
	public class RPCManager : IRPCManager<IPEndPoint>, IHostManagerObserver
	{

		private INetworkPackageFactory m_networkPackageFactory;
		private IDictionary<IPEndPoint, ClientConnection> m_connections;
		private ITaskMonitor m_taskMonitor;
		private IHostManager<IPEndPoint> m_hostManager;
		
		public RPCManager (IHostManager<IPEndPoint> hostManager, INetworkPackageFactory packageFactory, ITaskMonitor taskMonitor)
		{

			m_hostManager = hostManager;
			m_networkPackageFactory = packageFactory;
			m_hostManager.AddObserver (this);
			m_taskMonitor = taskMonitor;
			m_connections = new Dictionary<IPEndPoint, ClientConnection> ();
			
		}
		
		public ITaskMonitor TaskMonitor { get { return m_taskMonitor; } } 

		~RPCManager ()
		{
			foreach (ClientConnection con in m_connections.Values) {

				con.Close ();
			
			}

		}

		#region INetworkManager implementation
		
		public Task RPCRequest (Guid target, 
		                        string methodName, 
		                        IPEndPoint endPoint)
		{
			
			Task networkTask = Task.Factory.StartNew (() => {

				GetReply (target.ToString(), methodName, endPoint);
			
			});
			
			return networkTask;

		}
		
		public Task RPCRequest<K> (Guid target, string methodName, IPEndPoint endPoint, K data)
		{

			Task networkTask = Task.Factory.StartNew (() => {

				GetReply<K> (target.ToString(), methodName, endPoint, data);

			});
			
			return networkTask;
		
		}
		
		public Task<T> RPCRequest<T,K> (Guid target, string methodName, IPEndPoint endPoint, K data)
		{
			
			Task<T> networkTask = Task.Factory.StartNew<T> (() => {

				byte [] rawInput = GetReply<K> (target.ToString(), methodName, endPoint, data);
				return HandleReply<T>(rawInput);

				});
			
			return networkTask;

		}
		
		public Task<T> RPCRequest<T> (Guid target, string methodName, IPEndPoint endPoint)
		{

			Task<T> networkTask = Task.Factory.StartNew<T> (() => {
				
				byte [] rawInput = GetReply (target.ToString(), methodName, endPoint);
				return HandleReply<T> (rawInput);

			});
			
			return networkTask;

		}
			
		private T HandleReply<T> (byte[] rawInput)
		{
			
			if (rawInput != null) {

				return m_networkPackageFactory.Unserialize<T> (rawInput);
			
			} else {
			
				Log.e ("Did not receive valid message from host");
			
			}

			return default(T);
		
		}

		private byte[] GetReply (string target, string methodName, IPEndPoint endPoint)
		{

			IDataPackage outputPackage = m_networkPackageFactory.CreateRpcPackage (target, methodName);

			if (m_connections.ContainsKey(endPoint)) {

				byte [] rawOutput = m_networkPackageFactory.Serialize (outputPackage);
				return m_connections[endPoint].Send (rawOutput);

			}

			throw new DeviceException ("ERROR: Trying to GetReply from host: " + endPoint.ToString() + " and target: " + target + ", which had no connection.");

		}
		
		private byte[] GetReply<K> (string target, string methodName, IPEndPoint endPoint, K data)
		{

			if (m_connections.ContainsKey(endPoint)) {

				IDataPackage<K> outputPackage = m_networkPackageFactory.CreateRpcPackage<K> (target, methodName, data);
				byte [] rawOutput = m_networkPackageFactory.Serialize<K> (outputPackage);
				return m_connections[endPoint].Send (rawOutput);

			}

			throw new DeviceException ("ERROR: Trying to GetReply from host: " + endPoint.ToString() + " and target: " + target + ", which had no connection.");

		}
		
		
		public byte [] RPCReply (Guid target, string methodName)
		{

			IDataPackage package = m_networkPackageFactory.CreateRpcPackage (target.ToString(), methodName);
			return m_networkPackageFactory.Serialize (package);

		}
			
		public byte [] RPCReply<T> (Guid target, string methodName, T data)
		{

			IDataPackage<T> package = m_networkPackageFactory.CreateRpcPackage<T> (target.ToString(), methodName, data);
			return m_networkPackageFactory.Serialize (package);
		
		}
		
		public T ParsePackage<T> (byte[]rawPackage)
		{

			return m_networkPackageFactory.Unserialize<T> (rawPackage);
		
		}
		
		public bool HostAvailable (IPEndPoint host)
		{

			return m_hostManager.Has (host);
		
		}
	
		#endregion

		#region IHostManagerObserver implementation

		public void NewHostIdentified (IPEndPoint host)
		{

			if (m_connections.ContainsKey (host)) {

				m_connections [host].Close ();
				m_connections.Remove (host);
			
			}

			m_connections.Add (host, new ClientConnection (host, m_hostManager, true));
		
		}

		public void HostDropped (IPEndPoint host)
		{

			if (m_connections.ContainsKey (host)) {

				m_connections.Remove (host);
			
			}
		
		}

		#endregion

	}

}

