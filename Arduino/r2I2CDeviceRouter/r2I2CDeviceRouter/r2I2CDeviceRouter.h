#ifndef R2I2C_DEVICE_ROUTER_H
#define R2I2C_DEVICE_ROUTER_H

#include <Arduino.h>
#include "r2I2C_config.h"
#include <stdbool.h>

// -- Public properties --

// Keeps track of the devices (and thus their id's).
static byte deviceCount = 0;

// -- Configurations

// The maximum number of ports a device can occupy
#define DEVICE_MAX_PORTS 2

// The id for the mastel node.
#define DEVICE_HOST_LOCAL 0x0

// Maximum buffer for input packages
#define MAX_RECEIVE_SIZE 20

// The maximum size (in bytes) for package content;
#define MAX_CONTENT_SIZE (MAX_DEVICES * 2 + 1)

// -- Type definitions --

// Return value type for getDevice is always 16-bit int.
#define r2Int uint16_t

// The type of a device (being created)
typedef byte DEVICE_TYPE;

// The address to a slave node
typedef byte HOST_ADDRESS;

// Type of action to invoke durng requests.
typedef byte ACTION_TYPE;

typedef struct Devices {

  // Unique id of the perephial (device).
  byte id;
  
  // Type of the device (i e DEVICE_TYPE_DIGITAL_INPUT).
  DEVICE_TYPE type;
  
  // What I/O ports is used for the device. (max DEVICE_MAX_PORTS)
  byte IOPorts[DEVICE_MAX_PORTS];
  
  // Object specific data.
  void* object;
  
} Device;

// -- Package definitions --

// Containing data which should be returned to host after a request.
struct ResponsePackage {

    byte checksum;
    byte messageId;
    HOST_ADDRESS host;
    ACTION_TYPE action;
    byte id;
    byte contentSize;
    byte content[MAX_CONTENT_SIZE];
    
} __attribute__((__packed__));

typedef struct ResponsePackage ResponsePackage;

// Request data from host.
struct RequestPackage {

    byte checksum;
    HOST_ADDRESS host;
    ACTION_TYPE action;
    byte id;
    byte argSize;
    byte args[MAX_CONTENT_SIZE];
    
} __attribute__((__packed__));

#define requestPackageSize(package) (5 + (package)->argSize)
#define responsePackageSize(package) (6 + (package)->contentSize)

#define MIN_REQUEST_SIZE (sizeof(RequestPackage) - MAX_CONTENT_SIZE)
#define MAX_REQUEST_SIZE (sizeof(RequestPackage) + MAX_CONTENT_SIZE)

typedef struct RequestPackage RequestPackage;

// -- Device types --

// Available device types
#define DEVICE_TYPE_UNDEFINED 0
#define DEVICE_TYPE_DIGITAL_INPUT 1
#define DEVICE_TYPE_DIGITAL_OUTPUT 2
#define DEVICE_TYPE_ANALOGUE_INPUT 3
#define DEVICE_TYPE_SERVO 4
#define DEVICE_TYPE_HCSR04_SONAR 5
#define DEVICE_TYPE_DHT11 6
#define DEVICE_TYPE_SIMPLE_MOIST 7
#define DEVICE_TYPE_ANALOGE_OUTPUT 8
#define DEVICE_TYPE_SONAR 9
// -- Error codes --

// No device with the specified id was found.
#define ERROR_CODE_NO_DEVICE_FOUND 1
// The port you're trying to assign is in use.
#define ERROR_CODE_PORT_IN_USE 2
// The device type specified was not declared.
#define ERROR_CODE_DEVICE_TYPE_NOT_FOUND 3
// Too many devices has been allocated.
#define ERROR_CODE_MAX_DEVICES_IN_USE 4
// This device does not suport set operation.
#define ERROR_CODE_DEVICE_TYPE_NOT_FOUND_SET_DEVICE 5
// This device does not support read operation.
#define ERROR_CODE_DEVICE_TYPE_NOT_FOUND_READ_DEVICE 6
// Unknown action received.
#define ERROR_CODE_UNKNOWN_ACTION 7
// DHT11 read error (is it connected).
#define ERROR_CODE_DHT11_READ_ERROR 8
// The node (during remote communication) was not available.
#define ERROR_RH24_NODE_NOT_AVAILABLE 9
// Whenever a read did not return the expected size.
#define ERROR_RH24_BAD_SIZE_READ 10
// Called if a read-write timed out during RH24 communication.
#define ERROR_RH24_TIMEOUT 11
// If a write operation fails for RH24.
#define ERROR_RH24_WRITE_ERROR 12
// Unknown message. Returned if the host receives a message with an unexpected type.
#define ERROR_RH24_UNKNOWN_MESSAGE_TYPE_ERROR 13
// If the the master's read operation was unable to retrieve data.
#define ERROR_RH24_NO_NETWORK_DATA 14
// Routing through a node that is not the master is not supported.
#define ERROR_RH24_ROUTING_THROUGH_NON_MASTER 15
// If the master receives two messages with the same id.
#define ERROR_RH24_DUPLICATE_MESSAGES 16
// Internal error: Serial read timed out.
#define ERROR_SERIAL_TIMEOUT 17
// Failed to make the node sleep.
#define ERROR_FAILED_TO_SLEEP 18
// Messages are not in sync. Unrecieved messages found in the masters input buffer.
#define ERROR_RH24_MESSAGE_SYNCHRONIZATION 19
// If the size of the incomming data is invalid.
#define ERROR_INVALID_REQUEST_PACKAGE_SIZE 20
// If the checksum in a request did not match the rest of the request data.
#define ERROR_BAD_CHECKSUM 21
// TCP read or write timed out.
#define ERROR_TCP_TIMEOUT 22
// TCP read failed.
#define ERROR_TCP_READ 23

// Error reserved for external purposes
#define ERROR_RESERVED_1 0xF0
#define ERROR_RESERVED_2 0xF1
#define ERROR_RESERVED_3 0xF2
#define ERROR_RESERVED_4 0xF3
#define ERROR_RESERVED_5 0xF4
#define ERROR_RESERVED_6 0xF5
#define ERROR_RESERVED_7 0xF6
#define ERROR_RESERVED_8 0xF7
// -- Response Actions --

// Just to communicate that something went wrong.
#define ACTION_ERROR 0xF0
// The slave has been restarted and requires reinitialization.
#define ACTION_REQUIRE_INITIALIZATION 0xF0

// -- Request Actions --

// If no action has been specified. Must be an error...
#define ACTION_UNDEFINED 0x0
// Creates a device
#define ACTION_CREATE_DEVICE 0x1
// Sets the value of a device
#define ACTION_SET_DEVICE 0x2
// Returns the value of a device
#define ACTION_GET_DEVICE 0x3
// Re-set (initialize) the controller
#define ACTION_INITIALIZE 0x4
// Return action message indicating that the initialization was successful
#define ACTION_INITIALIZATION_OK 0x5
// Set nodeId for the controller. This method will override any network communication
#define ACTION_SET_NODE_ID 0x6
// To check if the node is connected
#define ACTION_CHECK_NODE 0x7
// To retrieve a list of nodes
#define ACTION_GET_NODES 0x8
// Internally used by RH24 to distinguish regular messages from ping messages.
#define ACTION_RH24_PING 0x9
// Sends this node to sleep according to configuration
#define ACTION_SEND_TO_SLEEP 0x0A
// To find out if a is in sleep mode
#define ACTION_CHECK_SLEEP_STATE 0x0B
// Pauses the sleep state of a node. Use this to minimize EEPROM writings if multiple requests are made to this node.
#define ACTION_PAUSE_SLEEP 0x0C
// Will reset this node to a state where it's no longer is initialized
#define ACTION_RESET 0x0D
// Returns a checksum of configuration and devices.
#define ACTION_CHECK_INTEGRITY 0x0E

// -- Internal Actions --

// Internal action definition. Used to check that there were no unread message in the pipe.
#define ACTION_RH24_NO_MESSAGE_READ 0xF1
// Ping message from master node to slave in order to find out if the slave is available
#define ACTION_RH24_PING_SLAVE 0xF2

// -- Request & response parameter definitions

// Number of 16-bit arguments to return in response
#define RESPONSE_VALUE_COUNT 2

// Used by the create request.
#define REQUEST_ARG_CREATE_TYPE_POSITION 0x0 // Position of the type to create.
#define REQUEST_ARG_CREATE_PORT_POSITION 0x1 // Position of port information.

// Telling which position in the argument byte array for REQUEST_ARG_CREATE_PORT_POSITION the HC-SR04 will use as trigger port/echo port.
#define SONAR_TRIG_PORT 0x0
#define SONAR_ECHO_PORT 0x1
#define SONAR_MAX_DISTANCE 0x3 // The maximum distance for a "regular" sonar
#define HCSR04_SONAR_DISTANCE_DENOMIATOR 58 // I found this number somewhere

// Port positions for simple moisture sensors
#define SIMPLE_MOIST_ANALOGUE_IN 0x0 // The analogue input port used for the actual reading.
#define SIMPLE_MOIST_DIGITAL_OUT 0x1 // The digital output port used to enable reading.

// -- Response positions for ResponsePackage.content --

// Here be response positions for DHT11
#define RESPONSE_POSITION_DHT11_TEMPERATURE 0x0
#define RESPONSE_POSITION_DHT11_HUMIDITY 0x1

// The response part containing the error code
#define RESPONSE_POSITION_ERROR_TYPE 0x0
// Might contain additional error information
#define RESPONSE_POSITION_ERROR_INFO 0x1

// The response position containing host availability information
#define RESPONSE_POSITION_HOST_AVAILABLE 0x0

// -- Public methods and macros--

// The size of the response on ACTION_GET_DEVICE requests
#define RESPONSE_VALUE_CONTENT_SIZE (sizeof(int) * RESPONSE_VALUE_COUNT)

// Decides what to do with the incoming data.
ResponsePackage interpret(byte* input);

// The base size of response packages (without additional output stored in "content").
#define RESPONSE_PACKAGE_SIZE(responsePackage) (sizeof(ResponsePackage) - MAX_CONTENT_SIZE + responsePackage.contentSize)

// Removes a device from list and free resources
void deleteDevice(byte id);

// Creates and stores a Device using the specified parameters. Returns true if successful
bool createDevice(byte id, DEVICE_TYPE type, byte* input);

// Tries to reserve the IO-Port. if reserved: return false and sets the error flag if port was already reserved.
bool reservePort(byte IOPort);

// Returns a pointer to a device with the specified id
Device* getDevice(byte id);

// Tries to set the value of a device. Returns true if successfull
void setValue(Device* device, int value);

// Returns the value(s) of a device
r2Int* getValue(Device* device);

// Performs the actions requested by the RequestPackage.
ResponsePackage execute(RequestPackage *request);


#endif
