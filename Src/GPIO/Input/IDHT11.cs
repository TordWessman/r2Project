using System;
using R2Core.Device;

namespace R2Core.GPIO
{
	/// <summary>
	/// DHT11 Temperature and humidity sensor.
	/// </summary>
	public interface IDHT11: IDevice {
		
		/// <summary>
		/// Temperature in C
		/// </summary>
		/// <value>The temperature.</value>
		int Temperature { get; }

		/// <summary>
		/// Relative humidity in percentage
		/// </summary>
		/// <value>The humidity.</value>
		int Humidity { get; }

		/// <summary>
		/// Returns an analog sensor for measuring the humidity of me.
		/// </summary>
		/// <returns>The humidity sensor.</returns>
		/// <param name="id">Identifier.</param>
		IInputMeter<int> GetHumiditySensor(string id);

		/// <summary>
		/// Returns an analog sensor for measuringing my temperature.
		/// </summary>
		/// <returns>The temperature sensor.</returns>
		/// <param name="id">Identifier.</param>
		IInputMeter<int> GetTemperatureSensor(string id);

	}
}

