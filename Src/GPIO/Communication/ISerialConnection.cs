using System;
using R2Core.Device;

namespace R2Core.GPIO {

	/// <summary>
	/// A device capable of serial communication. Currently I2C (using libr2I2C.so) or USB/Serial connection is implemented.
	/// </summary>
	public interface ISerialConnection : IDevice {

        /// <summary>
        /// Returns <c>true</c> if the connection should be up and running (<c>Ready</c> returns <c>true</c> only if it's connected, which might be unreliable.
        /// </summary>
        /// <value><c>true</c> if should runt; otherwise, <c>false</c>.</value>
        bool ShouldRun { get; }

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