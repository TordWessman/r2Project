#ifndef R2I2C_CONFIG_H
#define R2I2C_CONFIG_H

// If defined, I2C communication will be enabled
//#define USE_I2C

// If defined, serial communication will be enabled 
#define USE_SERIAL

// If defined, RF24 mesh networking will be enabled
#define USE_RH24

#ifdef USE_RH24

  // The timeout for a slave before it gives up it's renewal atempt
  #define RH24_SLAVE_RENEWAL_TIMEOUT 3000
  
  #define RH24_PORT1 9
  #define RH24_PORT2 10
  
#endif

#define SERIAL_BAUD_RATE 9600
//115200

// Maximum number of devices
#define MAX_DEVICES 20

// Use with caution. If defined, serial communication (USE_SERIAL)
#define R2_PRINT_DEBUG

// The address position used to store the node id 
#define NODE_ID_EEPROM_ADDRESS 0x0

// If defined, these leds will be used to communicate status and error 
//#define R2_STATUS_LED 0x2
//#define R2_ERROR_LED 0x4

#endif