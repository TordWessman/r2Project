#ifndef R2I2C_RH24_H
#define R2I2C_RH24_H

#include "r2I2CDeviceRouter.h"

// Send a package!
ResponsePackage rh24Send(RequestPackage* request);

// Initializes RH24 communication. Should be called in setup().
void rh24Setup();

// Used in the run loop. Checks for incomming data.
void rh24Communicate();

#endif

