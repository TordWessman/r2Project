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
    internal class SerialAnalogInput : SerialDeviceBase<int[]>, IInputMeter<double> {

        protected byte[] m_ports;

        internal SerialAnalogInput(string id, ISerialNode node, IArduinoDeviceRouter host, int[] ports) : base(id, node, host) {

            m_ports = new byte[ports.Length];
            Denomiator = 1;
            for (int i = 0; i < ports.Length; i++) { m_ports[i] = (byte)ports[i]; }

        }

        protected override byte[] CreationParameters { get { return m_ports; } }

        protected override SerialDeviceType DeviceType { get { return SerialDeviceType.AnalogInput; } }

        public virtual double Value { get { return GetValue()[0] / Denomiator; } }

        /// <summary>
        /// Devides the value after reading. Provides an optional "normailzation" option for the returned value. 
        /// </summary>
        public double Denomiator{ get; set; }
	
	}

	internal class SimpleAnalogueHumiditySensor : SerialAnalogInput {

		internal SimpleAnalogueHumiditySensor(string id, ISerialNode node, IArduinoDeviceRouter host, int[] ports): base(id, node, host, ports) {

            if (node.ContinousSynchronization) {

                Log.w(  $"SimpleAnalogueHumiditySensor creation error: " +
                	    $"Node with id {node.NodeId} has ´ContinousSynchronization´ set to ´true´. This may cause probe electrolysis.");
            
            }
        
        }

		protected override SerialDeviceType DeviceType { get { return SerialDeviceType.SimpleMoist; } }

        public override double Value { 

            get {
                    double value = base.Value;
                    Log.i($"SimpleAnalogueHumiditySensor: Retrieved value: {value} for: {Identifier}");
                    return value; 

                }
                 
            }

    }

}
