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
using Core;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using Core.Net.Extensions;
using System.Security.Cryptography;

namespace Core.Net
{
	internal class TCPConnection : IByteArrayConnection
	{
		private Socket _socket;
		private bool _isRunning;
		private IPEndPoint _remoteIpEndPoint;
		private NetworkStream _networkStream;
		private IDataReceived<byte []> _observer;
		private bool _busy;
		
		//private readonly byte [] PACKAGE_CONTROL = {0xFF, 0, 0xFF, 0, 0xFF, 0, 0xFF, 0};
		private const int MD5_HASH_LENGTH = 16;
		private const int DEFAULT_BUFFER_SIZE = 4096;
		
		private Object _lock = new Object();
		
		
		public TCPConnection (TcpClient client, IDataReceived<byte []> observer)
		{
			if (client == null)
				throw new NullReferenceException ("Client must not be null.");
			
			if (!client.Connected)
				throw new ApplicationException ("Client not connected!");
				
			_socket = client.Client;
			_isRunning = true;
			_observer = observer;
			
			_remoteIpEndPoint = _socket.RemoteEndPoint as IPEndPoint;
			
			Log.d("Connection initiated: " + IP, this.ToString());
		
		}
		
		public bool Ready 
		{
			get 
			{
				return _networkStream != null &&
					   _socket.Connected;
			}
		}
		
		public bool Busy 
		{
			get 
			{
				return _busy;
			}
		}
		
		public string IP 
		{
			get 
			{
				return _remoteIpEndPoint.Address + ":" + _remoteIpEndPoint.Port;
			}
		}
				
		
		public void Start() {
			
			while (_isRunning) {
				using (_networkStream = new NetworkStream(_socket)) {
					
					//Log.d ("Waiting read...");
					WaitForData(_networkStream, ref _isRunning);
					//Log.d ("...done waiting");

					lock (_lock) 
					{
						//Console.WriteLine ("Unlocked and preparing to read."); //
						Thread.Sleep(100); //
						if (_isRunning && _networkStream.CanRead && _networkStream.DataAvailable) 
						{
							_busy = true;
							
							Log.d("About to get from: " + IP);
							
							byte [] hash1 = ReadStream (_networkStream, MD5_HASH_LENGTH);
							
							int contentLength = BitConverter.ToInt32(ReadStream (_networkStream, sizeof (int)),0);
							
							if (contentLength < 1)
								throw new ApplicationException("Content length in header was: " + contentLength);
							
							byte []buffer = ReadStream (_networkStream,contentLength);
							
							byte [] hash = GenerateCheckSum (buffer);
							
							if (!CompareArrays (hash1 ,hash))
								throw new ApplicationException ("Hash1 compare failed for: " + IP);

							byte [] hash2 = ReadStream (_networkStream, MD5_HASH_LENGTH);
							
							if (!CompareArrays (hash2 ,hash))
								throw new ApplicationException ("Hash2 compare failed for: " + IP);
							
							_observer.DataReceived(buffer, IP);
						
							Log.d("Received data from: " + IP);
							_busy = false;
						}
					}
					
					
				}
			}
			
			Log.d ("Connection finished.");
			
			
			
		}
		
		private void WaitForData (NetworkStream stream, ref bool condition) 
		{
			
			while (!stream.DataAvailable || !stream.CanRead) 
			{
				if (!condition) {
					Log.d ("WILL DIE!");
						return;
				}
				
				
				Thread.Sleep(500);
				Console.Write ("-");
				//Log.d ("-");
			}
			
			
				
		}
		
		private byte [] GenerateCheckSum (byte [] buffer) {
			
			if (buffer == null || buffer.Length < 1)
				throw new ApplicationException ("Bad buffer size");
			
			byte [] hash = new MD5CryptoServiceProvider ().ComputeHash (buffer);
			
			if (hash.Length != MD5_HASH_LENGTH)
				throw new ApplicationException ("Bad md5 hash length!");
			
			return hash;
			
		}
		
		private byte [] ReadStream (NetworkStream stream, int expectedSize = -1) {
			
			//Log.d ("ReadStream: " + expectedSize);
			
			byte [] tmpBuffer = new byte [DEFAULT_BUFFER_SIZE < expectedSize ? 
				DEFAULT_BUFFER_SIZE : expectedSize];
			
			int size = 0;
			int bytesRead = 0;
			IList<byte[]> chunks = new List<byte[]>();
			
			while (size < expectedSize)
			{
				int readSize = expectedSize - size < tmpBuffer.Length ?
							   expectedSize - size : tmpBuffer.Length;
				
				bytesRead = stream.Read(tmpBuffer, 0, readSize);
				
				if (bytesRead > 0) 
				{
					byte [] tmp = new byte[bytesRead];
					System.Array.Copy(tmpBuffer,tmp, bytesRead);
					chunks.Add(tmp);
					size += bytesRead;
				}
			}
			
			byte []buffer = new byte[size];
			int offset = 0;
			foreach (byte [] chunk in chunks) 
			{
				System.Array.Copy(chunk,0, buffer, offset, chunk.Length);
				
				offset += chunk.Length;
			}
					
			return buffer;
			
		}
		
		private bool CompareArrays (byte [] array1, byte [] array2) 
		{
			if (array1 == null && array2 == null)
				return true;
			
			if (array1 == null || array2 == null)
				return false;
			
			if (array1.Length != array2.Length)
				return false;
			
			for (int i = 0; i < array1.Length; i++)
				if (array1[i] != array2[i])
					return false;
			
			return true;
			
		}
		
		
		
		public void Stop () 
		{
			lock (_lock) 
			{
				
				Log.d ("Stopping connection");
				_isRunning = false;
			}
			
			
			//_client.EndConnect();
		}
		
		public void Write (byte [] buffer) 
		{
			
			if (buffer == null)
				throw new NullReferenceException ("Buffer cannot be null!");
			
			if (buffer.Length == 0) 
				throw new ArgumentException ("Length of buffer must be > 0");
			
			if (_busy)
				Log.w("Warning, resource is busy: " + IP);
			//	throw new ApplicationException ("Unable to write. Is busy: " + IP);
			
			
			lock (_lock)
			{
				_busy = true;
				
				while (!_networkStream.CanWrite) 
				{
					Log.w ("Cannot write to stream: " + IP);
					Thread.Sleep(100);
				} 
				
				if (_isRunning)  
				{
				
					//Log.d ("WriteStream
					byte [] size = BitConverter.GetBytes(buffer.Length);
					byte [] hash = GenerateCheckSum (buffer);
					
					_networkStream.Write(hash,0, MD5_HASH_LENGTH);
					_networkStream.Write(size,0, size.Length);
					_networkStream.Write(buffer,0, buffer.Length);
					_networkStream.Write(hash,0, MD5_HASH_LENGTH);
					_networkStream.Flush();

				}
				
				_busy = false;
			}
			
		}
	}
}

