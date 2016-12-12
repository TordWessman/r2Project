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
using Core.Device;
using System.Net;
using System.Text;

namespace Core.Network.Web
{
	public class JsonClient : DeviceBase, IJsonClient
	{
		private string m_serverUrl;
		private JavaScriptSerializer m_serializer;

		public JsonClient (string id, string serverUrl) : base (id)
		{
			m_serverUrl = serverUrl;

			if (m_serverUrl[m_serverUrl.Length - 1] == '/') {
				m_serverUrl = m_serverUrl.Substring (0, m_serverUrl.Length - 1);
			}

			m_serializer = new JavaScriptSerializer ();
		}

		public void RegisterMe() {
			var pkg = new JsonMessageFactory ("tmp").CreateRegister ("olga", "mamma", "kacka", "4242");
			//var pkg = new JsonDeviceMessage () { Device = "mamma", Function = "farfar", Token = "pappa" };
			//m_serverUrl = @"http://localhost:4242";
			//var res = Send (pkg);//, "logins");

			var res = Send (pkg, "logins");
			Log.t (res != null ? res.ToString () : "null");
		}

		/// <summary>
		/// Used by non parameterized callers (i e ruby scripts)
		/// </summary>
		/// <returns>The object.</returns>
		/// <param name="message">Message.</param>
		/// <param name="path">Path.</param>
		/// <param name="httpMethod">Http method.</param>
		public JsonBaseMessage SendObject (JsonBaseMessage message, string path = "", string httpMethod = "POST") {
			return Send (message, path, httpMethod);
		}

		public T Send<T> (T message, string path = "", string httpMethod = "POST")  where T: JsonBaseMessage, new() {

			string outputJson =  m_serializer.Serialize (message);
			string inputJson = null;

			string url = m_serverUrl;

			if (!String.IsNullOrEmpty (path)) {
				if (path [0] != '/') {
					path = @"/" + path;
				}

				url = m_serverUrl + path;

			}

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

			request.Method = httpMethod;

			System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
			Byte[] byteArray = encoding.GetBytes(outputJson);

			request.ContentLength = byteArray.Length;
			request.ContentType = @"application/json";
			request.ReadWriteTimeout = 30000;
			request.Timeout = 30000;

			using (Stream dataStream = request.GetRequestStream()) {
				dataStream.Write(byteArray, 0, byteArray.Length);

				dataStream.Close ();
	
			}

			HttpWebResponse response = null;

			try {
				response = (HttpWebResponse) request.GetResponse();
			} catch (System.Net.WebException ex) {
				Log.w ( "Connection failed: " + request.RequestUri.ToString() + " exception:" +  ex.Message);

				try {
					using (Stream responseStream = ex.Response.GetResponseStream ()) {
						StreamReader reader = new StreamReader (responseStream, Encoding.UTF8);
						inputJson = reader.ReadToEnd ();
						Log.t (inputJson );
					}

					response.Close ();

				} catch(Exception) {}


				return new T () { Status = 500 };
			}


			if (response.StatusCode == HttpStatusCode.OK) {

				using (Stream responseStream = response.GetResponseStream()) {
					StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
					inputJson = reader.ReadToEnd();

					response.Close ();
					responseStream.Close ();

					if (!response.ContentType.Contains(request.ContentType)) {
						Log.t ("DATA: " + inputJson);
						Log.w ("JsonRequest got bad content type: " + response.ContentType + ". Expected: " + request.ContentType);
						return new T () { Status = 0 };
					}

					T inputObject = m_serializer.Deserialize<T> (inputJson);
					inputObject.Status = (int)response.StatusCode;
					return inputObject;
				}
			}

			Log.w ("JsonRequest got bad http status code: " + response.StatusCode);

			response.Close ();

			return new T () { Status = (int)response.StatusCode };

		}

	}
}

