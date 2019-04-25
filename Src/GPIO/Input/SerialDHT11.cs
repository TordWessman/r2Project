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

namespace R2Core.GPIO
{
	internal class SerialDHT11 : SerialDeviceBase<int[]>, IDHT11 {
		private int m_port;

		private const int DHT11_TEMPERATURE_POSITION = 0x0;
		private const int DHT11_HUMIDITY_POSITION = 0x1;

		internal SerialDHT11  (string id, ISerialNode node, ISerialHost host, int port): base(id, node, host) {

			m_port = port;

		}

		protected override byte[] CreationParameters { get { return new byte[]{ (byte)m_port }; } }

		protected override SerialDeviceType DeviceType { get { return SerialDeviceType.DHT11; } }

		public int Temperature  { get { return GetValue(DHT11_TEMPERATURE_POSITION);} }

		public int Humidity  { get { return GetValue(DHT11_HUMIDITY_POSITION); } }

		private int GetValue(int DHT11ResponsePosition) {

			try {

				return GetValue()[DHT11ResponsePosition];

			} catch (SerialConnectionException ex) {

				if (ex.ErrorType == SerialErrorType.DHT11_READ_ERROR) {

					Log.w("DHT11 Read Error. Returning 0");

				} else { throw ex; }

			}

			return 0;

		}

		public IInputMeter<int> GetHumiditySensor(string id) { return new DHT11Sensor(id, this, DHT11Sensor.DHT11ValueType.Humidity); }

		public IInputMeter<int> GetTemperatureSensor(string id)  { return new DHT11Sensor(id, this, DHT11Sensor.DHT11ValueType.Temperature); }

	}
}

