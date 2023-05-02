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
using System.Linq;
using R2Core.Device;

namespace R2Core.GPIO {

    internal class SerialMultipleDigitalOutput : SerialDeviceBase<byte[]>, IOutputPort<bool[]> {

        private byte[] m_ports;
        private bool[] m_value;

        internal SerialMultipleDigitalOutput(string id, ISerialNode node, IArduinoDeviceRouter host, int[] ports) : base(id, node, host) {

            m_ports = new byte[ports.Length];

            for (int i = 0; i < m_ports.Length; i++) { m_ports[i] = (byte)ports[i]; }

        }

        protected override byte[] CreationParameters { get {

                byte[] parameters = new byte[m_ports.Length + 1];

                parameters[0] = (byte)m_ports.Length;

                for (byte i = 0; i < m_ports.Length; i++) {

                    parameters[i + 1] = m_ports[i];
                
                }

                foreach(byte p in parameters) {
                    Log.t($"Portis: {p}");
                }

                return parameters;
            } 

        }

        protected override SerialDeviceType DeviceType { get { return SerialDeviceType.MultipleDigitalOutput; } }

        public void S1(bool a1) { Value = new bool[1] { a1 }; }
        public void S2(bool a1, bool a2) { Value = new bool[2] { a1, a2 }; }
        public void S3(bool a1, bool a2, bool a3) { Value = new bool[3] { a1, a2, a3 }; }
        public void S4(bool a1, bool a2, bool a3, bool a4) { Value = new bool[4] { a1, a2, a3, a4 }; }

        #region IOutputPort implementation

        public bool[] Value {

            set {

                if (!Ready) { throw new System.IO.IOException("Unable to set Value. Device not Ready." + (Deleted ? " Deleted" : "")); }

                if (value.Length != m_ports.Length) {

                    throw new ArgumentException($"Invalid number of values ({value.Length}). {m_ports.Length} defined.");

                }

                m_value = value;
                int bitshifted = 0;

                foreach (bool portValue in m_value.Reverse()) {

                    bitshifted <<= 1;
                    bitshifted |= portValue ? 1 : 0;
                     
                }

                Host.Set(DeviceId, Node.NodeId, bitshifted);

            }

            get { return m_value; }

        }

        #endregion

        public override string ToString() {

            return $"[OutputPort: {Identifier} : {Value}]";

        }

        public override void Stop() {
            base.Stop();

            if (m_value != null) {

                if (Host.Ready) { Value = new bool[m_value.Length]; }

            }

        }

    }

}