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
	public class HttpServer : ServerBase
	{


		private HttpListener m_listener;
		private ISerialization m_serialization;

		public HttpServer (string id, int port, ISerialization serialization) :  base (id, port)
		{
			m_serialization = serialization;

		}

		protected override void Service() {

			m_listener = new HttpListener ();
			m_listener.Prefixes.Add(String.Format("http://*:{0}/", Port));

			m_listener.Start ();

			while (ShouldRun) {

 				HttpListenerContext context = m_listener.GetContext();

				if (m_listener.IsListening) {

					HttpListenerRequest request = context.Request;
					HttpListenerResponse response = context.Response;

					IWebEndpoint endpoint = GetEndpoint (request.Url.AbsolutePath);

					if (endpoint != null) {
					
						Interpret (request, response, endpoint);
					
					} else {

						Log.w ("No IWebEndpoint accepts: " + request.Url.AbsolutePath);
						response.StatusCode = (int) WebStatusCode.NotFound;

						Write (response,  m_serialization.Serialize(new WebErrorMessage((int) WebStatusCode.NotFound, $"Path not found: {request.Url.AbsolutePath}") ));

					}

				}

			}

			m_listener.Stop();

		}

		public override bool Ready { get { return m_listener?.IsListening == true; }}

		protected override void Cleanup () {
			
			try {
			
				m_listener.Stop ();

			} catch (System.Net.Sockets.SocketException) {}

		}

		private void Interpret(HttpListenerRequest request, HttpListenerResponse response, IWebEndpoint endpoint) {
		
			using (StreamReader reader = new StreamReader(request.InputStream, m_serialization.Encoding))
			{

				byte[] responseBody = new byte[0];
				byte[] requestBody = default(byte[]);
				HttpMessage requestObject = new HttpMessage () {Destination = request.Url.AbsolutePath, Headers = new Dictionary<string, object> () };

				requestObject.Method = request.HttpMethod;

				// TODO: If one needs querystring parameters 
				//NameValueCollection queryStringParameters = HttpUtility.ParseQueryString (request.Url.Query);
				//foreach (string key in  queryStringParameters.AllKeys) { requestObject.Headers[key] = queryStringParameters[key]; }

				try {
					
					// Read body
					using (var memstream = new MemoryStream())
					{

						reader.BaseStream.CopyTo(memstream);
						requestBody = memstream.ToArray();

					}

					if (request.ContentType?.Contains("json") == true) {
			
						// Treaded as complex object.
						requestObject.Payload = m_serialization.Deserialize(requestBody);
					
					} else if (request.ContentType?.Contains("text") == true) {
					
						// Treated as string.
						requestObject.Payload = m_serialization.Encoding.GetString(requestBody);

					} else {
					
						// Treated as byte array.
						requestObject.Payload = requestBody;

					}

					// Parse request and create response body.
					INetworkMessage responseObject = endpoint.Interpret (requestObject, request.RemoteEndPoint);
					string contentType = responseObject.Headers?.ContainsKey("Content-Type") == true ? responseObject.Headers["Content-Type"] as string : null;

					if (responseObject.Payload is byte[]) {

						//Data was returned in raw format.
						responseBody = responseObject.Payload;
						response.ContentType = contentType ?? "application/octet-stream";

					} else if (responseObject.Payload is string) {

						// Data was a string.
						responseBody = m_serialization.Encoding.GetBytes (responseObject.Payload);
						response.ContentType = contentType ?? "text/plain";

					} else {

						// Object is considered to be complex and will be transcribed into a json object
						responseBody = m_serialization.Serialize(responseObject.Payload);
						response.ContentType = contentType ?? "application/json";

					}

					// Add header fields from metadata
					responseObject.Headers?.ToList().ForEach( kvp => response.Headers[kvp.Key] = kvp.Value.ToString());
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

	}



}

