#ifndef R2I2C_RH24_H
#define R2I2C_RH24_H

#include "r2I2CDeviceRouter.h"

// Send a package!
ResponsePackage rh24Send(RequestPackage* request);

// Initializes RH24 communication. Should be called in setup().
void rh24Setup();

// Used in the run loop. Checks for incomming data.
void rh24Communicate();

// Returns true if the specified host is connected
bool nodeAvailable(HOST_ADDRESS nodeId);

// Number of connected nodes.
int nodeCount();

// Returns an array of HOST_ADDRESS containing all connected nodes
HOST_ADDRESS* getNodes();

// Returns true if this node is the router (master) node.
bool isMaster();

#endif

