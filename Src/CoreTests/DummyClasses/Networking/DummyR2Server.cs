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
using Core.Network;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Device;

namespace Core.Tests
{
	public class DummyR2Server: DeviceBase, IBasicServer<IPEndPoint>
	{
		private IPEndPoint m_endpoint;

		public DummyR2Server () : base ("dummy_server")
		{
			m_endpoint = new IPEndPoint (IPAddress.Loopback, 0);
		}

		public IPEndPoint LocalEndPoint {get { return m_endpoint; } }
		public void AddObserver (Core.Network.Data.DataPackageType type, IDataReceived<byte[], IPEndPoint> observer) {}
		public void SetClientObserver (IClientMessageObserver observer) {}
		public void PrintConnections() {}

		public string Ip {get { return m_endpoint.Address.ToString (); }}
		public int Port {get { return m_endpoint.Port; }}
	
		public IDictionary<string,Task> GetTasksToObserve() {
			return new Dictionary<string,Task> ();
		}
	}
}

