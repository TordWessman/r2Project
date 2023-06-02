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
using System.Linq;
using R2Core.Device;

namespace R2Core.GPIO {

    /// <summary>
    /// A simple IInputMeter interface for fetching a value from a `SerialMoistureMultiplexer`.
    /// </summary>
    internal class SerialMultiplexerMoistureSensor: DeviceBase, IInputMeter<int> {

        private SerialMoistureMultiplexer m_multiplexer;
        private int m_sensorPairIndex;

        internal SerialMultiplexerMoistureSensor(string id, SerialMoistureMultiplexer multiplexer, int sensorPairIndex) : base(id) {

            m_multiplexer = multiplexer;
            m_sensorPairIndex = sensorPairIndex;

        }

        public int Value => m_multiplexer.ValueFor(m_sensorPairIndex);

    }

    /// <summary>
    /// This is a weird interface for an intricate circuit used for measuring soil humidity using a pair of measurment rods and a multiplexer.
    /// </summary>
    internal class SerialMoistureMultiplexer: SerialDeviceBase<int>, ISerialMultiplexerInput<int> {

        /// The value denotes the number of rods per sensor. I's specified (and compiled) in the SENSOR_ROD_COUNT 
        /// definition of the r2Moist.h and should not be changed unless the r2I2CDeviceRouter and r2Moist.h has been changed 
        /// updated accordingly.
        private const int SENSOR_ROD_COUNT = 2;

        private byte[] m_channelSelectionPorts;
        private byte[] m_controlPorts;
        private byte[] m_sensorPairs;
        private byte m_analogPort;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:R2Core.GPIO.SerialMoist"/> class. See the R2Moist project for details.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="node">Node.</param>
        /// <param name="host">Router managing the connection.</param>
        /// <param name="channelSelectionPorts">The ports used for the multiplexer's channel selection</param>
        /// <param name="controlPorts">The ports controlling the output voltage to the measurment rods. Should have a length of 2 with the default R2Moist compilation</param>
        /// <param name="sensorPairs">An array of sensor pairs. 2 ports for the multiplexer per rod. A multiplexer with two sensor makes 4 rods and thus 4 sensor pair ports.</param>
        internal SerialMoistureMultiplexer(string id, ISerialNode node, IArduinoDeviceRouter host, int[] channelSelectionPorts, int[] controlPorts, int[] sensorPairs, int analogPort) : base(id, node, host) {

            if (controlPorts.Length != SENSOR_ROD_COUNT) { throw new ArgumentException($"The number of of `controlPorts` should be equal to {SENSOR_ROD_COUNT}"); }
            if (sensorPairs.Length % SENSOR_ROD_COUNT != 0) { throw new ArgumentException($"The length of of `sensorPairs` should be evenly dividable by {SENSOR_ROD_COUNT}"); }

            m_channelSelectionPorts = channelSelectionPorts.Select(i => (byte) i).ToArray();
            m_controlPorts = controlPorts.Select(i => (byte) i).ToArray();
            m_sensorPairs = sensorPairs.Select(i => (byte) i).ToArray();
            m_analogPort = (byte) analogPort;

        }

        protected override byte[] CreationParameters { 
        
            get {


                byte[] result = new byte[3 + m_channelSelectionPorts.Length + m_controlPorts.Length + m_sensorPairs.Length + 1];

                result[0] = (byte) m_channelSelectionPorts.Length;
                int pos = 1;
                Array.Copy(m_channelSelectionPorts, 0, result, pos, m_channelSelectionPorts.Length);
                pos += m_channelSelectionPorts.Length;

                result[pos] = (byte) m_controlPorts.Length;
                Array.Copy(m_controlPorts, 0, result, pos + 1, m_controlPorts.Length);
                pos += 1 + m_controlPorts.Length;

                result[pos] = (byte)m_sensorPairs.Length;
                Array.Copy(m_sensorPairs, 0, result, pos + 1, m_sensorPairs.Length);
                pos += 1 + m_sensorPairs.Length;

                result[pos] = m_analogPort;

                return result;               
    
            } 
       
       }

        protected override SerialDeviceType DeviceType => SerialDeviceType.MultiplexMoist;

        /// <summary>
        /// Returns the value for a sensor pair of index `sensorPairIndex`. This index is a position for each tuple of sensors defined 
        /// in the constructor (`sensorPairs`). If the multiplexer has 2 sensor pairs defined (i.e. multiplexer ports {1, 2, 3, 4}, the
        /// index of `0` will refer to the first sensor pairs {1, 2} whereas index `1` will refer to the rods connected to multiplexer port {3, 4}.
        /// (All assuming 2 rods per sensor).
        /// </summary>
        /// <returns>The for.</returns>
        /// <param name="sensorIndex">Sensor pair index.</param>
        public int ValueFor(int sensorIndex) {

            return GetValue(new byte[1] { (byte)sensorIndex });

        }

        /// <summary>
        /// Will wrap the multiplexer as a simpler `SerialMultiplexerMoistureSensor` which represents a sensor
        /// defined by the `sensorPairs` in the constructor (will hence not _create_ a sensor at the host, but rather
        /// act as a weak link to a sensor already defined.
        /// </summary>
        /// <returns>The sensor.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="sensorPairIndex">Sensor pair index.</param>
        public IInputMeter<int> LinkSensor(string id, int sensorPairIndex) {

            return new SerialMultiplexerMoistureSensor(id, this, sensorPairIndex);

        }

    }

}

