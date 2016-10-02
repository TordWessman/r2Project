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
using System.Collections.Generic;
using Core.Network;
using System.Net;

namespace Core.Network.Data
{

	public interface IDataPackageFactory
	{
		
		
		byte [] Serialize<T>(IDataPackage<T> sourcePackage);
		byte [] Serialize (IDataPackage sourcePackage);
		
		IDataPackageHeader UnserializeHeader (byte[]rawPackage);
		
		T Unserialize<T> (byte[]rawPackage);
		int GetHeaderSize (byte[]rawPackage);
	
	
		//IDataPackage CreateEmptyPackage (DataPackageType type, IDictionary<string,string> fields = null);
		
		/*
		IDataPackage CreateDeviceAddedPackage (string target, string deviceType,  IBasicServer<IPEndPoint> sourceServer);
		IDataPackage CreateRegisterHostPackage (string ip, string port);
		IDataPackage CreateRemoveHostPackage (string ip, string port);
		IDataPackage<T> CreateRpcPackage<T>(string target, string methodName, T data);
		IDataPackage CreateRpcPackage (string target, string methodName);
		*/
		
	}
	
	
	
}

