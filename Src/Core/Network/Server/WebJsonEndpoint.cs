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
using System.Collections.Generic;
using System.Linq;

namespace R2Core.Network
{

	/// <summary>
	/// A JsonEndpoint is intended to represent a HttpServer endpoint mapped to a specified URI path.
	/// </summary>
	public class WebJsonEndpoint : IWebEndpoint {

		private IWebObjectReceiver m_receiver;
		private IDictionary<string, object> m_extraHeaders;
		private readonly ISerialization m_serialization;

		/// <summary>
		/// Parsing the input as a dictionary.
		/// receiver is the interface responsible of handle incoming input json and returning data in a suitable format.
		/// </summary>
		public WebJsonEndpoint(IWebObjectReceiver receiver, ISerialization serialization) {
			
			m_receiver = receiver;

			m_extraHeaders = new Dictionary<string, object>();
			m_serialization = serialization;

			m_extraHeaders.Add("Content-Type", @"application/json");

			// Allow cross domain access from browsers. 
			m_extraHeaders.Add("Access-Control-Allow-Origin", "*");
			m_extraHeaders.Add("Access-Control-Allow-Methods", "GET, PUT, POST, DELETE, OPTIONS");
			m_extraHeaders.Add("Access-Control-Allow-Headers", "Content-Type, Content-Range, Content-Disposition, Content-Description");

		}

		#region IWebEndpoint implementation

		public INetworkMessage Interpret(INetworkMessage input, System.Net.IPEndPoint source) {

			// Let reciver interpret the response.
			INetworkMessage outputObject = m_receiver.OnReceive(input, source);

			// Let Metadata be the extra headers.
			if (outputObject.Headers == null) {
			
				outputObject.Headers = new Dictionary<string, object>();

			}

			outputObject.Headers.ToList().ForEach(kvp => m_extraHeaders[kvp.Key] = kvp.Value.ToString());

			return outputObject;

		}

		public string UriPath { get { return m_receiver.DefaultPath; } }

        public IDictionary<string, object> Metadata => m_extraHeaders;

		#endregion

	}

}

