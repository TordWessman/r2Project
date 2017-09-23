using System;
using Core.Device;

namespace GPIO
{
	public interface ISerialConnection: IDevice
	{

		/// <summary>
		/// Will send the array of bytes to slave and return the reply (if any)
		/// </summary>
		/// <param name="data">Data.</param>
		byte [] Send(byte []data);

		/// <summary>
		/// Will try to read from the slave.
		/// </summary>
		byte [] Read();

	}

}