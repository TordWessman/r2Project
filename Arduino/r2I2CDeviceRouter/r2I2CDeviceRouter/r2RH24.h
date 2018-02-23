#ifndef R2I2C_RH24_H
#define R2I2C_RH24_H

#include "r2I2CDeviceRouter.h"

// Definitions for the package (Action) containing sleep information
#define SLEEP_MODE_TOGGLE_POSITION 0x0
#define SLEEP_MODE_CYCLES_POSITION 0x1

// Maximum number of nodes in the slave network
#define MAX_NODES 20

// If this amount is reached, the mest.begin() should be called on the slave
#define MAX_RENEWAL_FAILURE_COUNT 5

// How often the slave will try to renew it's address (in ms)
#define RH24_NETWORK_RENEWAL_TIME 1000

// Timeout in ms before a network read operation fails. Should be high, since sleeping nodes in a mesh might have a slow response time 
#define RH24_READ_TIMEOUT 15000

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

#endif

