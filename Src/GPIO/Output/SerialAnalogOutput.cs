using System;
using R2Core.Device;

namespace R2Core.GPIO
{
    internal class SerialAnalogOutput : SerialDeviceBase<byte[]>, IOutputPort<byte> {

        private byte m_port;
        private byte m_value;

        public byte Value {

            get { return m_value; }
            set {

                m_value = value;
                Host.Set(DeviceId, NodeId, value);

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

    }

}
