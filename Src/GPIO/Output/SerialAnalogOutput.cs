﻿// This file is part of r2Poject.
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

    internal class SerialAnalogOutput : SerialDeviceBase<byte[]>, IOutputPort<ushort> {

        private readonly byte m_port;
        private ushort m_value;

        public ushort Value {

            get { return m_value; }

            set {

                Host.Set(DeviceId, Node.NodeId, value);

                m_value = value;

            }

        }

        internal SerialAnalogOutput(string id, ISerialNode node, IArduinoDeviceRouter host, int port) : base(id, node, host) {

            m_port = (byte)port;

        }

        protected override byte[] CreationParameters { get { return new byte[] { m_port }; } }

        protected override SerialDeviceType DeviceType { get { return SerialDeviceType.AnalogOutput; } }

        public override string ToString() {

            return $"[AnalogeOutput: {Identifier} : {Value}]";

        }

        public override void Stop() {
            base.Stop();

            if (Host.Ready) { Value = 0; }

        }

    }

}
