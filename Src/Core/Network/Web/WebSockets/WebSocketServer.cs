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

using System;
using WebSocketSharp;
using WebSocketSharp.Net;
using Core;
using Core.Device;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Core.Network.Web
{
	internal class WebSocketHandler: WebSocketSharp.Server.WebSocketBehavior, IWebSocketSenderDelegate {
	
		private IWebEndpoint m_endpoint;
		private IEnumerable<IWebSocketSender> m_senders;
		private System.Text.Encoding m_encoding;

		public string UriPath { get { return m_endpoint?.UriPath; } }

		public WebSocketHandler(IEnumerable<IWebSocketSender> senders, IWebEndpoint endpoint, System.Text.Encoding encoding) {
			
			m_senders = senders;
			m_encoding = encoding;

			if (m_senders != null) {
			
				foreach (IWebSocketSender sender in m_senders) {

					sender.Delegate = this;

				}

			}


			m_endpoint = endpoint;
		}

		public void SetEndpoint(IWebEndpoint endpoint) {
		
			m_endpoint = endpoint;

		}

		public void AddSender(IWebSocketSender sender) {
		
			sender.Delegate = this;
			//m_senders.Add (sender);

		}

		protected override void OnMessage (MessageEventArgs e)
		{
			Core.Log.d ("OnMessage");
			Core.Log.t (e.Data);
			Console.WriteLine (e.RawData);

			if (m_endpoint != null) {

				byte[] request = m_encoding.GetBytes(e.Data);

				byte[] response = m_endpoint.Interpret (request);
				if (response != null && response.Length > 0) {

					if (this.State == WebSocketState.Open) {

						Send (response);

					}

				}

			}

		}

		protected override void OnClose (CloseEventArgs e)
		{
			Core.Log.d ("OnClose");

			//Sessions.Broadcast (String.Format ("{0} got logged off...", _name));
		}

		protected override void OnOpen ()
		{
			Core.Log.d ("OnOpen");
		}

		public void OnSend (byte[] data) {
		
			if (this.State == WebSocketState.Open) {
			
				Send (data);

			}

		}

	}

	public class WebSocketServer: DeviceBase, IWebSocketServer
	{
		private WebSocketSharp.Server.WebSocketServer m_server;
		//private IDictionary<string,WebSocketHandler> m_handlers;
		private IDictionary<string, IWebEndpoint> m_endpoints;
		private IDictionary<string, IList<IWebSocketSender>> m_senders;
		System.Text.Encoding m_encoding;

		public WebSocketServer (string id, int port, System.Text.Encoding encoding) : base (id) {

			m_encoding = encoding;
			m_server = new WebSocketSharp.Server.WebSocketServer (port);

			m_server.KeepClean = true;
			m_endpoints = new Dictionary<string, IWebEndpoint> ();
			m_senders = new Dictionary<string, IList<IWebSocketSender>> ();

			m_server.Log.Level = LogLevel.Trace;

		}

		public override void Start() {
		
			m_server.Start ();

		}

		public override void Stop ()
		{
			m_server.Stop ();
		}

		public int Port { get { return m_server.Port; } }
		public string Ip { get { return /*m_server.Address.ToString (); */ Dns.GetHostEntry (Dns.GetHostName ()).AddressList.Where (ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault ()?.ToString ();  } }

		public override bool Ready { get { return m_server.IsListening; } }

		private WebSocketHandler CreateWebSocketHandler(string uriPath) {

			IWebEndpoint endpoints = m_endpoints.Where(e => e.Key == uriPath).FirstOrDefault().Value;
			IEnumerable<IWebSocketSender> senders = m_senders.Where(s => s.Key == uriPath).FirstOrDefault().Value;
			return new WebSocketHandler(senders, endpoints, m_encoding); 
		}

		public void AddEndpoint(IWebEndpoint interpreter) {

			m_server.RemoveWebSocketService (interpreter.UriPath);

			if (!m_endpoints.ContainsKey (interpreter.UriPath)) {

				m_endpoints.Add (interpreter.UriPath, interpreter);

			} else {

				m_endpoints [interpreter.UriPath] = interpreter;

			}

			m_server.AddWebSocketService<WebSocketHandler> (interpreter.UriPath, delegate() { return CreateWebSocketHandler(interpreter.UriPath); });

		}

		public void AddSender(IWebSocketSender sender) {

			m_server.RemoveWebSocketService (sender.UriPath);

			if (!m_senders.ContainsKey (sender.UriPath)) {

				m_senders.Add(sender.UriPath, new List<IWebSocketSender>());

			}

			m_senders [sender.UriPath].Add (sender);

			m_server.AddWebSocketService<WebSocketHandler> (sender.UriPath, delegate() { return CreateWebSocketHandler(sender.UriPath); });

		}

	}

}