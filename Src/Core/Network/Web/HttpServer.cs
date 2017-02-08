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

namespace Core.Network.Web
{

	/// <summary>
	/// Primitive HTTP server.
	/// </summary>
	public class HttpServer : DeviceBase, IWebServer
	{
		public static readonly string HEADERS_KEY = "_headers";
		public static readonly string HTTP_METHOD_KEY = "_httpMethod";
		public static readonly string URI_KEY = "_uri";

		private HttpListener m_listener;
		private bool m_shouldrun;
		private int m_port;
		private IDictionary<string,IWebEndpoint> m_endpoints;
		private Task m_task;
		private System.Text.Encoding m_encoding;

		public readonly static System.Text.Encoding DefaultEncoding = System.Text.Encoding.UTF8; 

		public HttpServer (string id, int port, System.Text.Encoding encoding = null) :  base (id)
		{
			m_port = port;
			m_endpoints = new Dictionary<string,IWebEndpoint> ();
			m_encoding = encoding ?? DefaultEncoding;

		}

		private void RunLoop() {

			m_listener.Start ();

			while (m_shouldrun) {

 				HttpListenerContext context = m_listener.GetContext();

				if (m_listener.IsListening) {

					HttpListenerRequest request = context.Request;
					HttpListenerResponse response = context.Response;

					if (!m_endpoints.ContainsKey (request.Url.AbsolutePath)) {

						Log.w ("No IWebEndpoint accepts: " + request.Url.AbsolutePath);
						response.StatusCode = 404;

					} else {
					
						Interpret (request, response);

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

			m_task = Task.Factory.StartNew(RunLoop);

			Log.d ("HTTP server running with state: " +  m_task.Status);


		}

		public override void Stop() {
			m_shouldrun = false;

			m_listener.Stop ();
		}

		public void AddEndpoint(IWebEndpoint interpreter) {

			m_endpoints.Add (interpreter.UriPath, interpreter);

		}

		private void Interpret(HttpListenerRequest request, HttpListenerResponse response) {
		
			using (StreamReader reader = new StreamReader(request.InputStream, m_encoding))
			{

				byte[] responseBody;
				byte[] requestBody = default(byte[]);

				try {

					IWebEndpoint interpreter = m_endpoints[request.Url.AbsolutePath];

					// Read body
					using (var memstream = new MemoryStream())
					{

						reader.BaseStream.CopyTo(memstream);
						requestBody = memstream.ToArray();

					}

					// Parse request and create response body.
					responseBody = interpreter.Interpret (requestBody, CreateMetadata(request));

					// Add header fields from metadata
					interpreter.Metadata.ToList().ForEach( kvp => response.Headers[kvp.Key] = kvp.Value.ToString());

				} catch (Exception ex) {

					Log.x (ex);

					#if DEBUG
					responseBody = ("{ error:\"" + ex.Message + "\"}").ToByteArray (m_encoding);
					#else
					responseBuffer = "ERROR".ToByteArray(m_encoding);
					#endif

					response.StatusCode = 500;

				}

				response.ContentLength64 = responseBody.Length;
				System.IO.Stream output = response.OutputStream;
				output.Write(responseBody,0,responseBody.Length);
				output.Close();

			}
		}

		/// <summary>
		/// Creates meta data (headers, HTTP method, uri etc)  retreived from the request
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
			metadata[URI_KEY] = request.Url;

			return metadata;

		}

	}



}

