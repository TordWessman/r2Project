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
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Core;

namespace Core.Net
{
	public class Server : IDataReceived<byte[]>
	{
		public static readonly int DEFAULT_PORT = 1234;
		
		private int _port;
		private bool _isRunning;
		private TcpListener _tcpListener;
		private IList<IClientConnection> _connectionObservers;
		
		private Object _lock = new Object();
		
		private IDictionary<string, IByteArrayConnection> _connections;
		
		public Server ()
		{
			_port = DEFAULT_PORT;
			_connections = new Dictionary<string, IByteArrayConnection> ();
			_connectionObservers = new List<IClientConnection>();
			
		}
		
		public bool IsBusy (string ip) {
			if (!_connections.ContainsKey(ip))
				throw new ApplicationException ("No ip: " + ip);
			
			return _connections[ip].Busy;
		}
		
		public void Start() 
		{
			_isRunning = true;
			ListenAsync();
		}
		
		public void AddObserver (IClientConnection observer)
		{
			_connectionObservers.Add(observer);
		}
		
		public void Stop () 
		{
			foreach (IByteArrayConnection connection in _connections.Values)
				if (connection.Busy)
					throw new ApplicationException ("Unable to Stop: connection busy: " + connection.IP);
			
			lock (_lock) {
				_isRunning = false;
				_tcpListener.Stop();
				
			}
			
			foreach (IByteArrayConnection connection in _connections.Values)
				connection.Stop();
		}
		
		private void ListenAsync() 
		{
			_tcpListener =  new TcpListener(IPAddress.Any, _port);
			_tcpListener.Start();
			
			_tcpListener.BeginAcceptSocket (EndAcceptTcpClient, _tcpListener);
		}
		
		internal void EndAcceptTcpClient(IAsyncResult asyncResult) 
		{
			
			if (_isRunning) 
			{
					TcpListener listener = asyncResult.AsyncState as TcpListener;
			TcpClient client = listener.EndAcceptTcpClient(asyncResult);
			
			lock (_lock) 
			{
				
				if (client != null &&
					client.Connected)
				{
					IByteArrayConnection connection = new TCPConnection (client, this);
					_connections.Add(connection.IP, connection);
					
					ThreadPool.QueueUserWorkItem( delegate (object obj) { connection.Start(); });
					
					foreach (IClientConnection observer in _connectionObservers)
						if (observer != null)
							observer.ClientConnected(connection.IP);
				} 
				else 
				{
					throw new ApplicationException ("Bad client: " + (client == null ? "NULL" : "not connected"));
					
				}
			}
			
			//TODO: this value should be set relative to TIME_WAIT
			Thread.Sleep(1000);
			_tcpListener.BeginAcceptSocket (EndAcceptTcpClient, _tcpListener);
			//ListenAsync();
			}
		
		}
		
		int count = 0;
		
		public void DataReceived(byte [] data, string ip)	
		{
			
			
			Log.d ((count++) + " Data received with length: " + data.Length + " from: " + ip);
		}
	
		public void Write(byte [] buffer, string ip) 
		{
			
			
			if (!_connections.ContainsKey(ip))
				throw new ApplicationException ("No connection found with ip: " + ip);
			
			IByteArrayConnection connection = _connections[ip];
			connection.Write(buffer);
		}

	}
}

