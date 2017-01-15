using System;
using Core.Device;

namespace GPIO
{
	/// <summary>
	/// DHT11 Temperature and humidity sensor.
	/// </summary>
	public interface IDHT11: IDevice
	{
		/// <summary>
		/// Temperature in C
		/// </summary>
		/// <value>The temperature.</value>
		float Temperature { get; }

		/// <summary>
		/// Relative humidity in percentage
		/// </summary>
		/// <value>The humidity.</value>
		float Humidity { get; }

	}
}

