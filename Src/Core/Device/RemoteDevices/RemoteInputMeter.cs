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
using System.Threading.Tasks;

namespace Core.Device
{

	public class RemoteInputMeter : RemoteDeviceBase, IInputMeter<int>
	{
		public static readonly string GET_VALUE_FUNCTION_NAME = "get_cm_function_name";

		public RemoteInputMeter (RemoteDeviceReference reference) : base (reference) {}

		#region IDistanceMeter implementation

		public int Value {

			get {
			
				return Execute<int> (GET_VALUE_FUNCTION_NAME);
			
			}
		
		}

		#endregion

	}

}

