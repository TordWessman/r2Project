This project is the bridge between r2Project and Arduino hardware.

### Fix dependencies
* Find your Arduino library folder (i.e. _~/Arduino/libraries/_).
* Add _../../r2I2C_ to your Arduino library folder. (i.e. `ln -s <path>/r2Project/Arduino/r2I2C ~/Arduino/libraries`)
* Add the RF24 libraries _(../../3rdParty/RF24-Libraries/*)_ to your Arduino library folder.
* Add the DHT11 library _(../../3rdParty/DHT11)_ to your Arduino library folder.
* restart Arduino Studio.

### Create your local configuration file
`$ cp r2I2C_config.h.template r2I2C_config.h`

An Arduino running `r2I2CDeviceRouter` acts as a bridge to the `R2Core.GPIO.SerialGPIOFactory`
via an `IArduinoDeviceRouter`. There are two ways to accomplish this:

#### Connect the arduino using the serial port
* Compile the `r2I2C_config.h` with `#define USE_SERIAL`
* Make sure `r2I2C_config.h` has `//#define R2_PRINT_DEBUG` commented out

#### Connect using the I2C port
* Compile the `r2I2C_config.h` with `#define USE_I2C` 

#### Set the node id:
This is necessary for RF24 networks.
* Uncomment `saveNodeId(n)` in the `r2I2CDeviceRouter.ino` and burn. `n` is the requested id for the node (0-6)
* Make sure to _comment out_ `saveNodeId(n)`. Burn again. The node id is stored in the EEPROM
* There are other ways to handle this, but I dont't remember how.

#### Use the RF24 interface
An Arduino can be used as a standalone RF24 node in a mesh network. This allows a _r2I2CDeviceRouter_ connection to be extended by up to 6 remote Arduinos.
* This requires a _master node_ to be configured with node id _0_ and `#define USE_RH24` _and_ to be configured either as serial _or_ i2c as described above.
* Each _slave_ node needs to be configured with `#define USE_RH24` and a _node id_ that is 1-6.
* The port configuration for the RF24 transceiver is configured in `r2I2C_config.h`

See the detailed examples on how to use an array of the RF24 network (there are none).

#### Here's a dummy example of how the configuration for RF24 setup could look like in Python
It will require a Raspberry Pi and an I2C connection, but it could as well have been any computer using a serial connection.

```
		# Create the factory, that allows us to create the connections
		gpio_factory_factory = self.device_manager.Get(self.settings.I.IoFactory())
		# This will create an I2C connection on bus 1 and port 4 (default for Raspberry Pi)
		# Use gpio_factory_factory.CreateSerialConnection("serial_host", "/dev/ttyACM") instead on a linux machine to connect through the serial port instead.
		host = gpio_factory_factory.CreateSerialConnection("serial_host",1,4)
		# Establish connection to the I2C port
		host.Start()
		# You might want to access the host at some point.
		self.device_manager.Add(host)
		# Initialize the master node
		host.Initialize(0) #make sure master node is reset
		# Initialize a slave node with id 5
		host.Initialize(5) #make sure node 5 is reset

		# Create a remote analog I/O-port on the Arduino port 8 (on node with id 5) 
		remote_io_port = factory.CreateAnalogOutput("remote_io_port", 8, 5)
		self.device_manager.Add(remote_io_port)

```
