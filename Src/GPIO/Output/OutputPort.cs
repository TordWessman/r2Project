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
using Core;


namespace GPIO
{
	public class OutputPort : RemotlyAccessableDeviceBase, IOutputPort
	{
		RaspberryPiDotNet.GPIO m_gpi;

		public OutputPort (string id, RaspberryPiDotNet.GPIO gpio) : base (id)
		{
			m_gpi = gpio;
			
			if (m_gpi.PinDirection != RaspberryPiDotNet.GPIODirection.Out) {
				throw new ArgumentException ("Provided GPIO pin had not out-direction");
			}
		}

		#region IRemotlyAccessable implementation
		public override byte[] RemoteRequest (string methodName, byte[] rawData, IRPCManager<System.Net.IPEndPoint> mgr)
		{
			if (IsBaseMethod (methodName)) {
				return ExecuteStandardDeviceMethod (methodName, rawData, mgr);
			} else if (methodName == RemoteOutputPort.SET_VALUE_FUNCTION_NAME) {
				Set(mgr.ParsePackage<bool> (rawData));
				return null;
			} else
				throw new NotImplementedException ("Method name: " + methodName + " is not implemented for Distance meter.");

		}

		public override RemoteDevices GetTypeId ()
		{
			return RemoteDevices.OutputPort;
		}
		#endregion

		#region IOutputPort implementation
		public void Set ( bool value )
		{
			m_gpi.Write (value);
		}

		#endregion

	}

}

