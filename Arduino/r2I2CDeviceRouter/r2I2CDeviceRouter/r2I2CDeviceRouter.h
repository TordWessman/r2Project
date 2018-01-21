#include <stdbool.h>
#include <Arduino.h>

#ifndef R2I2C_DEVICE_ROUTER_H
#define R2I2C_DEVICE_ROUTER_H

typedef byte DEVICE_TYPE;

typedef struct Devices {

  // Unique id of the perephial (device).
  byte id;
  
  // Type of the device (i e DEVICE_TYPE_DIGITAL_INPUT).
  DEVICE_TYPE type;
  
  // What I/O ports is used for the device. (max 4)
  byte IOPorts[4];
  
  // The host containing this device (DEVICE_HOST_LOCAL if not remote).
  byte host;
  
  // Object specific data.
  void* object;
  
} Device;

#define DEVICE_HOST_LOCAL 0xFF

// Available device types
#define DEVICE_TYPE_UNDEFINED 0
#define DEVICE_TYPE_DIGITAL_INPUT 1
#define DEVICE_TYPE_DIGITAL_OUTPUT 2
#define DEVICE_TYPE_ANALOGUE_INPUT 3
#define DEVICE_TYPE_SERVO 4
#define DEVICE_TYPE_HCSR04_SONAR 5
#define DEVICE_TYPE_DHT11 6
// Used by responses to indicate an error.
#define DEVICE_TYPE_ERROR 250

// Maximum number of deviceses
#define MAX_DEVICES 20

// No device with the specified id was found.
#define ERROR_CODE_NO_DEVICE_FOUND 1
// The port you're trying to assign is in use.
#define ERROR_CODE_PORT_IN_USE 2
// The device type specified was not declared
#define ERROR_CODE_DEVICE_TYPE_NOT_FOUND 3
// Too many devices has been allocated
#define ERROR_CODE_MAX_DEVICES_IN_USE 4
// This device does not suport set operation
#define ERROR_CODE_DEVICE_TYPE_NOT_FOUND_SET_DEVICE 5
// This device does not support read operation
#define ERROR_CODE_DEVICE_TYPE_NOT_FOUND_READ_DEVICE 6
// Unknown action received
#define ERROR_CODE_UNKNOWN_ACTION 7

// Removes a device from list and free resources
void deleteDevice(byte id);

// Creates and stores a Device using the specified parameters.
bool createDevice(byte id, DEVICE_TYPE type, byte* input);

// Tries to reserve the IO-Port. if reserved: return false and sets the error flag.
bool reservePort(byte IOPort);

// Returns a pointer to a device with the specified id
Device* getDevice(byte id);

// Tries to set the value of a device
bool setValue(Device* device, int value);

// Returns the value(s) of a device
int* getValue(Device* device);

// REQUEST PARSING: ---------------------------

// Maximum buffer for input packages
#define MAX_RECEIZE_SIZE 100

// The maximum size (in bytes) for package content;
#define MAX_CONTENT_SIZE 100

// The base size of response packages (without additional output stored in "content").
#define PACKAGE_SIZE sizeof(ResponsePackage) - MAX_CONTENT_SIZE

// Package "checksum" headers. Shuld initially be sent in the beginning of every transaction. 
#define PACKAGE_HEADER_IDENTIFIER {0xF0, 0x0F, 0xF1}

// Type of action to invoke durng requests.
typedef byte ACTION_TYPE;

// Just to communicate that something went wrong.
#define ACTION_ERROR 0xF0
// The slave has been restarted and requires reinitialization.
#define ACTION_REQUIRE_INITIALIZATION 0xF0

// Normal actions.
#define ACTION_CREATE_DEVICE 0x1
#define ACTION_SET_DEVICE 0x2
#define ACTION_GET_DEVICE 0x3
#define ACTION_INITIALIZE 0x4
#define ACTION_INITIALIZATION_OK 0x5

// Used by the create request.
#define REQUEST_ARG_CREATE_TYPE_POSITION 0x0 // Position of the type to create.
#define REQUEST_ARG_CREATE_PORT_POSITION 0x1 // Position of port information.

// Telling which position in the argument byte array for REQUEST_ARG_CREATE_PORT_POSITION the HC-SR04 will use as trigger port/echo port.
#define HCSR04_SONAR_TRIG_PORT 0x0
#define HCSR04_SONAR_ECHO_PORT 0x1
#define HCSR04_SONAR_DISTANCE_DENOMIATOR 58 // I found this number somewhere

// Here be response positions for DHT11
#define DHT11_TEMPERATUR_RESPONSE_POSITION 0x0;
#define DHT11_HUMIDITY_RESPONSE_POSITION 0x1;

// Number of arguments to return in response
#define RESPONSE_VALUE_COUNT 2

// The size of the response on ACTION_GET_DEVICE requests
#define RESPONSE_VALUE_CONTENT_SIZE (sizeof(int) * RESPONSE_VALUE_COUNT)

// Containing data which should be returned to host after a request.
struct ResponsePackage {

    byte host;
    ACTION_TYPE action;
    byte id;
    byte contentSize;
    byte content[MAX_CONTENT_SIZE];
    
} __attribute__((__packed__));

typedef struct ResponsePackage ResponsePackage;

// Request data from host.
struct RequestPackage {

    byte host;
    ACTION_TYPE action;
    byte id;
    byte args[MAX_CONTENT_SIZE];
    
} __attribute__((__packed__));

typedef struct RequestPackage RequestPackage;

// -- Various "helper" methods.

// Converts a big endian 16-bit int contained in a byte array.
int toInt16(byte *bytes);

// 16-bit-Int-to-byte-array-conversions (Don't forget to free the result!)
byte *asInt16(int value);

// Set error state with a message.
void err (const char* msg, int code);

// Returns the current error message.
const char* getError();

#endif
