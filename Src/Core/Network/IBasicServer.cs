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
using Core.Device;

namespace Core.Network
{
	/// <summary>
	/// Represents an object available through a network.
	/// </summary>
	public interface IEndpoint {
	
		/// <summary>
		/// Returns the Ip address on which the server is configured to listen to.
		/// </summary>
		/// <value>The port.</value>
		string Ip {get;}

		/// <summary>
		/// Returns the port on which the server is configured to listen to.
		/// </summary>
		/// <value>The port.</value>
		int Port {get;}

	}

	public interface IBasicServer<T> : IEndpoint, ITaskMonitored, IDevice
	{
		
		T LocalEndPoint {get;}
		void AddObserver (DataPackageType type, IDataReceived<byte[], T> observer);
		void SetClientObserver (IClientMessageObserver observer);
		void PrintConnections();

	}

}
