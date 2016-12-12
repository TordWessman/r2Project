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

namespace Core.Network.Web
{
	internal class WebSocketHandler: WebSocketSharp.Server.WebSocketBehavior, IWebSocketSenderDelegate {
	
		private IWebEndpoint m_endpoint;
		private IList<IWebSocketSender> m_senders;

		public string UriPath { get { return m_endpoint?.UriPath; } }

		public WebSocketHandler() {

			m_senders = new List<IWebSocketSender> ();

		}

		public void SetEndpoint(IWebEndpoint endpoint) {
		
			m_endpoint = endpoint;

		}

		public void AddSender(IWebSocketSender sender) {
		
			sender.Delegate = this;
			m_senders.Add (sender);

		}

		protected override void OnMessage (MessageEventArgs e)
		{
			//Core.Log.d ("OnMessage");
			//Core.Log.t (e.Data);
			byte[] response = m_endpoint.Interpret (e.RawData);
			if (response != null && response.Length > 0) {
			
				Send (m_endpoint.Interpret (e.RawData));

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
		
			Send (data);

		}

	}

	public class WebSocketServer: DeviceBase, IWebSocketServer
	{
		private WebSocketSharp.Server.WebSocketServer m_endpoint;
		private IDictionary<string,WebSocketHandler> m_handlers;

		public WebSocketServer (string id, int port) : base (id) {
			
			m_endpoint = new WebSocketSharp.Server.WebSocketServer (port);
			m_handlers = new Dictionary<string, WebSocketHandler> ();

			m_endpoint.Log.Level = LogLevel.Trace;

		}

		public override void Start() {
		
			m_endpoint.Start ();

		}

		public override void Stop ()
		{
			m_endpoint.Stop ();
		}

		public override bool Ready { get { return m_endpoint.IsListening; } }

		public void AddEndpoint(IWebEndpoint interpreter) {

			if (!m_handlers.ContainsKey (interpreter.UriPath)) {

				WebSocketHandler handler = new WebSocketHandler ();
				handler.SetEndpoint (interpreter);
				m_handlers.Add (interpreter.UriPath, handler);

				m_endpoint.AddWebSocketService<WebSocketHandler> (interpreter.UriPath);
			} else {

				m_handlers [interpreter.UriPath].SetEndpoint (interpreter);

			}

		}

		public void AddSender(IWebSocketSender sender) {

			if (!m_handlers.ContainsKey (sender.UriPath)) {

				WebSocketHandler handler = new WebSocketHandler ();
				m_handlers[sender.UriPath].AddSender (sender);
				m_endpoint.AddWebSocketService<WebSocketHandler> (sender.UriPath);

			} else {
			
				m_handlers [sender.UriPath].AddSender (sender);
			}

		}

	}

}