This project is the bridge between r2Project and Arduino hardware.

### Fix dependencies
* Find your Arduino library folder (i.e. _~/Arduino/libraries/_).
* Add _../../r2I2C_ to your Arduino library folder.
* Add the RF24 libraries _(../../3rdParty/RF24-Libraries/*)_ to your Arduino library folder.
* Add the DHT11 library _(../../3rdParty/DHT11)_ to your Arduino library folder.
* restart Arduino Studio.


### Create your local configuration file
`$ cp r2I2C_config.h.template r2I2C_config.h`