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

namespace Core.Net.Extensions
{
	public static class NetworkStreamExtensions
	{

		public static byte [] ReadBytes (this NetworkStream stream, int bytesExpected = -1) 
		{
			byte [] tmpBuffer = new byte [bytesExpected];
			
			int size = 0;
			int bytesRead = 0;
			IList<byte[]> chunks = new List<byte[]>();
			
			while (size < bytesExpected)
			{
				bytesRead = stream.Read(tmpBuffer, 0, tmpBuffer.Length);
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
		
	}
}

