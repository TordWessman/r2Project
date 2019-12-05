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
using R2Core.Device;
using Newtonsoft.Json;
using System.Net.NetworkInformation;

namespace R2Core.Network
{
	/// <summary>
	/// Called whenever the connection receives an INetworkMessage.
	/// </summary>
	public delegate INetworkMessage OnReceiveHandler(INetworkMessage request, IPEndPoint address);

	/// <summary>
	/// Called whenever the client disconnected. `Exception` is not null if the connection was gracefully disconnected.
	/// </summary>
	public delegate void OnDisconnectHandler(IClientConnection connection, Exception ex);

	/// <summary>
	/// Represents a connection (INetworkConnection) that is capable of detecting disconnection and new messages (i.e. broadcast messages)
	/// </summary>
	public interface IClientConnection : INetworkConnection {

		/// <summary>
		/// Called upon asynchronous message retrieval
		/// </summary>
		event OnReceiveHandler OnReceive;

		/// <summary>
		/// Called whenever the client is disconnected.
		/// </summary>
		event OnDisconnectHandler OnDisconnect;

	}

	/// <summary>
	/// Check if the host connection replies to ping messages.
	/// </summary>
	public static class IClientConnectionExtensions {
	
		public static bool Ping(this IClientConnection self) {
		
			System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
			PingReply reply = ping.Send(self.Address);
			return reply.Status == IPStatus.Success;

		}

		public static int GetHashCode(this IClientConnection self) {
		
			return self.Identifier.GetHashCode();
		
		}

	}

}

