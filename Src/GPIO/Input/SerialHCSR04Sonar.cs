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
//
using System;
using Core.Device;

namespace GPIO
{
	public class SerialHCSR04Sonar: SerialDeviceBase, IInputMeter<int>
	{
		private int m_echoPort;
		private int m_triggerPort;

		public SerialHCSR04Sonar (string id, byte hostId, ISerialHost host, int triggerPort, int echoPort): base(id, hostId, host) {

			m_echoPort = echoPort;
			m_triggerPort = triggerPort;
		
		}

		protected override byte Update() {

			return Host.Create ((byte)HostId, SerialDeviceType.Sonar_HCSR04, new byte[]{  (byte)m_triggerPort, (byte)m_echoPort  });

		}

		public int Value { get { return ((int[]) Host.GetValue(DeviceId, HostId))[0]; } }

	}
}

