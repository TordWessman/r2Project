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

namespace Core.Network.Http
{
	public class HttpServer : DeviceBase, IHttpServer
	{
		private HttpListener m_listener;
		private bool m_shouldrun;
		private int m_port;
		private IList<IHttpServerInterpreter> m_interpreters;
		private Task m_task;
		private System.Text.Encoding m_encoding;

		public HttpServer (string id, int port, System.Text.Encoding encoding = null) :  base (id)
		{
			m_port = port;
			m_interpreters = new List<IHttpServerInterpreter> ();
			m_encoding = encoding ?? System.Text.Encoding.UTF8;

		}

		private void RunLoop() {

			m_listener.Start ();

			while (m_shouldrun) {

 				HttpListenerContext context = m_listener.GetContext();

				if (m_listener.IsListening) {
					HttpListenerRequest request = context.Request;

					// Obtain a response object.
					HttpListenerResponse response = context.Response;

					using (StreamReader reader = new StreamReader(request.InputStream, m_encoding))
					{
						string inputString = reader.ReadToEnd();
						byte[] responseBuffer = {};
						bool didFindResponder = false;

						try {

							foreach (IHttpServerInterpreter interpreter in m_interpreters) {

								if (interpreter.Accepts(request.Url.AbsolutePath)) {

									// The interpreter can handle the specified URI and will continue to process it.

									didFindResponder = true;
									responseBuffer = interpreter.Interpret (inputString, request.Url.AbsolutePath, request.HttpMethod);

									foreach (string headerKey in interpreter.ExtraHeaders.Keys) {
										response.Headers.Add (headerKey, interpreter.ExtraHeaders [headerKey]);
									}

									response.ContentType = interpreter.HttpContentType;

									break;
								
								} 

							}


						} catch (Exception ex) {
							Log.x (ex);

							responseBuffer = ex.Message.ToByteArray (m_encoding);

							response.StatusCode = 500;
						}

						if (!didFindResponder && response.StatusCode != 500) {

							Log.w("No IHttpServerInterpreter accepts: " + request.Url.AbsolutePath);
							response.StatusCode = 404;

						}

						response.ContentLength64 = responseBuffer.Length;
						System.IO.Stream output = response.OutputStream;
						output.Write(responseBuffer,0,responseBuffer.Length);
						output.Close();

					}

				}

			}

			m_listener.Stop();

		}

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

		public void AddInterpreter(IHttpServerInterpreter interpreter) {
		
			m_interpreters.Add (interpreter);

		}

	}



}

