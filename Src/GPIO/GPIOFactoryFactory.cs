using System;
using Core.Device;
using Core;

namespace GPIO
{
	public class GPIOFactoryFactory: DeviceBase
	{
		
		public GPIOFactoryFactory (string id) : base (id)
		{
		}

		/// <summary>
		/// Creates a connection using the serial protocol
		/// </summary>
		/// <returns>The serial connection.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="portIdentifier">Port identifier.</param>
		/// <param name="baudRate">Baud rate.</param>
		public ISerialHost CreateSerialConnection(string id, string portIdentifier = null, int baudRate = 0) {
		
			if (baudRate <= 0) {
			
				baudRate = Settings.Consts.ArduinoSerialConnectorBaudRate ();

			}

			ISerialConnection connection = new ArduinoSerialConnector (id, portIdentifier, baudRate);
			return new SerialHost(id, connection, new ArduinoSerialPackageFactory());

		}

		/// <summary>
		/// Creates a connection using the I2C protocol
		/// </summary>
		/// <returns>The serial connection.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="bus">Bus.</param>
		/// <param name="port">Port.</param>
		public ISerialHost CreateSerialConnection(string id, int? bus = null, int? port = null) {

			ISerialConnection connection = new R2I2CMaster (id, bus, port);
			return new SerialHost(id, connection, new ArduinoSerialPackageFactory());

		}

		public SerialGPIOFactory CreateSerialFactory(string id, ISerialHost serialHost) {

			return new SerialGPIOFactory (id, serialHost);

		}

		public PiGPIOFactory CreatePiFactory(string id) {

			return new PiGPIOFactory (id);

		}
	}
}

