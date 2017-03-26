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
using Core.Network.Data;
using Core.Device;

namespace Core.Tests
{
	public class DummyHostManager : DeviceBase, IHostManager<IPEndPoint>
	{
		private IBasicServer<IPEndPoint> m_server;

		public DummyHostManager (IBasicServer<IPEndPoint> server) : base("dummyhostmanager")
		{
			m_server = server;
		}

		public IBasicServer<IPEndPoint> Server { get { return m_server; } }

		public bool Has (IPEndPoint endpoint) { return false;}

		public int ConnectedHostsCount {get { return 0; }}

		public bool IsRunning {get { return true; }}

		public void Broadcast (IPEndPoint host = default(IPEndPoint)) {}

		public void HostDropped (IPEndPoint host) {}

		public void SendToAll (Core.Network.Data.IDataPackage package, bool async = true) {}

		public void AddObserver(IHostManagerObserver observer) {}

		public bool RegisterMe(IPEndPoint host) { return true; } 

		//ITaskMonitored
		public IDictionary<string,Task> GetTasksToObserve() { return new Dictionary<string, Task> (); }

		//IDataReceived
		public byte[] DataReceived(DataPackageType type, byte[] rawData, IPEndPoint source) { return new byte[0]; }

	}
}

