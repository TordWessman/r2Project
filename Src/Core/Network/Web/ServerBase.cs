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
//
using System;
using Core.Network.Web;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Device;
using System.Net;
using System.Linq;

namespace Core.Network
{
	public abstract class ServerBase : DeviceBase,  IWebServer
	{

		int m_port;
		private bool m_shouldRun;
		private Task m_service;

		private IList<IWebEndpoint> m_endpoints;

		protected bool ShouldRun { get { return m_shouldRun; } }

		public ServerBase (string id, int port) : base(id)
		{
			
			m_port = port;
			m_endpoints = new List<IWebEndpoint> ();

		}

		protected abstract void Service();

		public int Port { get { return m_port; } }

		public string Ip { 

			get {

				return Dns.GetHostEntry (Dns.GetHostName ()).AddressList.Where (ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault ()?.ToString ();

			}

		}

		protected IWebEndpoint GetEndpoint(string path) {
		
			return m_endpoints.Where (endpoint => System.Text.RegularExpressions.Regex.IsMatch (path, endpoint.UriPath)).FirstOrDefault ();
			/*
			foreach (IWebEndpoint endpoint in m_endpoints) {

				if (System.Text.RegularExpressions.Regex.IsMatch (path, endpoint.UriPath)) {

					return endpoint;

				}

			}

			return null;*/

		}

		public void AddEndpoint(IWebEndpoint interpreter) {

			m_endpoints.Add (interpreter);

		}

		public override void Start () {

			m_shouldRun = true;
			m_service = Task.Factory.StartNew (Service);

		}


		public override void Stop () {

			m_shouldRun = false;
		
		}


	}
}

