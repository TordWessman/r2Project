#ifndef R2I2C_RH24_H
#define R2I2C_RH24_H

#include "r2I2CDeviceRouter.h"

// Definitions for the package (Action) containing sleep information
#define SLEEP_MODE_TOGGLE_POSITION 0x0
#define SLEEP_MODE_CYCLES_POSITION 0x1

// Will the slave try to ping the master
//#define RH24_PING_ENABLED

// How often will the slave try to ping the master
#define RH24_PING_INTERVAL 60000

// If this amount is reached, the mest.begin() should be called on the slave
#define MAX_RENEWAL_FAILURE_COUNT 5

// How often the slave will try to renew it's address (in ms)
#define RH24_NETWORK_RENEWAL_TIME 3000

// The timeout for a slave before it gives up it's renewal attempt
#define RH24_SLAVE_RENEWAL_TIMEOUT 5000

// Retry before a slave tries to rewrite a failed message
#define RH24_SLAVE_WRITE_RETRY 500

// Timeout in ms before a network read operation fails. Should be high, since sleeping nodes in a mesh might have a slow response time 
#define RH24_READ_TIMEOUT 15000

// The number of second which is the default interval for pause sleep actions
#define PAUSE_SLEEP_DEFAULT_INTERVAL 30

// Send a package!
ResponsePackage rh24Send(RequestPackage* request);

// Initializes RH24 communication. Should be called in setup().
void rh24Setup();

// Used in the run loop. Checks for incomming data.
void loop_rh24();

// Returns true if the specified host is connected
bool nodeAvailable(HOST_ADDRESS nodeId);

// Number of connected nodes.
int nodeCount();

// Returns an array of HOST_ADDRESS containing all connected nodes
HOST_ADDRESS* getNodes();

// Returns true if this node is the router (master) node.
bool isMaster();

// Sets this nodes state to sleep state (until it's restarted).
void sleep(bool on, byte cycles);

// As with sleep above, but with the default cycles defaulting to RH24_SLEEP_UNTIL_MESSAGE_RECEIVED. The sleep state will be stored on the EEPROM and the node will remain in sleep upon reboot.
void sleep(bool on);

// Will pause the sleep state of a node, allowing access for a limited time period. Currently, the maximum sleep time is 60 seconds.
void pauseSleep(byte seconds);

// As above, but with the default parameter 'PAUSE_SLEEP_DEFAULT_INTERVAL'.
void pauseSleep();

// Returns true if this node is sleeping.NODE_void pauseSleep(byte seconds);

// Returns true if this node is sleeping.NODE_
bool isSleeping();

#endif

