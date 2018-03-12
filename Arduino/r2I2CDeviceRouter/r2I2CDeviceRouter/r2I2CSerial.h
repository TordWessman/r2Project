#ifndef R2I2C_SERIAL_H
#define R2I2C_SERIAL_H

// Package "checksum" headers. Shuld initially be sent in the beginning of every serial transaction. 
#define PACKAGE_HEADER_IDENTIFIER {0xF0, 0x0F, 0xF1}

#include "r2I2CDeviceRouter.h"

// Handles the serial read/write operations. Prefarbly used in run loop.
void loop_serial();

#endif
