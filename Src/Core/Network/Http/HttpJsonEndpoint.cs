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

namespace Core.Network.Http
{
	/// <summary>
	/// A JsonEndpoint is intended to represent a HttpServer endpoint mapped to a specified URI path.
	/// </summary>
	public class HttpJsonEndpoint : IHttpEndpoint
	{

		private Func<dynamic, string, dynamic> m_interpretationFunc;
		private string m_responsePath;
		private string m_contentType;
		private IDictionary<string,string> m_extraHeaders;
		private System.Text.Encoding m_encoding;

		/// <summary>
		/// Initializes a new instance of the <see cref="Core.Network.Http.JsonInterpreter`2"/> class.
		/// responseURIPath is the URI path of which this interpeter listens to (i.e. '/devices')
		/// </summary>
		/// <param name="responsePath">Response path.</param>

		public HttpJsonEndpoint (string responseURIPath, Func<dynamic,string, dynamic> interpretationFunc, System.Text.Encoding encoding = null)
		{
			m_interpretationFunc = interpretationFunc;
			m_encoding = encoding ?? System.Text.Encoding.UTF8;

			m_responsePath = responseURIPath;
			m_contentType = @"application/json";

			m_extraHeaders = new Dictionary<string,string> ();

			// Allow cross domain access from browsers. 
			m_extraHeaders.Add ("Access-Control-Allow-Origin", "*");
			m_extraHeaders.Add ("Access-Control-Allow-Methods", "GET, PUT, POST, DELETE, OPTIONS");
			m_extraHeaders.Add ("Access-Control-Allow-Headers", "Content-Type, Content-Range, Content-Disposition, Content-Description");

		}

		#region IHttpServerInterpreter implementation

		public byte[] Interpret (string inputJson, string uri = null, string httpMethod = null)
		{
			//Log.t ("got input json:" + inputJson + " and method: " + httpMethod);

			dynamic inputObject = String.IsNullOrEmpty(inputJson) ? null : JObject.Parse (inputJson);

			dynamic outputObject = m_interpretationFunc (inputObject, httpMethod.ToLower());
			String outputString = Convert.ToString (JsonConvert.SerializeObject (outputObject));
			return outputObject != null ? outputString.ToByteArray() : new byte[0];

		}

		public bool Accepts(string uri) {

			return m_responsePath == uri;

		}

		public string HttpContentType {

			get {

				return m_contentType;

			}
		}

		public IDictionary<string,string> ExtraHeaders {
		
			get {

				return m_extraHeaders;
			}

		}

		#endregion

	}

}

