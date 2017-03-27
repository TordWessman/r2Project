using System;
using Core.Device;

namespace GPIO
{
	public interface II2CMaster: IDevice
	{

		/// <summary>
		/// Will send the array of bytes to slave and return the reply (if any)
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="responseSize">Expected data size.</param>
		byte [] Send(byte []data, int responseSize);

		/// <summary>
		/// Will try to read from the slave. Requires a size parameter.
		/// </summary>
		/// <param name="responseSize">Expected data size.</param>
		byte [] Read(int responseSize);

	}

}