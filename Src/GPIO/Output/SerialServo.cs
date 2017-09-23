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

namespace GPIO
{
	public class SerialServo: RemotlyAccessibleDeviceBase, IServo
	{

		//The identifier used by the serial slave device.
		private byte m_slaveId;

		private float m_value;
		private ISerialConnection m_connection;
		private ISerialPackageFactory m_packageFactory;

		public SerialServo (string id, byte slaveId, ISerialConnection connection, ISerialPackageFactory packageFactory): base(id)
		{
			m_slaveId = slaveId;
			m_packageFactory = packageFactory;
			m_connection = connection;
		}

		#region IOutputPort implementation

		public float Value  {

			set {

				m_value = value;
				m_connection.Send (m_packageFactory.SetDevice (m_slaveId, (int) value).ToBytes ());

			}

			get { return m_value; }

		}

		#endregion

		#region implemented abstract members of Core.Device.RemotlyAccessibleDeviceBase

		public override byte[] RemoteRequest (string methodName, byte[] rawData, IRPCManager<System.Net.IPEndPoint> mgr)
		{
			if (IsBaseMethod (methodName)) {
				return ExecuteStandardDeviceMethod (methodName, rawData, mgr);
			} else if (methodName == RemoteServo.GET_VALUE_FUNCTION_NAME) {
				return mgr.RPCReply<float> (Guid, methodName, Value);
			} else if (methodName == RemoteServo.SET_VALUE_FUNCTION_NAME) {
				Value = mgr.ParsePackage<float> (rawData);
			} else {
				throw new NotImplementedException ("Method name: " + methodName + " is not implemented for servo.");
			}

			return null;
		}

		public override RemoteDevices GetTypeId ()
		{
			return RemoteDevices.Servo;
		}

		#endregion


	}
}

