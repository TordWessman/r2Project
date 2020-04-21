﻿// This file is part of r2Poject.
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
using R2Core.Network;
using System.Collections.Generic;
using System.Threading.Tasks;
using R2Core.Device;
using System.Net;
using System.Linq;

namespace R2Core.Network
{
	/// <summary>
	/// Contains some general functionality used by all IServers
	/// </summary>
	public abstract class ServerBase : DeviceBase,  IServer, ITaskMonitored {

		private int m_port;
		private bool m_shouldRun;
		private Task m_serviceTask;
		private IList<IWebEndpoint> m_endpoints;

		protected bool ShouldRun { get { return m_shouldRun; } }

		/// <summary>
		/// The task used by the service
		/// </summary>
		protected Task ServiceTask { get {return m_serviceTask; } }

		public ServerBase(string id) : base(id) {
			
			m_endpoints = new List<IWebEndpoint>();

		}

		~ServerBase() {

			Log.d($"Deallocating {this} [{Identifier}:{Guid.ToString()}].");
			Stop();
            m_serviceTask?.Dispose();

		}

		public string UriPath { get { return ".*"; } }

		public override bool Ready { get { return ShouldRun; } }

		public int Port { get { return m_port; } }

		public IEnumerable<string> Addresses { 

			get {

				foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)) {

					yield return address?.ToString();

				}

			}

		}

		protected void SetPort(int port) {

			m_port = port;

		}

		protected IWebEndpoint GetEndpoint(string path) {
		
			return m_endpoints.FirstOrDefault(endpoint => System.Text.RegularExpressions.Regex.IsMatch(path, endpoint.UriPath));

		}

		public void AddEndpoint(IWebEndpoint interpreter) {

			m_endpoints.Add(interpreter);

		}

		public override void Start() {

			m_shouldRun = true;
			m_serviceTask = Task.Factory.StartNew(Service, TaskCreationOptions.LongRunning);

		}


		/// <summary>
		/// Need to be implemented. Allows connection cleanup operations after Stop has been called.
		/// </summary>
		protected abstract void Cleanup();

		/// <summary>
		/// The service running the host connection. Will be called upon start
		/// </summary>
		protected abstract void Service();

		public abstract INetworkMessage Interpret(INetworkMessage request, System.Net.IPEndPoint source);

		public override void Stop() {

			m_shouldRun = false;
			try { Cleanup(); } catch (Exception ex) { Log.x(ex); } 
		
		}

		#region ITaskMonitored implementation
		public IDictionary<string,Task> GetTasksToObserve() {
			return new Dictionary<string, Task> { { Identifier, ServiceTask} };
		}
		#endregion

	}

}