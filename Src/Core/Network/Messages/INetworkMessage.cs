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
using System.Linq;

namespace R2Core.Network
{
	/// <summary>
	/// Represents the core features requirements of a network message sent/received by IWebServer or IMessageClient implementations
	/// </summary>
	public interface INetworkMessage {

		/// <summary>
		/// Represents a status. See <see cref="NetworkStatusCode"/> 
		/// </summary>
		/// <value>The code.</value>
		int Code { get; set; }

		/// <summary>
		/// A string describing the final destination of the message(i.e. /products/cats/42)
		/// </summary>
		/// <value>The destination.</value>
		string Destination { get; set; }

		/// <summary>
		/// A set of optional headers. Headers are more extensively use by the Http server
		/// </summary>
		/// <value>The headers.</value>
		System.Collections.Generic.IDictionary<string, object> Headers { get; set; }

		/// <summary>
		/// The actual data being transmitted. The serialization of the object is performed by the server, so the type of the object should be perserved.
		/// </summary>
		/// <value>The payload.</value>
		dynamic Payload { get; set; }

	}

	public static class INetworkMessageExtensions {

		/// <summary>
		/// Add the nullable ´overridingHeaders´ to ´Headers´. Any KVP in ´overridingHeaders´ will override local header values. Returns
		/// the new header collection.  
		/// </summary>
		/// <param name="self">Self.</param>
		/// <param name="overridingHeaders">Overriding headers.</param>
		public static System.Collections.Generic.IDictionary<string, object> OverrideHeaders(this INetworkMessage self, System.Collections.Generic.IDictionary<string, object> overridingHeaders) {

			if (self.Headers != null) { overridingHeaders?.ToList().ForEach(kvp => self.Headers[kvp.Key] = kvp.Value); }

			self.Headers = self.Headers ?? overridingHeaders;

			return self.Headers;

		}

		/// <summary>
		/// Set the header value using ´key´ if ´value´ is not null. 
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public static void SetHeader(this INetworkMessage message, string key, string value) {

			if (value != null) {

				if (message.Headers == null) { message.Headers = new System.Collections.Generic.Dictionary<string, object>(); }

				message.Headers[key] = value;

			}

		}

		/// <summary>
		/// Sets the host name header value for a routed network message.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="hostName">Host name.</param>
		public static void SetHostName(this INetworkMessage message, string hostName) {

			message.SetHeader(Settings.Consts.ConnectionRouterHeaderHostNameKey(), hostName);

		}

	}

}