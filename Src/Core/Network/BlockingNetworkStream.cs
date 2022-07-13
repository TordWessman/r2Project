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
using System.Net.Sockets;

namespace R2Core.Network
{

	/// <summary>
	/// Only used to distinguish failed reads for the BlockingNetworkStream
	/// </summary>
	public class BlockingNetworkException : SocketException {

		private string m_message;

		public BlockingNetworkException(string message) : base(10053) {
		
			m_message = message;

		}

        public override string Message => m_message;

	}

	public class BlockingNetworkStream : NetworkStream {
		
		public BlockingNetworkStream(Socket socket) : base(socket) {

			socket.Blocking = true;

		}

		public override void Write(byte[] buffer, int offset, int size) {

			base.Write(buffer, offset, size);
		
        }

		public override int Read(byte[] buffer, int offset, int size) {

			int totalBytesReceived = 0;
			int bytesLeft = size;

			while (totalBytesReceived < size) {

				int received = Socket.Receive(buffer, offset + totalBytesReceived, bytesLeft, SocketFlags.None);
				totalBytesReceived += received;
				bytesLeft -= received;

				if (totalBytesReceived > size) {

					Log.w($"Unable to retrieve message. Number of bytes read: {totalBytesReceived}. Requested: {size} bytes. Host: {Socket.GetEndPoint()?.ToString()}.");

				}

			}

			return totalBytesReceived;
		
		}

	}

}

