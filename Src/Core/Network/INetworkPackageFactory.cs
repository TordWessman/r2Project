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

﻿using System;
using Core.Network.Data;
using System.Net;

namespace Core.Network
{
	/// <summary>
	/// Interface for creation of inter communication packages (Rpc, brodcasting etc).
	/// </summary>
	public interface INetworkPackageFactory : IDataPackageFactory
	{

		/// <summary>
		/// Creates an un-typed Rpc package (remote procedure without return).
		/// </summary>
		/// <returns>The rpc package.</returns>
		/// <param name="target">Target.</param>
		/// <param name="methodName">Method name.</param>
		IDataPackage CreateRpcPackage (string target, string methodName);

		/// <summary>
		/// Creates an un-typed Rpc package (remote procedure with return data of type T).
		/// </summary>
		/// <returns>The rpc package.</returns>
		/// <param name="target">Target.</param>
		/// <param name="methodName">Method name.</param>
		IDataPackage<T> CreateRpcPackage<T> (string target, string methodName, T data);

		/// <summary>
		/// Package urging remote endpoint to register me among hosts 
		/// </summary>
		/// <returns>The register host package.</returns>
		/// <param name="ip">Ip.</param>
		/// <param name="port">Port.</param>
		IDataPackage CreateRegisterHostPackage (string ip, string port);

		/// <summary>
		/// Informing a host about my dismissal.
		/// </summary>
		/// <returns>The remove host package.</returns>
		/// <param name="ip">Ip.</param>
		/// <param name="port">Port.</param>
		IDataPackage CreateRemoveHostPackage (string ip, string port);

		/// <summary>
		/// Tell a host about a new IDevice that has been added and should be available vie Rpc
		/// </summary>
		/// <returns>The device added package.</returns>
		/// <param name="identifier">Identifier.</param>
		/// <param name="guid">GUID.</param>
		/// <param name="deviceType">Device type.</param>
		/// <param name="sourceServer">Source server.</param>
		IDataPackage CreateDeviceAddedPackage (
			string identifier,
			Guid guid,
			string deviceType,
			IBasicServer<IPEndPoint> sourceServer);

		/// <summary>
		/// Inform a host about a device that have been removed.
		/// </summary>
		/// <returns>The device removed package.</returns>
		/// <param name="guid">GUID.</param>
		IDataPackage CreateDeviceRemovedPackage (Guid guid);

		/// <summary>
		/// Obsolete. Creates the memory bus online package.
		/// </summary>
		/// <returns>The memory bus online package.</returns>
		IDataPackage CreateMemoryBusOnlinePackage ();

	}

}

