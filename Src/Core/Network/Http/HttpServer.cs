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
using System.Collections.Generic;
using System.Linq;

namespace R2Core.Network
{

	/// <summary>
	/// Primitive HTTP server.
	/// </summary>
	public class HttpServer : ServerBase {
		
		private HttpListener m_listener;
		private ISerialization m_serialization;
		private IList<Task> m_writeTasks = new List<Task>();

		public HttpServer(string id, int port, ISerialization serialization) :  base(id) {
			
			m_serialization = serialization;
			SetPort(port);

		}

		protected override void Service() {

            try {

                m_listener = new HttpListener();
                m_listener.Prefixes.Add(String.Format("http://*:{0}/", Port));
                m_listener.Start();

            } catch (Exception ex) {

                Log.x(ex, Identifier);

            }

			while(ShouldRun) {

 				HttpListenerContext context = m_listener.GetContext();

				if (m_listener.IsListening) {

                    HandleRequest(context);

				}

			}

			m_listener.Stop();

		}

		public override bool Ready { get { return m_listener?.IsListening == true; }}

		protected override void Cleanup() {
			
			try {
			
				m_listener?.Stop();

			} catch (System.Net.Sockets.SocketException) {}

		}

		private dynamic GetPayload(HttpListenerRequest request) {

			if (request.HttpMethod.ToLower () == "get") {

				R2Dynamic payload = new R2Dynamic ();

				foreach (var key in request.QueryString.AllKeys) {
				
					payload.Add (key, request.QueryString[key]);

				}

				return payload;

			}

			using(StreamReader reader = new StreamReader(request.InputStream, m_serialization.Encoding)) {
				
				byte[] requestBody = default(byte[]);

				using (var memstream = new MemoryStream()) {

					reader.BaseStream.CopyTo(memstream);
					requestBody = memstream.ToArray();

				}

				if (request.ContentType?.Contains("json") == true) {

					// Treaded as complex object.
					return m_serialization.Deserialize(requestBody);

				} else if (request.ContentType?.Contains("text") == true) {

					// Treated as string.
					return m_serialization.Encoding.GetString(requestBody);

				} 

				// Treated as byte array.
				return (requestBody?.Length ?? 0) > 0 ? requestBody : null;

			}

		}

		public override INetworkMessage Interpret(INetworkMessage request, System.Net.IPEndPoint source) {
		
			IWebEndpoint endpoint = GetEndpoint (request.Destination);

			if (endpoint == null) {
			
				Log.w($"No HTTP IWebEndpoint accepts {request.Destination}", Identifier);

                HttpMessage response = new HttpMessage(new NetworkErrorMessage(NetworkStatusCode.NotFound, $"Path not found: {request.Destination}", request)) {
                    ContentType = HttpMessage.DefaultContentType
                };
                return response;

			}

			return endpoint.Interpret(request, source);

		}

		private void HandleRequest(HttpListenerContext context) {

            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            Log.i($"Got request from {request.RemoteEndPoint.Address}. Path: {request.Url.AbsolutePath}", Identifier);

            Dictionary<string, object> requestHeaders = new Dictionary<string, object>();
			byte[] responseBody = new byte[0];

			try {

				foreach (var header in request.Headers.AllKeys.SelectMany(request.Headers.GetValues, (k, v) => new {key = k, value = v})) {

					requestHeaders[header.key] = header.value;

				}

				// Perserve the HTTP Method by 
				requestHeaders[Settings.Consts.HttpServerHeaderMethodKey()] = request.HttpMethod;

				HttpMessage requestObject = new HttpMessage {Destination = request.Url.AbsolutePath, Headers = requestHeaders};
				requestObject.Method = request.HttpMethod;

				requestObject.Payload = GetPayload(request);

				// Parse request and create response body.
				INetworkMessage responseObject = Interpret(requestObject, request.RemoteEndPoint);

				string contentType = responseObject.Headers?.ContainsKey("Content-Type") == true ? responseObject.Headers["Content-Type"] as string : null;

				if (contentType == null && responseObject is HttpMessage) {
					
					contentType = ((HttpMessage) responseObject).ContentType;
				
				}

				if (responseObject.Payload is byte[]) {

					//Data was returned in raw format.
					responseBody = responseObject.Payload;
					response.ContentType = contentType ?? "application/octet-stream";

				} else if (responseObject.Payload is string) {

					// Data was a string.
					responseBody = m_serialization.Encoding.GetBytes(responseObject.Payload);
					response.ContentType = contentType ?? "text/plain";

				} else {

					// Object is considered to be complex and will be transcribed into a json object
					responseBody = m_serialization.Serialize(responseObject.Payload);
					response.ContentType = contentType ?? "application/json";

				}

				// Add header fields from metadata
				responseObject.Headers?.ToList().ForEach( kvp => response.Headers[kvp.Key] = kvp.Value.ToString());

				response.StatusCode = responseObject.Code == 0 ? NetworkStatusCode.Ok.Raw() : responseObject.Code;

			} catch (Exception ex) {

				Log.x(ex, Identifier);

				response.StatusCode = NetworkStatusCode.ServerError.Raw();

				responseBody = m_serialization.Serialize(new HttpMessage(new NetworkErrorMessage(ex)));

			}

			Write(response, responseBody);
            Log.i($"Did reply to {request.RemoteEndPoint.Address}. Length: {responseBody.Length}. StatusCode: {response.StatusCode}", Identifier);
        }

		private void ClearInactiveWriteTasks() {

			IList<Task> inactiveTasks = new List<Task>();

			foreach (Task task in m_writeTasks) {

				if (task.IsCompleted || task.IsFaulted) {

					inactiveTasks.Add(task);

				}

			}

			foreach (Task task in inactiveTasks) { m_writeTasks.Remove(task); }

		}

		private void Write(HttpListenerResponse response, byte[] data) {

			response.ContentLength64 = data.Length;
			Stream output = response.OutputStream;
			WeakReference<HttpListenerResponse> reference = new WeakReference<HttpListenerResponse>(response);

			ClearInactiveWriteTasks();

            Log.i($"Will write. Current task count: {m_writeTasks.Count}. ", Identifier);
            Task writeTask = new Task(() => {

                try {

                    output.Write(data, 0, data.Length);

                } catch (Exception ex) { Log.x(ex, Identifier); } 
                finally {

                    output.Close();
                    response.Close();
                    Log.i($"Did close stream.", Identifier);
                }
            });

            m_writeTasks.Add(writeTask);
            writeTask.Start();

		}

	}

}

