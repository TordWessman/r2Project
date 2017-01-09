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
//using Core.Network;
//using Core.Network.Data;
using System.Net;
using System.Threading.Tasks;

namespace Core.Device
{
	public class RemoteDummy : RemoteDeviceBase, IDummy
	{

		public RemoteDummy (RemoteDeviceReference reference) : base (reference) {}

		public int Value { get { return Execute<int> ("get_Value"); } 
			set { Execute<int> ("Set_value", value); }
		}
		#region IDummy implementation
		public int Test (TestObject testObj)
		{
			Console.WriteLine (" Will try to connect to host: " + m_host.ToString ());
			Task<int> fetch = m_networkManager.RPCRequest<int,TestObject> (Guid, "test", m_host, testObj);;
			
			fetch.Wait ();
	
			return fetch.Result;
			
		}
		#endregion



		#region IDummy implementation
		public void Test2 (TestObject testObj)
		{
			Console.WriteLine (" Will try to connect to host: " + m_host.ToString ());
			Task fetch = m_networkManager.RPCRequest<TestObject> (testObj.Guid, "test2", m_host, testObj);
			
			fetch.Wait ();

		}

		public TestObject Test3 ()
		{
			Console.WriteLine (" Will try to connect to host: " + m_host.ToString ());
			Task<TestObject> fetch = m_networkManager.RPCRequest<TestObject> (Guid, "test3", m_host);

			fetch.Wait ();
	
			return fetch.Result;
		}

		public void Test4 ()
		{
			Console.WriteLine (" Will try to connect to host: " + m_host.ToString ());
			Task fetch = m_networkManager.RPCRequest (Guid, "test4", m_host);

			fetch.Wait ();
		}
		#endregion
	}
}

