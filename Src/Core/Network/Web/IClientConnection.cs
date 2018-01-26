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
using Core.Device;
using Newtonsoft.Json;

namespace Core.Network
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
	/// Represents a connection to a client from a server.
	/// </summary>
	public interface IClientConnection : IDevice
	{
		/// <summary>
		/// Writes INetworkMessage to remote. Will bypas the OnReceive handler.
		/// </summary>
		/// <param name="message">Message.</param>
		INetworkMessage Send (INetworkMessage message);

		/// <summary>
		/// Called upon asynchronous message retrieval
		/// </summary>
		event OnReceiveHandler OnReceive;

		/// <summary>
		/// Called whenever the client is disconnected.
		/// </summary>
		event OnDisconnectHandler OnDisconnect;

		/// <summary>
		/// Returns the other end of the line.
		/// </summary>
		/// <value>The endpoint.</value>
		[JsonIgnoreAttribute] // IPEndPoint is not very serializable..
		IPEndPoint Endpoint { get; } 

	}
}

