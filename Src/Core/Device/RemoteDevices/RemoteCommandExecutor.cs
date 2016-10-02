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
using System.Collections.Generic;

namespace Core.Device
{
	public class RemoteCommandExecutor : RemoteDeviceBase, ICommandExecutor
	{
		public static readonly string SEND_FUNCTION_NAME = "send_string_function_name";
		public static readonly string REQUEST_FUNCTION_NAME = "request_string_function_name";
			
		public static readonly string SEND_BINARY_FUNCTION_NAME = "send_binary_function_name";
		public static readonly string REQUEST_BINARY_FUNCTION_NAME = "request_binary_function_name";
		
		public static readonly string SEND_OBJECT_FUNCTION_NAME = "send_object_function_name";

		public RemoteCommandExecutor (RemoteDeviceReference reference) : base (reference) {}

		#region ICommandExecutor implementation
		public string Request (string command)
		{
			return Execute<string,string> (REQUEST_FUNCTION_NAME, command);
		}
		
		public void SendMessage (string command)
		{
			Execute<string> (SEND_FUNCTION_NAME, command);
		}
		
		
		public string RequestBinary (int header, byte[] data)
		{
			KeyValuePair<int,byte[]> shit = new KeyValuePair<int,byte[]>(header, data);
			
			return Execute<string,KeyValuePair<int,byte[]>> (REQUEST_BINARY_FUNCTION_NAME, shit);
		}
		
		public void SendBinary (int header, byte[] data)
		{
			KeyValuePair<int,byte[]> shit = new KeyValuePair<int,byte[]>(header, data);
			
			Execute<KeyValuePair<int,byte[]>> (SEND_BINARY_FUNCTION_NAME, shit);
		}
		
		public void SendObject (int header, object obj)
		{
			KeyValuePair<int,object> shit = new KeyValuePair<int,object> (header, obj);
			
			Execute<KeyValuePair<int,object>> (SEND_OBJECT_FUNCTION_NAME, shit);
		}
		#endregion
	}
}

