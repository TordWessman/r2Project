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

﻿using System;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Dynamic;
using Newtonsoft.Json.Converters;
using System.Web;
using System.Linq;
using Core.Data;

namespace Core.Network.Web
{

	/// <summary>
	/// A JsonEndpoint is intended to represent a HttpServer endpoint mapped to a specified URI path.
	/// </summary>
	public class WebJsonEndpoint: IWebEndpoint
	{

		private IWebObjectReceiver m_receiver;
		private string m_responsePath;
		private IDictionary<string, object> m_extraHeaders;
		private ISerialization m_serialization;

		/// <summary>
		/// Parsing the input as a dictionary.
		/// responseURIPath is the URI path of which this interpeter listens to (i.e. '/devices')<br/>
		/// receiver is the interface responsible of handle incoming input json and returning data in a suitable format.
		/// </summary>
		/// <param name="responsePath">Response path.</param>
		/// <param name="responsePath">receiver</param>

		public WebJsonEndpoint (string responseURIPath, IWebObjectReceiver receiver, ISerialization serialization)
		{
			
			m_receiver = receiver;

			m_responsePath = responseURIPath;
			m_extraHeaders = new Dictionary<string, object> ();
			m_serialization = serialization;

			m_extraHeaders.Add ("Content-Type", @"application/json");

			// Allow cross domain access from browsers. 
			m_extraHeaders.Add ("Access-Control-Allow-Origin", "*");
			m_extraHeaders.Add ("Access-Control-Allow-Methods", "GET, PUT, POST, DELETE, OPTIONS");
			m_extraHeaders.Add ("Access-Control-Allow-Headers", "Content-Type, Content-Range, Content-Disposition, Content-Description");

		}

		#region IHttpServerInterpreter implementation

		public byte[] Interpret (byte[] input, string url, IDictionary<string, object> metadata = null) {
			
			R2Dynamic inputObject = m_serialization.Deserialize(input);

			// Let reciver parse response.
			IWebIntermediate outputObject = m_receiver.OnReceive (inputObject, "", metadata);

			// Let Metadata be the extra headers.
			outputObject.Metadata?.ToList ().ForEach (kvp => m_extraHeaders[kvp.Key] = kvp.Value.ToString ());

			if (outputObject.Data is byte[]) {

				//Data was returned in raw format.

				return outputObject.Data;

			} else if (outputObject.Data is string) {
			
				// Data was a string.

				return m_serialization.Encoding.GetBytes (outputObject.Data);

			} else {

				return m_serialization.Serialize(outputObject.Data);

			}

		}

		public string UriPath { get { return m_responsePath; } }

		public IDictionary<string, object> Metadata { get { return m_extraHeaders; } }

		#endregion

	}

}

