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
using System.Net.NetworkInformation;

namespace R2Core.Network
{

	public static class NetworkExceptionExtensions {
	
		/// <summary>
		/// Returns true if the exception indicates that this was caused by a disconnection
		/// </summary>
		/// <returns><c>true</c> if is closing network the specified ex; otherwise, <c>false</c>.</returns>
		/// <param name="ex">Ex.</param>
		public static bool IsClosingNetwork(this Exception ex) {
		 
			if (ex is SocketException) {

				SocketException socketException = ex as SocketException; 

				return 	socketException.SocketErrorCode == SocketError.Interrupted ||
						socketException.SocketErrorCode == SocketError.ConnectionReset;

			} else if (ex is System.Threading.ThreadAbortException) {
			
				return true;

			} else if (ex.InnerException is SocketException) {

				return ex.InnerException.IsClosingNetwork();

			} 

			return false;

		}

	}

	public static class NetworkStatusCodeExtensions {

		/// <summary>
		/// Simplifies the comparation with a status code 
		/// </summary>
		/// <returns><c>true</c> if is self rawValue; otherwise, <c>false</c>.</returns>
		/// <param name="rawValue">Raw value.</param>
		public static bool HasCode(this INetworkMessage self, NetworkStatusCode? code) {

			return code?.Raw() == self.Code;

		}

		/// <summary>
		/// Simplifies the comparation between a status code and it's integer representation.
		/// </summary>
		/// <returns><c>true</c> if is self rawValue; otherwise, <c>false</c>.</returns>
		/// <param name="rawValue">Raw value.</param>
		public static bool Is(this NetworkStatusCode self, int? rawValue) {

			return rawValue == (int)self;

		}

		/// <summary>
		/// Raw(int)value of a ´NetworkStatusCode´
		/// </summary>
		/// <param name="self">Self.</param>
		public static int Raw(this NetworkStatusCode self) {

			return(int)self;

		}

	}

	public class NetworkExtensions {

		/// <summary>
		/// Pings all ´addresses´ in the list and returns the first to reply.
		/// </summary>
		/// <returns>The available address.</returns>
		/// <param name="addresses">Addresses.</param>
		public static string GetAvailableAddress(System.Collections.Generic.IEnumerable<dynamic> addresses) {

			foreach(dynamic address in addresses) {

				System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
				PingReply reply = ping.Send(address);

				if (reply.Status == IPStatus.Success) {

					return address;

				}

			}

			return null;

		}

	}

	public static class TcpListenerExtensions {
	
		/// <summary>
		/// Blocks until a client has connected. Set the send timeout to ´timeout´.
		/// </summary>
		/// <returns>The connected TcpClient.</returns>
		/// <param name="timeout">Timeout.</param>
		public static TcpClient WaitForConnection(this TcpListener self, int timeout) {

			TcpClient client = self.AcceptTcpClient();
			client.SendTimeout = timeout;
			client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

			return client;

		}

	}

}

