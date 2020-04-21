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
using R2Core;
using R2Core.Device;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using R2Core.Data;

namespace R2Core.Network
{
	internal class WebSocketHandler: WebSocketSharp.Server.WebSocketBehavior, IWebSocketSenderDelegate {
	
		private IWebEndpoint m_endpoint;
		private IEnumerable<IWebSocketSender> m_senders;
		private ITCPPackageFactory<TCPMessage> m_packageFactory;

		public string UriPath { get { return m_endpoint?.UriPath; } }

		public WebSocketHandler(IEnumerable<IWebSocketSender> senders, IWebEndpoint endpoint, ITCPPackageFactory<TCPMessage> packageFactory) {
			
			m_senders = senders;
			m_packageFactory = packageFactory;
			m_senders?.AsParallel().ForAll(s => s.Delegate = this);
			m_endpoint = endpoint;

		}

		public void SetEndpoint(IWebEndpoint endpoint) {
		
			m_endpoint = endpoint;

		}

		public void AddSender(IWebSocketSender sender) {
		
			sender.Delegate = this;
			//m_senders.Add(sender);

		}

		protected override void OnMessage(MessageEventArgs e) {
			
			if (m_endpoint != null) {

				try {

					/*
					INetworkMessage request = m_packageFactory.DeserializePackage(e.RawData);
				

					//TODO: Fix Web socket server. Should use packetization(Requests should be serialized with headers, uri etc). 

					//Interpret response. No metadata is provided(and thus null).
					INetworkMessage response = m_endpoint.Interpret(request, e.);

					if (response != null && response.Length > 0) {

						if (this.State == WebSocketState.Open) {

							Send(response);

						}

					}*/

				} catch (Exception ex) {

					R2Core.Log.x(ex);

				}

			}

		}

		protected override void OnClose(CloseEventArgs e) {
			R2Core.Log.d("Web socket: OnClose");

			//Sessions.Broadcast(String.Format("{0} got logged off...", _name));
		}

		protected override void OnOpen() {
			R2Core.Log.d("Web socket: OnOpen");
		}

		public void OnSend(byte[] data) {
		
			if (this.State == WebSocketState.Open) {
			
				Send(data);

			}

		}

	}

	public class WebSocketServer : DeviceBase, IWebSocketServer
	{
		private WebSocketSharp.Server.WebSocketServer m_server;
		//private IDictionary<string,WebSocketHandler> m_handlers;
		private IDictionary<string, IWebEndpoint> m_endpoints;
		private IDictionary<string, IList<IWebSocketSender>> m_senders;
		private ITCPPackageFactory<TCPMessage> m_packageFactory;

		public WebSocketServer(string id, int port, ITCPPackageFactory<TCPMessage> packageFactory) : base(id) {

			throw new NotImplementedException("Web socket server is broken");
			m_packageFactory = packageFactory;
			m_server = new WebSocketSharp.Server.WebSocketServer(port);

			m_server.KeepClean = true;
			m_endpoints = new Dictionary<string, IWebEndpoint>();
			m_senders = new Dictionary<string, IList<IWebSocketSender>>();

            m_server.Log.Level = WebSocketSharp.LogLevel.Trace;

		}

		public override void Start() {
		
			m_server.Start();

		}

		public override void Stop() {
			m_server.Stop();
		}

		public int Port { get { return m_server.Port; } }
		public IEnumerable<string> Addresses { get { throw new NotImplementedException("Web socket server is broken"); } }

		public override bool Ready { get { return m_server.IsListening; } }

		private WebSocketHandler CreateWebSocketHandler(string uriPath) {

			IWebEndpoint endpoints = m_endpoints.Where(e => e.Key == uriPath).FirstOrDefault().Value;
			IEnumerable<IWebSocketSender> senders = m_senders.Where(s => s.Key == uriPath).FirstOrDefault().Value;
            m_server.Log.Level = WebSocketSharp.LogLevel.Fatal;

			return new WebSocketHandler(senders, endpoints, m_packageFactory); 
		}

		public void AddEndpoint(IWebEndpoint interpreter) {

			m_server.RemoveWebSocketService(interpreter.UriPath);

			m_endpoints [interpreter.UriPath] = interpreter;

			m_server.AddWebSocketService<WebSocketHandler> (interpreter.UriPath, delegate() { return CreateWebSocketHandler(interpreter.UriPath); });

		}

		public void AddSender(IWebSocketSender sender) {

			m_server.RemoveWebSocketService(sender.UriPath);

			if (!m_senders.ContainsKey(sender.UriPath)) {

				m_senders.Add(sender.UriPath, new List<IWebSocketSender>());

			}

			m_senders [sender.UriPath].Add(sender);

			m_server.AddWebSocketService<WebSocketHandler> (sender.UriPath, delegate() { return CreateWebSocketHandler(sender.UriPath); });

		}

		public INetworkMessage Interpret(INetworkMessage request, System.Net.IPEndPoint source) {
			throw new NotImplementedException();
		}

		public string UriPath { get { throw new NotImplementedException(); } }

	}

}