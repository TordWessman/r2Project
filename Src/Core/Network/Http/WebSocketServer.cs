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

namespace Core.Network.Http
{
	internal class WebSocketHandler: WebSocketSharp.Server.WebSocketBehavior {
	
		private IHttpEndpoint m_endpoint;

		public WebSocketHandler(IHttpEndpoint endpoint) {
		
			m_endpoint = endpoint;

		}

		public WebSocketHandler() {
		
			throw new NotImplementedException ("Do not create me using this constructor!");
		}

		protected override void OnMessage (MessageEventArgs e)
		{
			//Core.Log.d ("OnMessage");
			//Core.Log.t (e.Data);


			Send (m_endpoint.Interpret (e.RawData));
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
	}

	public class WebSocketServer: DeviceBase, IHttpServer
	{
		private WebSocketSharp.Server.WebSocketServer m_endpoint;

		public WebSocketServer (string id, int port) : base (id) {
			
			m_endpoint = new WebSocketSharp.Server.WebSocketServer (port);

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

		public void AddEndpoint(IHttpEndpoint interpreter) {

			m_endpoint.AddWebSocketService<WebSocketHandler> (interpreter.UriPath, () => new WebSocketHandler(interpreter));
			
		}

	}

}