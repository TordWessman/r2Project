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
using R2Core.Device;

namespace R2Core.GPIO
{
	internal class SerialDigitalInput : SerialDeviceBase<bool>, IInputPort {
		
		// The I/O port on the device
		private byte m_port;

		internal SerialDigitalInput  (string id, ISerialNode node, IArduinoDeviceRouter host, int port): base(id, node, host) {

			m_port = (byte)port;

		}

		protected override byte[] CreationParameters { get { return new byte[]{ (byte)m_port }; } }

		protected override SerialDeviceType DeviceType { get { return SerialDeviceType.DigitalInput; } }

		public bool Value { get { return GetValue(); } }

		public void SetTrigger(Func<object> triggerFunc, bool pullUp = false, int interval = 100) {
			//TODO:
			throw new NotImplementedException();
		}

		public void StopTrigger() {
			//TODO:
			throw new NotImplementedException();
		}

	}
}

