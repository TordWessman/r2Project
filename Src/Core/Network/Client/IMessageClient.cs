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
using R2Core.Device;
using System.Linq;

namespace R2Core.Network
{

	/// <summary>
	/// Implementations are capable of transmitting data to a remote host.
	/// </summary>
	public interface IMessageClient : INetworkConnection {

		/// <summary>
		/// Send a ´INetworkMessage´ to the remote host asynchronously. ´responseDelegate´ will be called when the 
		/// requests fails or completes. Returns the request ´Task´.
		/// </summary>
		/// <returns>The async.</returns>
		/// <param name="request">Request message.</param>
		/// <param name="responseDelegate">Response delegate.</param>
		System.Threading.Tasks.Task SendAsync(INetworkMessage request, Action<INetworkMessage> responseDelegate);

		/// <summary>
		/// Adds a ´IMessageClientObserver´. Depending on the implementation of the ´IMessageClient´, some or all methods might never be called.
		/// </summary>
		/// <param name="observer">Observer.</param>
		void AddClientObserver(IMessageClientObserver observer);

		/// <summary>
		/// Default headers included in each request. These will override headers set in the message (´INetworkMessage.Headers)´.
		/// </summary>
		System.Collections.Generic.IDictionary<string, object> Headers { get; set; }

	}

	public static class IMessageClientExtensions {

		/// <summary>
		/// Set the header value using ´key´ if ´value´ is not null. 
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public static void SetHeader(this IMessageClient client, string key, string value) {

			if (value != null) {

				if (client.Headers == null) { client.Headers = new System.Collections.Generic.Dictionary<string, object>(); }

				client.Headers[key] = value;

			}

		}

	}

}