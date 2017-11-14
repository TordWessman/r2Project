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
using System.Net;
using Core.Scripting;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Core.Device
{
	public class RemoteScriptExecutor : RemoteDeviceBase, IScriptExecutor
	{
		public static readonly string SET_VALUE_METHOD_NAME = "SET_VALUE_METHOD_NAME";
		public static readonly string GET_VALUE_METHOD_NAME = "GET_VALUE_METHOD_NAME";
		public static readonly string DONE_METHOD_NAME = "DONE_METHOD_NAME";

		public RemoteScriptExecutor (RemoteDeviceReference reference) : base (reference) {}

		#region IScriptExecutor implementation
		public void Set (string handle, object value)
		{
			KeyValuePair<string,object> input = new KeyValuePair<string, object> (handle, value);
			
			Execute<KeyValuePair<string,object>>(SET_VALUE_METHOD_NAME, input);

		}

		public object Get (string handle)
		{
			return Execute<object,string> (GET_VALUE_METHOD_NAME,handle);
			
		}
		
		public bool Done  { get {
			
				return Execute<bool> (DONE_METHOD_NAME);
			
			}
		}
		#endregion
	}
}

