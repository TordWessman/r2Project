using R2Core.Device;

namespace R2Core.GPIO
{
    /// <summary>
    /// Allows the creation GPIO factories and connections dynamically, depending on hardware configuration.
    /// </summary>
	public class GPIOFactoryFactory : DeviceBase {
		

		public GPIOFactoryFactory(string id) : base(id) { }

        /// <summary>
        /// The default package factory used to serialize/deserialize packages.
        /// </summary>
        public ISerialPackageFactory PackageFactory = new ArduinoSerialPackageFactory();

        /// <summary>
        /// Creates a connection using the serial port to a R2I2CDeviceRouter device.
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
			return new ArduinoDeviceRouter(id, connection, PackageFactory);

		}

        /// <summary>
        /// Creates a R2I2CDeviceRouter connection using a TCP connection (typically for WiFi enabled devices).
        /// </summary>
        /// <returns>The serial TCPC onnection.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="host">Host.</param>
        /// <param name="port">Port.</param>
        public IArduinoDeviceRouter CreateSerialTCPConnection(string id, string host, int port = 0) {

            if (port <= 0) {

                port = Settings.Consts.TCPSerialConnectionDefaultPort();
            
            }

            ISerialConnection connection = new TCPSerialConnection(id, host, port);
            return new ArduinoDeviceRouter(id, connection, PackageFactory);

        }

        /// <summary>
        /// Creates a connection using the I2C protocol to a R2I2CDeviceRouter device. Requires
        /// a I2C port on this host machine.
        /// </summary>
        /// <returns>The serial connection.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="bus">Bus.</param>
        /// <param name="port">Port.</param>
        public IArduinoDeviceRouter CreateSerialConnection(string id, int? bus = null, int? port = null) {

			ISerialConnection connection = new R2I2CMaster(id, bus, port);
			return new ArduinoDeviceRouter(id, connection, PackageFactory);

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

