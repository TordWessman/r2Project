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

namespace GPIO
{
	public class SerialDHT11: SerialDeviceBase, IDHT11
	{
		private int m_port;

		private const int DHT11_TEMPERATURE_POSITION = 0x0;
		private const int DHT11_HUMIDITY_POSITION = 0x1;

		public SerialDHT11  (string id, byte nodeId, ISerialHost host, int port): base(id, nodeId, host) {

			m_port = port;

		}

		public int Temperature  { get { return ((int[]) Host.GetValue(DeviceId, NodeId))[DHT11_TEMPERATURE_POSITION]; } }

		public int Humidity  { get { return ((int[]) Host.GetValue(DeviceId, NodeId))[DHT11_HUMIDITY_POSITION]; } }

		public IInputMeter<int> GetHumiditySensor(string id) { return new DHT11Sensor (id, this, DHT11Sensor.DHT11ValueType.Humidity); }

		public IInputMeter<int> GetTemperatureSensor(string id)  { return new DHT11Sensor (id, this, DHT11Sensor.DHT11ValueType.Temperature); }


		protected override byte ReCreate() {

			return Host.Create ((byte)NodeId, SerialDeviceType.DHT11, new byte[]{ (byte)m_port });

		}

		public double Value { get { return (double) ((int[]) Host.GetValue(DeviceId, NodeId))[0]; } }
	}
}

