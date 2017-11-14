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
using Core.Device;
using System.Net;
using System.Threading.Tasks;

namespace Core.Device
{
	/// <summary>
	/// IRPC manager.
	/// 
	/// Interface for remote procedure calls on devices.
	/// 
	/// </summary>
	public interface IRPCManager<N>
	{
		
		ITaskMonitor TaskMonitor {get;}
			
		Task<T> RPCRequest<T,K> (Guid target, string methodName, N endPoint, K data);
		Task RPCRequest<K> (Guid target, string methodName, N endPoint, K data);
		Task<T> RPCRequest<T> (Guid target, string methodName, N endPoint);
		Task RPCRequest (Guid target, string methodName, N endPoint);
		
		byte [] RPCReply<T>(Guid target, string methodName, T data);
		byte [] RPCReply(Guid target, string methodName);
		
		T ParsePackage<T> (byte[]rawPackage);
		bool HostAvailable (N endPoint);

	}
}

