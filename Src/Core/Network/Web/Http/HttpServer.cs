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
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Core.Device;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.Web;
using Core.Data;
using System.Text.RegularExpressions;

namespace Core.Network.Web
{

	/// <summary>
	/// Primitive HTTP server.
	/// </summary>
	public class HttpServer : DeviceBase, IWebServer
	{
		public static readonly string HEADERS_KEY = "_headers";
		public static readonly string HTTP_METHOD_KEY = "_httpMethod";

		private HttpListener m_listener;
		private bool m_shouldrun;
		private int m_port;
		private IList<IWebEndpoint> m_endpoints;
		private Task m_task;
		private ISerialization m_serialization;

		public HttpServer (string id, int port, ISerialization serialization) :  base (id)
		{
			m_port = port;
			m_endpoints = new List<IWebEndpoint> ();
			m_serialization = serialization;

		}

		private void Service() {

			m_listener.Start ();

			while (m_shouldrun) {

 				HttpListenerContext context = m_listener.GetContext();

				if (m_listener.IsListening) {

					HttpListenerRequest request = context.Request;
					HttpListenerResponse response = context.Response;

					bool match = false;

					foreach (IWebEndpoint endpoint in m_endpoints) {
					
						if (Regex.IsMatch (request.Url.AbsolutePath, endpoint.UriPath)) {

							Interpret (request, response, endpoint);
							match = true;
							break;

						}

					}

					if (!match) {

						Log.w ("No IWebEndpoint accepts: " + request.Url.AbsolutePath);
						response.StatusCode = (int) WebStatusCode.NotFound;

						Write (response,  m_serialization.Serialize(new WebErrorMessage((int) WebStatusCode.NotFound, $"Path not found: {request.Url.AbsolutePath}") ));
					
					} 

				}

			}

			m_listener.Stop();

		}

		public string Ip { 

			get {

				return Dns.GetHostEntry (Dns.GetHostName ()).AddressList.Where (ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault ()?.ToString ();
			
			} 
		
		}
		public int Port { get { return m_port; } }

		public override bool Ready { get { return m_shouldrun; }}

		public override void Start() {

			m_shouldrun = true;
			m_listener = new HttpListener ();
			m_listener.Prefixes.Add(String.Format("http://*:{0}/", m_port));

			m_task = Task.Factory.StartNew(Service);

			Log.d ("HTTP server running with state: " +  m_task.Status);


		}

		public override void Stop() {
			
			m_shouldrun = false;
			try {
			
				m_listener.Stop ();

			} catch (System.Net.Sockets.SocketException) {}

		}



		public void AddEndpoint(IWebEndpoint interpreter) {

			m_endpoints.Add (interpreter);

		}

		private void Interpret(HttpListenerRequest request, HttpListenerResponse response, IWebEndpoint endpoint) {
		
			using (StreamReader reader = new StreamReader(request.InputStream, m_serialization.Encoding))
			{

				byte[] responseBody = new byte[0];
				byte[] requestBody = default(byte[]);
				dynamic requestObject;

				try {
					
					// Read body
					using (var memstream = new MemoryStream())
					{

						reader.BaseStream.CopyTo(memstream);
						requestBody = memstream.ToArray();

					}

					if (request.ContentType?.Contains("json") == true) {
			
						// Treaded as complex object.
						requestObject = m_serialization.Deserialize(requestBody);
					
					} else if (request.ContentType?.Contains("text") == true) {
					
						// Treated as string.
						requestObject = m_serialization.Encoding.GetString(requestBody);

					} else {
					
						// Treated as byte array.
						requestObject = requestBody;

					}

					// Parse request and create response body.
					dynamic responseObject = endpoint.Interpret (requestObject, request.Url.AbsolutePath, CreateMetadata(request));

					if (responseObject is byte[]) {

						//Data was returned in raw format.
						responseBody = responseObject;

					} else if (responseObject is string) {

						// Data was a string.
						responseBody = m_serialization.Encoding.GetBytes (responseObject);

					} else {

						// Object is considered to be complex and will be transcribed into a json object
						responseBody = m_serialization.Serialize(responseObject);

					}

					// Add header fields from metadata
					endpoint.Metadata.ToList().ForEach( kvp => response.Headers[kvp.Key] = kvp.Value.ToString());

					response.StatusCode = (int) WebStatusCode.Ok;

				} catch (Exception ex) {

					Log.x (ex);

					response.StatusCode = (int) WebStatusCode.ServerError;

					#if DEBUG
					responseBody = m_serialization.Serialize(new HttpMessage() { Payload = new HttpError(ex.Message), Code = response.StatusCode } );
					#endif

				}

				Write (response, responseBody);

			}

		}

		private void Write(HttpListenerResponse response, byte[] data) {
		
			response.ContentLength64 = data.Length;
			System.IO.Stream output = response.OutputStream;
			output.Write(data, 0, data.Length);
			output.Close();

		}

		/// <summary>
		/// Creates meta data (headers, HTTP method, uri etc)  retreived from the request.
		/// </summary>
		/// <returns>The meta data.</returns>
		/// <param name="request">Request.</param>
		private IDictionary<string, object> CreateMetadata(HttpListenerRequest request) {

			IDictionary<string, object> metadata = new Dictionary<string, object> ();

			// Add query string parameters to meta data.
			if (request.Url != null) {

				NameValueCollection queryStringParameters = HttpUtility.ParseQueryString (request.Url.Query);

				foreach (string key in  queryStringParameters.AllKeys) { metadata[key] = queryStringParameters[key]; }

			}

			metadata[HEADERS_KEY] = request.Headers;
			metadata[HTTP_METHOD_KEY] = request.HttpMethod;

			return metadata;

		}

	}



}

