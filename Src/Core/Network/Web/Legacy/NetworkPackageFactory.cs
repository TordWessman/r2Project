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
using System.Collections.Generic;
using System.Net;


namespace Core.Network
{
	public class NetworkPackageFactory : DataPackageFactory, INetworkPackageFactory
	{


		public NetworkPackageFactory (INetworkSecurity security) : base (security)
		{

		}

		private IDataPackageHeader CreateRPCPackageHeader (string target, string methodName)
		{

			IDictionary<string, string> headerValues = new Dictionary<string, string> ();
			headerValues.Add (HeaderFields.Method.ToString (), methodName);
			headerValues.Add (HeaderFields.Target.ToString (), target);
			headerValues.Add (HeaderFields.Checksum.ToString (), m_security.Token);
			
			IDataPackageHeader header = new DataPackageHeader (headerValues);
			
			return header;

		}
		
		public IDataPackage CreateRpcPackage (string target, string methodName)
		{

			IDataPackageHeader header = CreateRPCPackageHeader (target, methodName);
			return new DataPackage (header, DataPackageType.Rpc);
		
		}
			
		public IDataPackage<T> CreateRpcPackage<T> (string target, string methodName, T data)
		{

			IDataPackageHeader header = CreateRPCPackageHeader (target, methodName);
			return new DataPackage<T>(header, data, DataPackageType.Rpc);
		
		}
		
		
		public IDataPackage CreateRegisterHostPackage (string ip, string port)
		{

			IDictionary<string,string> headerFields = new Dictionary<string, string> ();
			
			headerFields.Add (HeaderFields.Ip.ToString (), ip);
			headerFields.Add (HeaderFields.Port.ToString (), port);
			headerFields.Add (HeaderFields.Checksum.ToString (), m_security.Token);

			IDataPackage registerMePackage = CreateEmptyPackage (
				DataPackageType.RegisterThisHost,
				headerFields
			);
			
			return registerMePackage;

		}
		
		public IDataPackage CreateRemoveHostPackage (string ip, string port)
		{

			IDictionary<string,string> headerFields = new Dictionary<string, string> ();
			
			headerFields.Add (HeaderFields.Ip.ToString (), ip);
			headerFields.Add (HeaderFields.Port.ToString (), port);
			headerFields.Add (HeaderFields.Checksum.ToString (), m_security.Token);

			IDataPackage removeMePackage = CreateEmptyPackage (
				DataPackageType.RemoveThisHost,
				headerFields
			);
			
			return removeMePackage;

		}
		
		public IDataPackage CreateDeviceAddedPackage (
			string identifier,
			Guid guid,
			string deviceType,
		    IBasicServer<IPEndPoint> sourceServer)
		{

			IDictionary<string,string> headerFields = new Dictionary<string, string> ();
				
			headerFields.Add (HeaderFields.DeviceId.ToString (), identifier);
			headerFields.Add (HeaderFields.Target.ToString (), guid.ToString());
			headerFields.Add (HeaderFields.DeviceType.ToString (), deviceType);
			headerFields.Add (HeaderFields.Ip.ToString (), sourceServer.Ip);
			headerFields.Add (HeaderFields.Port.ToString (), sourceServer.Port.ToString());
			headerFields.Add (HeaderFields.Checksum.ToString (), m_security.Token);

			return CreateEmptyPackage (
					DataPackageType.DeviceAdded,
					headerFields);

		}
		
		
		public IDataPackage CreateDeviceRemovedPackage (Guid guid)
		{

			IDictionary<string,string> headerFields = new Dictionary<string, string> ();
				
			headerFields.Add (HeaderFields.Target.ToString (), guid.ToString());
			headerFields.Add (HeaderFields.Checksum.ToString (), m_security.Token);

			return CreateEmptyPackage (
					DataPackageType.DeviceRemoved,
					headerFields);

		}
		
		public IDataPackage CreateMemoryBusOnlinePackage () {

			return CreateEmptyPackage (
					DataPackageType.MemoryBusOnline);

		}

	}

}

