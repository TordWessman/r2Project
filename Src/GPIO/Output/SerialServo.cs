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

namespace R2Core.GPIO
{
	internal class SerialServo: SerialDeviceBase<byte[]>, IServo {
		
		private readonly byte m_port;
		private float m_value;

		internal SerialServo  (string id, ISerialNode node, IArduinoDeviceRouter host, int port): base(id, node, host) {

			m_port = (byte)port;
		
		}

		protected override byte[] CreationParameters { get { return new byte[]{ m_port }; } }

		protected override SerialDeviceType DeviceType { get { return SerialDeviceType.Servo; } }

		#region IOutputPort implementation

		public float Value  {

			set {

                if (!Ready) { throw new System.IO.IOException("Unable to set Value. Device not Ready." + (Deleted ? " Deleted" : "")); }

                m_value = value;
				Host.Set(DeviceId, Node.NodeId, (int)value);

			}

			get { return m_value; }

		}

		#endregion

	}

}

