using System;
using Core.Device;

namespace GPIO
{
	public class DHT11Sensor: DeviceBase, IInputMeter<int>
	{
		private IDHT11 m_dht;
		private DHT11ValueType m_type;

		internal enum DHT11ValueType {
			Temperature,
			Humidity
		}

		internal DHT11Sensor (string id, IDHT11 dht11, DHT11ValueType type) : base (id)
		{

			m_dht = dht11;
			m_type = type;

		}

		public int Value { get { return m_type == DHT11ValueType.Temperature ? m_dht.Temperature : m_dht.Humidity; } }

	}
}

