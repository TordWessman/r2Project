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
	public class SerialDigitalInput : DigitalInputBase, ISerialDevice
	{

		// The id of the device at the slave device
		private byte m_deviceId;
		// The id for the host where the device resides
		private byte m_nodeId;

		// The I/O port on the device
		private int m_port;

		private ISerialHost m_host;

		// ISerialDevice implementation
		public void Synchronize() {

			m_deviceId = m_host.Create (m_nodeId, SerialDeviceType.DigitalInput, new byte[]{  (byte)m_port  });

		}

		public SerialDigitalInput (string id, byte nodeId, ISerialHost host, int port): base(id)
		{
			m_port = port;
			m_host = host;
			m_nodeId = nodeId;
		}

		#region IInputPort implementation

		public override bool Value { get { return ((int[]) m_host.GetValue(m_deviceId, m_nodeId))[0] == 1; } }

		#endregion

	}
}

