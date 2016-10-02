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
using Core.Network;
using Core.Network.Data;
using Core.Device;
using System.Collections.Generic;

namespace Core.Test
{
	public class PackageTest
	{
		private NetworkPackageFactory m_factory;
		public PackageTest ()
		{
			INetworkSecurity simpleSecurity = new SimpleNetworkSecurity ("test");

			m_factory = new NetworkPackageFactory (simpleSecurity);
		}
		
		public void TestSerialization ()
		{
			string ip = "10.0.0.10";
			int port = 42;
			
			IDataPackage registerMePackage = m_factory.CreateRegisterHostPackage (ip, port.ToString ());
			
			byte [] registerMePackageOut = m_factory.Serialize (registerMePackage);
			
			NetworkUtils.GetBasePackageType (registerMePackageOut);
			NetworkUtils.GetSubPackageType (registerMePackageOut);
			
			IDataPackageHeader registerMePackageHeader = m_factory.UnserializeHeader (registerMePackageOut);
			
			registerMePackageHeader.GetValue (HeaderFields.Ip.ToString ());
			int.Parse (registerMePackageHeader.GetValue (HeaderFields.Port.ToString ()));
			

			
			TestObject obj = new TestObject (new List<string>{"apa", "hund"});
			obj.Value = 42;
			obj.Ping = "hej";
			RemoteDeviceReference reference = new RemoteDeviceReference ("test", Guid.NewGuid().ToString(), null, null);
			IDummy d = new RemoteDummy (reference);
			
			IDataPackage<TestObject> p = m_factory.CreateRpcPackage<TestObject> (d.Identifier, "test", obj);
			
			byte [] output = m_factory.Serialize (p);
			
			//TestObject pOut = m_factory.Unserialize<TestObject> (output);
			int inputResValue = 43;
			IDataPackage<int> p2 = m_factory.CreateRpcPackage<int> (d.Identifier, "test", inputResValue);
			int res = m_factory.Unserialize<int> (m_factory.Serialize (p2));
			
			Console.WriteLine ((NetworkUtils.GetBasePackageType (output) == PackageTypes.DataPackage) + 
				" " + (NetworkUtils.GetSubPackageType (output) == DataPackageType.Rpc) + " " + (res == inputResValue));
			

			
			//IDataPackage testObjPackage = m_factory
		}
		
		//private 
	}
}

