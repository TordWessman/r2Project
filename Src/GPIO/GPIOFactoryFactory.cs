using System;
using Core.Device;

namespace GPIO
{
	public class GPIOFactoryFactory: DeviceBase
	{
		private const string SERIAL_CONNECTION_POSTFIX = "_serial";

		public GPIOFactoryFactory (string id) : base (id)
		{
		}

		public ArduinoGPIOFactory CreateArduinoSerialFactory(string id, string portIdentifier = null, int baudRate = ArduinoSerialConnector.DEFAULT_BAUD_RATE) {
		
			ISerialConnection connection = new ArduinoSerialConnector (id + SERIAL_CONNECTION_POSTFIX, portIdentifier, baudRate);
			return new ArduinoGPIOFactory (id, connection);

		}

		public ArduinoGPIOFactory CreateArduinoI2CFactory(string id, int bus = R2I2CMaster.DEFAULT_BUS, int port = R2I2CMaster.DEFAULT_PORT) {

			ISerialConnection connection = new R2I2CMaster (id + SERIAL_CONNECTION_POSTFIX, bus, port);
			return new ArduinoGPIOFactory (id, connection);

		}

		public PiGPIOFactory CreatePiFactory(string id) {

			return new PiGPIOFactory (id);

		}
	}
}

