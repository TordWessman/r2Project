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
using System.Net;

namespace Core.Device
{
	public class RemoteGstream : RemoteDeviceBase, IGstream, IJSONAccessible
	{
		public static readonly string IS_RUNNING_METHOD_NAME = "is_running";

		public RemoteGstream (RemoteDeviceReference reference) : base (reference) {}

		public bool IsRunning {

			get {
				return Execute<bool> ("is_running");
			}
		}


		#region IExternallyAccessible implementation
		public string Interpret (string functionName, string parameters = null)
		{

			if (functionName == "start") {

				Start ();

				return IsRunning.ToString();

			} else 	if (functionName == "stop") {

				Stop ();

				return Ready.ToString();

			} else if (functionName == IS_RUNNING_METHOD_NAME) {

				return Ready.ToString();

			}

			throw new NotImplementedException ("Gstream '" + Identifier + "': Unable to interpret IJSONAccessible.Interpret: " + functionName);

		}

		#endregion
	
	}
}

