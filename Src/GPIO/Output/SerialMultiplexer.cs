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

namespace R2Core.GPIO {

	internal class SerialMultiplexer : SerialDeviceBase<int>, IOutputPort<int> {

		private int m_value;
		private byte[] m_ports;

		public SerialMultiplexer(string id, ISerialNode node, IArduinoDeviceRouter host, int[] ports) : base(id, node, host) {

            m_ports = new byte[ports.Length];

            for (int i = 0; i < m_ports.Length; i++) { m_ports[i] = (byte)ports[i]; }

		}

		#region IOutputPort implementation

		public int Value {

			set {

				Host.Set(DeviceId, Node.NodeId, value);

				m_value = value;

			}

			get { return m_value; }

		}

		#endregion

		protected override byte[] CreationParameters { get {

			 byte[] parameters = new byte[m_ports.Length + 1];

                parameters[0] = (byte)m_ports.Length;

                for (byte i = 0; i < m_ports.Length; i++) {

                    parameters[i + 1] = m_ports[i];
                
                }

                return parameters;

            } 

		}

		protected override SerialDeviceType DeviceType => SerialDeviceType.Multiplex;

	}

}
