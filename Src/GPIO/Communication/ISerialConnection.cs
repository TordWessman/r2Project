using System;
using R2Core.Device;

namespace R2Core.GPIO {

	/// <summary>
	/// A device capable of serial communication. Currently I2C (using libr2I2C.so) or USB/Serial connection is implemented.
	/// </summary>
	public interface ISerialConnection : IDevice {

		/// <summary>
		/// Will send the array of bytes to node and return the reply(if any)
		/// </summary>
		/// <param name="data">Data.</param>
		byte[] Send(byte[] data);

		/// <summary>
		/// Will try to read from the node.
		/// </summary>
		byte[] Read();

	}

}