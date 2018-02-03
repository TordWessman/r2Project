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
using System.Net;
using System.Net.Sockets;

namespace Core.Network
{
	
	public static class EndpointExtensions {

		public static string GetAddress(this EndPoint self) {

			return (self as IPEndPoint).Address.ToString();

		}

		public static int GetPort(this EndPoint self) {

			return (self as IPEndPoint).Port;

		}

		/// <summary>
		/// Returns a string representation of the endpoint (address:port)
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="self">Self.</param>
		public static override string ToString(this EndPoint self) {
		
			return $"{GetAddress()}:{GetPort()}";

		}

	}

	public static class TcpClientExtensions {
	
		/// <summary>
		/// Returns a string representation of the remote client 
		/// </summary>
		/// <returns>The remote description.</returns>
		/// <param name="self">Self.</param>
		public static override string ToString(this TcpClient self) {
			
			return self.Client.RemoteEndPoint.GetAddress ();

		}

		/// <summary>
		/// Returns the address of the remote endpoint
		/// </summary>
		/// <returns>The remote address.</returns>
		/// <param name="self">Self.</param>
		public static string GetRemoteAddress(this TcpClient self) {

			return self.Client.RemoteEndPoint.GetAddress();

		}

		/// <summary>
		/// Returns the port of the remote endpoint
		/// </summary>
		/// <returns>The remote port.</returns>
		/// <param name="self">Self.</param>
		public static int GetRemotePort(this TcpClient self) {

			return self.Client.RemoteEndPoint.GetPort();

		}

	}

}