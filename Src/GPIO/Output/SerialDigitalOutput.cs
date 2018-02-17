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
	public class SerialDigitalOutput : SerialDeviceBase, IOutputPort
	{
		private int m_port;
		private bool m_value;

		public SerialDigitalOutput (string id, byte nodeId, ISerialHost host, int port): base(id, nodeId, host) {

			m_port = port;

		}
			
		protected override byte ReCreate() {

			return Host.Create ((byte)NodeId, SerialDeviceType.DigitalOutput, new byte[]{  (byte)m_port  });

		}

		#region IOutputPort implementation

		public bool Value  {

			set {

				m_value = value;
				Host.Set (DeviceId, NodeId, value ? 1 : 0);

			}

			get { return m_value; }

		}

		#endregion

	}

}