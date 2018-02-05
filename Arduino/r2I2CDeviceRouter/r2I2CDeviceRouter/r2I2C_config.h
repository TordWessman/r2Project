#ifndef R2I2C_CONFIG_H
#define R2I2C_CONFIG_H

// If defined, I2C communication will be enabled
#define USE_I2C

// If defined, serial communication will be enabled 
#define USE_SERIAL

// If defined, RF24 mesh networking will be enabled
#define USE_RF24

#ifdef USE_RF24

  // The timeout for a slave before it gives up it's renewal atempt
  #define RH24_SLAVE_RENEWAL_TIMEOUT 3000
  
  #define RF24_PORT1 9
  #define RF24_PORT2 10

  // Number of delays before a write action causes a timeout  
  #define RH24_MAX_RESPONSE_TIMEOUT_COUNT 50
  
  // Delay time for a read after a write
  #define RH24_RESPONSE_DELAY 10
  
#endif

// Maximum number of devices
#define MAX_DEVICES 20

// Use with caution. If defined, serial errors will be printed through serial port.
//#define PRINT_ERRORS_AND_FUCK_UP_SERIAL_COMMUNICATION

// The address position used to store the node id 
#define NODE_ID_EEPROM_ADDRESS 0x0

#endif
