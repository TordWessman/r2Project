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
using Core.Network.Data;
using System.Net;
using Core.Device;

namespace Core.Network
{
	/// <summary>
	/// I host manager.
	/// 
	/// Manages hosts connected to the system.
	/// </summary>

	public interface IHostManager<T> : ITaskMonitored, IDataReceived<byte[],IPEndPoint>, IDevice
	{
		IBasicServer<IPEndPoint> Server { get; }
		
		bool Has (IPEndPoint endpoint);
		int ConnectedHostsCount {get;}

		bool IsRunning {get;}

		/// <summary>
		/// Broadcast your existence using UDP to a specific host (in case you're not on the same subnet)
		/// </summary>
		/// <param name="host">Host.</param>
		void Broadcast (T host = default(T));

		/// <summary>
		/// Call this method to inform the host manager about a lost connection to a host.
		/// </summary>
		/// <param name="host">Host.</param>
		void HostDropped (T host);
		
		void SendToAll (IDataPackage package, bool async = true);
		
		void AddObserver(IHostManagerObserver observer);

		/// <summary>
		/// Sends a registration message using TCP rather than UDP
		/// </summary>
		/// <returns><c>true</c>, if me was registered, <c>false</c> otherwise.</returns>
		/// <param name="host">Host.</param>
		bool RegisterMe(IPEndPoint host);
		
	}
}

