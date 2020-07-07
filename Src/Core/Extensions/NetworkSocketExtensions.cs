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
using System.Net;

namespace R2Core.Network
{
	public static class NetworkSocketExtensions
	{

		/// <summary>
		/// Returns true if the client is connected and has not lost connection(!).
		/// </summary>
		/// <returns><c>true</c> if is connected the specified self; otherwise, <c>false</c>.</returns>
		public static bool IsConnected(this TcpClient self) {

			return self.Connected && self.GetSocket()?.IsConnected() == true;

		}

		/// <summary>
		/// Returns the Socket object (or null if it has been disposed).
		/// </summary>
		/// <returns>The socket.</returns>
		public static Socket GetSocket(this TcpClient self) {

			try {

				return self.Client;

			} 
            catch (ObjectDisposedException) { return null; } 
            catch (NullReferenceException) { return null; }

        }

		/// <summary>
		/// Returns true if the ´Socket´ is connected to a remote host.
		/// https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.connected
		/// </summary>
		/// <returns><c>true</c> if is connected the specified client; otherwise, <c>false</c>.</returns>
		public static bool IsConnected(this Socket client) {
			
			try {
				
				return !(client.Poll(1, SelectMode.SelectRead) && client.Available == 0);
			
			} catch (SocketException) { return false; }

		}

		/// <summary>
		/// Returns the remote endpoint (or null, if not connected).
		/// </summary>
		/// <returns>The endpoidnt.</returns>
		public static IPEndPoint GetEndPoint(this TcpClient self) {

			return self.GetSocket()?.GetEndPoint();

		}

		/// <summary>
		/// Returns the local endpoint (or null, if not connected).
		/// </summary>
		/// <returns>The endpoidnt.</returns>
		public static IPEndPoint GetLocalEndPoint(this TcpClient self) {

			return self.GetSocket()?.GetLocalEndPoint();

		}

		/// <summary>
		/// Returns the remote endpoint(or null, if not connected).
		/// </summary>
		/// <returns>The endpoidnt.</returns>
		public static IPEndPoint GetEndPoint(this Socket self) {

			try {

				return (IPEndPoint)self.RemoteEndPoint;

			} catch (SocketException) {

				return null;

			} catch (ObjectDisposedException) {

				return null;

			}

		}

		/// <summary>
		/// Returns the local endpoint(or null, if not connected).
		/// </summary>
		/// <returns>The endpoidnt.</returns>
		public static IPEndPoint GetLocalEndPoint(this Socket self) {

			try {

				return (IPEndPoint)self.LocalEndPoint;

			} catch (SocketException) {

				return null;

			} catch (ObjectDisposedException) {

				return null;

			}

		}


		/// <summary>
		/// Returns a string representation of the remote client 
		/// </summary>
		/// <returns>The remote description.</returns>
		public static string GetDescription(this TcpClient self) {
			
			return self.GetEndPoint()?.GetDescription() ?? "<not connected>";

		}

		/// <summary>
		/// Returns the address of the remote endpoint
		/// </summary>
		/// <returns>The remote address.</returns>
		public static string GetRemoteAddress(this TcpClient self) {

			return self.GetEndPoint()?.GetAddress() ?? "null";

		}

		/// <summary>
		/// Returns the port of the remote endpoint
		/// </summary>
		/// <returns>The remote port.</returns>
		public static int GetRemotePort(this TcpClient self) {

			return self.GetEndPoint()?.GetPort() ?? 0;

		}

	}

	public static class EndpointExtensions {

		/// <summary>
		/// Returns the IPEndPoint's string representation or null.
		/// </summary>
		/// <returns>The address.</returns>
		public static string GetAddress(this EndPoint self) {

			return(self as IPEndPoint)?.Address?.ToString();

		}

		public static int GetPort(this EndPoint self) {

			return(self as IPEndPoint).Port;

		}

		/// <summary>
		/// Returns a string representation of the endpoint(address:port)
		/// </summary>
		/// <returns>The string.</returns>
		public static string GetDescription(this EndPoint self) {

			return $"{self.GetAddress()}:{self.GetPort()}";

		}

	}
}

