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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.IO;

namespace Core.Network
{
	
	public enum NetworkConnectionStatus {
		None = 0,
		OK = 1,
		BadContentLength = 2,
		Hash1Failed = 3,
		Hash2Failed = 4
	}
	
	public class NetworkConnection
	{
		private const int MD5_HASH_LENGTH = 16;
		private const int DEFAULT_BUFFER_SIZE = 4096;
		
		private NetworkConnectionStatus m_status;
		private bool m_keepalive;
		private NetworkStream m_networkStream;

		public NetworkConnection (NetworkStream stream)
		{

			m_status = NetworkConnectionStatus.None;
			m_networkStream = stream;
			m_keepalive = false;
		
		}
		
		public bool Keepalive { get { return m_keepalive; } }
		public NetworkConnectionStatus Status { get { return m_status; } }
		
		public byte[] ReadData ()
		{

			byte [] hashAndSize = ReadStream (MD5_HASH_LENGTH + sizeof(int)); 
			byte [] hash_in1 = new byte[MD5_HASH_LENGTH];
			byte [] size = new byte[sizeof(int)];
			
			Array.Copy (hashAndSize, 0, hash_in1, 0, MD5_HASH_LENGTH);
			Array.Copy (hashAndSize, MD5_HASH_LENGTH, size, 0, size.Length);
			
			int contentLength = BitConverter.ToInt32 (size, 0);
					
			if (contentLength < 1) {

				m_status = NetworkConnectionStatus.BadContentLength;
				return null;
			
			}
			
			byte [] bufferHashKeepAlive = ReadStream (contentLength + MD5_HASH_LENGTH + 1);

			byte [] buffer = new byte [contentLength];				
			byte [] hash_in2 = new byte[MD5_HASH_LENGTH];
			
			Array.Copy (bufferHashKeepAlive, 0, buffer, 0, contentLength);
			Array.Copy (bufferHashKeepAlive, contentLength, hash_in2, 0, MD5_HASH_LENGTH);
			
			byte [] hash = GenerateCheckSum (buffer);

			if (!CompareArrays (hash_in1, hash)) {

				m_status = NetworkConnectionStatus.Hash1Failed;
				return null;
			
			}
							
			if (!CompareArrays (hash_in2, hash)) {
			
				m_status = NetworkConnectionStatus.Hash2Failed;
				return null;
			
			}
			
			// = ReadStream (1);
			
			m_keepalive = bufferHashKeepAlive [bufferHashKeepAlive.Length - 1] == 1 ? true : false;
			
			m_status = NetworkConnectionStatus.OK;
			//Log.d ("READ DATA FROM NETWORK: " + buffer.Length);
			return buffer;

		}
		
		public void WriteData (byte[] buffer, bool keepalive)
		{
		
			byte [] size = BitConverter.GetBytes (buffer.Length);
			byte [] hash = GenerateCheckSum (buffer);
			//byte [] keepaliveByte = new byte[1] {keepalive ? (byte)1 : (byte)0};
			
			byte [] output = new byte [size.Length + hash.Length * 2 + buffer.Length + 1];
			
			System.Array.Copy (hash, 0, output, 0, hash.Length);
			System.Array.Copy (size, 0, output, hash.Length, size.Length);
			System.Array.Copy (buffer, 0, output, hash.Length + size.Length, buffer.Length);
			System.Array.Copy (hash, 0, output, buffer.Length + hash.Length + size.Length, hash.Length);

			output [output.Length - 1] = keepalive ? (byte)1 : (byte)0;
			m_networkStream.Write(output,0,output.Length);
			m_networkStream.Flush ();

		}

		private  byte [] ReadStream (int expectedSize = -1)
		{

			byte [] tmpBuffer = new byte [DEFAULT_BUFFER_SIZE < expectedSize ?
                    DEFAULT_BUFFER_SIZE : expectedSize];

			int size = 0;
			int bytesRead = 0;
			IList<byte[]> chunks = new List<byte[]> ();
	
			while (size < expectedSize) {

				int readSize = expectedSize - size < tmpBuffer.Length ?
                                           expectedSize - size : tmpBuffer.Length;

				bytesRead = m_networkStream.Read (tmpBuffer, 0, readSize);

				if (bytesRead > 0) {
				
					byte [] tmp = new byte[bytesRead];
					System.Array.Copy (tmpBuffer, tmp, bytesRead);
					chunks.Add (tmp);
					size += bytesRead;
				
				} else {

					SocketException ex = new SocketException (10004); //Interrupted
					throw ex;
				
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

			if (array1 == null && array2 == null) {
			
				return true;

			}

			if (array1 == null || array2 == null) {
			
				return false;

			}

			
			if (array1.Length != array2.Length) {
			
				return false;

			}

			
			for (int i = 0; i < array1.Length; i++) {

				if (array1[i] != array2[i]) {

					return false;

				}

			}

			return true;
			
		}
		
		public static byte [] GenerateCheckSum (byte[] buffer)
		{
			
			if (buffer == null || buffer.Length < 1) {
			
				throw new ArgumentException ("Bad buffer size");

			}

			byte [] hash = new MD5CryptoServiceProvider ().ComputeHash (buffer);
			
			if (hash.Length != MD5_HASH_LENGTH) {

				throw new ApplicationException ("Bad md5 hash length!");

			}

			return hash;
			
		}

	}

}

