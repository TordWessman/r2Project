﻿using System;
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

		/// <summary>
		/// Returns an analog sensor for measuring the humidity of me.
		/// </summary>
		/// <returns>The humidity sensor.</returns>
		/// <param name="id">Identifier.</param>
		IInputMeter<float> GetHumiditySensor(string id);

		/// <summary>
		/// Returns an analog sensor for measuringing my temperature.
		/// </summary>
		/// <returns>The temperature sensor.</returns>
		/// <param name="id">Identifier.</param>
		IInputMeter<float> GetTemperatureSensor(string id);

	}
}

