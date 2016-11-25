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
using System.Web.Script.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Specialized;

namespace Core.Network.Http
{
	/// <summary>
	/// A JsonEndpoint is intended to represent a HttpServer endpoint mapped to a specified URI path.
	/// </summary>
	public class HttpJsonEndpoint : IHttpEndpoint
	{

		private IHttpObjectReceiver m_receiver;
		private string m_responsePath;
		private string m_contentType;
		private NameValueCollection m_extraHeaders;

		/// <summary>
		/// Initializes a new instance of the <see cref="Core.Network.Http.HttpJsonEndpoint`"/>.<br/>
		/// responseURIPath is the URI path of which this interpeter listens to (i.e. '/devices')<br/>
		/// receiver is the interface responsible of handle incoming input json.
		/// </summary>
		/// <param name="responsePath">Response path.</param>
		/// <param name="responsePath">receiver</param>

		public HttpJsonEndpoint (string responseURIPath, IHttpObjectReceiver receiver)
		{
			m_receiver = receiver;

			m_responsePath = responseURIPath;
			m_contentType = @"application/json";

			m_extraHeaders = new NameValueCollection ();

			// Allow cross domain access from browsers. 
			m_extraHeaders.Add ("Access-Control-Allow-Origin", "*");
			m_extraHeaders.Add ("Access-Control-Allow-Methods", "GET, PUT, POST, DELETE, OPTIONS");
			m_extraHeaders.Add ("Access-Control-Allow-Headers", "Content-Type, Content-Range, Content-Disposition, Content-Description");

		}

		#region IHttpServerInterpreter implementation

		public byte[] Interpret (string inputJson, string uri, string httpMethod = null, NameValueCollection headers = null)
		{
			//Log.t ("got input json:" + inputJson + " and method: " + httpMethod);

			dynamic inputObject = JObject.Parse (inputJson);
			dynamic outputObject = m_receiver.onReceive (inputObject, httpMethod?.ToLower(), headers);
			String outputString = Convert.ToString (JsonConvert.SerializeObject (outputObject));
			return outputString?.ToByteArray() ?? new byte[0];

		}

		public bool Accepts(string uri) {

			return m_responsePath == uri;

		}

		public string HttpContentType {

			get {

				return m_contentType;

			}
		}

		public NameValueCollection ExtraHeaders {
		
			get {

				return m_extraHeaders;
			}

		}

		#endregion

	}

}

