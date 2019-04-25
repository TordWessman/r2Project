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

}

namespace R2Core.Network.Extensions {

	public static class INetworkMessage {



	}

}

