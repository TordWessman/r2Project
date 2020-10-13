using System;
using R2Core.Device;
using R2Core;

namespace R2Core.GPIO
{
    /// <summary>
    /// Allows the creation GPIO factories and connections dynamically, depending on hardware configuration.
    /// </summary>
	public class GPIOFactoryFactory : DeviceBase {
		
		public GPIOFactoryFactory(string id) : base(id) { }

		/// <summary>
		/// Creates a connection using the serial port
		/// </summary>
		/// <returns>The serial connection.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="portIdentifier">Port identifier.</param>
		/// <param name="baudRate">Baud rate.</param>
		public IArduinoDeviceRouter CreateSerialConnection(string id, string portIdentifier, int baudRate = 0) {
		
			if (baudRate <= 0) {
			
				baudRate = Settings.Consts.ArduinoSerialConnectorBaudRate();

			}

			ISerialConnection connection = new ArduinoSerialConnector(id, portIdentifier, baudRate);
			return new ArduinoDeviceRouter(id, connection, new ArduinoSerialPackageFactory());

		}

		/// <summary>
		/// Creates a connection using the I2C protocol
		/// </summary>
		/// <returns>The serial connection.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="bus">Bus.</param>
		/// <param name="port">Port.</param>
		public IArduinoDeviceRouter CreateSerialConnection(string id, int? bus = null, int? port = null) {

			ISerialConnection connection = new R2I2CMaster(id, bus, port);
			return new ArduinoDeviceRouter(id, connection, new ArduinoSerialPackageFactory());

		}

        /// <summary>
        /// Create a GPIO factory that uses an IArduinoDeviceRouter as connection to the hardware.
        /// </summary>
        /// <returns>The serial factory.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="connection">Serial host.</param>
		public SerialGPIOFactory CreateSerialFactory(string id, IArduinoDeviceRouter connection) {

			return new SerialGPIOFactory(id, connection);

		}

        /// <summary>
        /// Creates a GPIO factory that targets the GPIO of the Raspberry Pi.
        /// </summary>
        /// <returns>The pi factory.</returns>
        /// <param name="id">Identifier.</param>
		public PiGPIOFactory CreatePiFactory(string id) {

			return new PiGPIOFactory(id);

		}
	}
}

