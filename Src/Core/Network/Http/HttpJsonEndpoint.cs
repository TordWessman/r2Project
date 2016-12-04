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
		private ExpandoObjectConverter m_converter;

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
			m_converter = new ExpandoObjectConverter();  

			m_extraHeaders = new NameValueCollection ();

			// Allow cross domain access from browsers. 
			m_extraHeaders.Add ("Access-Control-Allow-Origin", "*");
			m_extraHeaders.Add ("Access-Control-Allow-Methods", "GET, PUT, POST, DELETE, OPTIONS");
			m_extraHeaders.Add ("Access-Control-Allow-Headers", "Content-Type, Content-Range, Content-Disposition, Content-Description");

		}

		#region IHttpServerInterpreter implementation

		public byte[] Interpret (string inputJson, Uri uri, string httpMethod = null, NameValueCollection headers = null) {
			
			dynamic inputObject = null;

			if (httpMethod.ToUpper () != "GET") {
			
				// Parse body

				inputObject = JsonConvert.DeserializeObject<ExpandoObject>(inputJson, m_converter);

			} 

			if (inputObject == null) {
			
				// HTTP-GET or no body
				inputObject = new ExpandoObject ();

			}

			// Add query string parameters. Will replace json-properties for now...

			NameValueCollection queryKeys = HttpUtility.ParseQueryString (uri.Query);
			foreach (string key in  queryKeys.AllKeys) {

				try {

					(inputObject as IDictionary<string, Object>).Add (key, queryKeys [key]);

				} catch (System.ArgumentException ex) {
				
					//More user friendly error message upon posible duplicates.
					Log.x(ex);

					throw new System.ArgumentException ("Unable to add header parameter: '" + key + "' from request. Does it exist a duplicate in the Json-body?");
				}


			}

			IHttpIntermediate outputObject = m_receiver.onReceive (inputObject, httpMethod?.ToUpper(), headers);

			m_extraHeaders.Add (outputObject.Headers);

			string outputString = Convert.ToString (JsonConvert.SerializeObject (outputObject.Data));
			return outputString?.ToByteArray() ?? new byte[0];

		}

		public bool Accepts(Uri uri) {

			return m_responsePath == uri?.AbsolutePath;

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

