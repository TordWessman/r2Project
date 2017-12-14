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
using System.IO;

namespace Core.Network
{

	/// <summary>
	/// Only used to distinguish failed reads for the BlockingNetworkStream
	/// </summary>
	public class BlockingNetworkException: IOException {

		public BlockingNetworkException(string message) : base (message) {
			
		}

	}

	public class BlockingNetworkStream: NetworkStream
	{
		
		public BlockingNetworkStream (Socket socket) : base(socket) {

			socket.Blocking = true;

		}

		public override int Read (byte[] buffer, int offset, int size) {

			int received = Socket.Receive (buffer, offset, size, SocketFlags.None);

			if (received != size) {

				throw new BlockingNetworkException ($"Unable to retrieve message. Number of bytes read: {received}. Requested: {size} bytes. Host: {Socket.RemoteEndPoint.ToString ()}.");
			}

			return received;
		
		}

	}

}

