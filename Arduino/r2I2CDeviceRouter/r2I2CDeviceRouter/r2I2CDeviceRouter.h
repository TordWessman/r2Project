#include <stdbool.h>

// If defined, serial communication (not I2C) will be used for master/slave communication. 
#define USE_SERIAL

typedef byte DEVICE_TYPE;

typedef struct Devices {

  // Unique id of the perephial (device).
  byte id;
  
  // Type of the device (i e DEVICE_TYPE_DIGITAL_INPUT).
  DEVICE_TYPE type;
  
  // What I/O port is used for the device.
  byte IOPort;
  
  // The host containing this device (DEVICE_HOST_LOCAL if not remote).
  byte host;
  
  // Object specific data.
  void* object;
  
} Device;

#define DEVICE_HOST_LOCAL 0xFF

// Available device types.
#define DEVICE_TYPE_UNDEFINED 0
#define DEVICE_TYPE_DIGITAL_INPUT 1
#define DEVICE_TYPE_DIGITAL_OUTPUT 2
#define DEVICE_TYPE_ANALOGUE_INPUT 3
#define DEVICE_TYPE_SERVO 4

#define MAX_DEVICES 20

// Creates and stores a Device using the specified parameters.
bool createDevice(byte id, DEVICE_TYPE type, byte IOPort);

// Returns a pointer to a device with the specified id
Device* getDevice(byte id);

// Tries to set the value of a device
bool setValue(Device* device, int value);

// Returns the value of a device
int getValue(Device* device);

// REQUEST PARSING: ---------------------------

// Maximum buffer for input packages
#define MAX_RECEIZE_SIZE 100

// The maximum size (in bytes) for package content;
#define MAX_CONTENT_SIZE 100

// The base size of response packages (without additional output stored in "content").
#define PACKAGE_SIZE sizeof(ResponsePackage) - MAX_CONTENT_SIZE

// Use with caution. If defined, serial errors will be printed through serial port.
//#define PRINT_ERRORS_AND_FUCK_UP_SERIAL_COMMUNICATION

// Package "checksum" headers. Shuld initially be sent in the beginning of every transaction. 
#define PACKAGE_HEADER_IDENTIFIER {0xF0, 0x0F, 0xF1}

// Type of action to invoke durng requests
typedef byte ACTION_TYPE;

// Just to communicate that something went wrong
#define ACTION_ERROR 0x0

// Normal actions.
#define ACTION_CREATE_DEVICE 0x1
#define ACTION_SET_DEVICE 0x2
#define ACTION_GET_DEVICE 0x3

#define REQUEST_ARG_CREATE_TYPE_POSITION 0x0
#define REQUEST_ARG_CREATE_PORT_POSITION 0x1

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

// Performs the actions requested by the request.
ResponsePackage execute(RequestPackage *request);

// Converts a big endian 16-bit int contained in a byte array.
int toInt16(byte *bytes);

// 16-bit-Int-to-byte-array-conversions (Don't forget to free the result!)
byte *asInt16(int value);

