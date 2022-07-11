#ifndef R2I2C_SERIAL_H
#define R2I2C_SERIAL_H

#include "r2I2CDeviceRouter.h"

#ifdef USE_SERIAL

  // Package headers. Shuld initially be sent in the beginning of every serial transaction (in order to filter out noise).
  #define PACKAGE_HEADER_IDENTIFIER {0xF0, 0x0F, 0xF1}
  
  // Handles the serial read/write operations.
  void loop_serial();
  
#endif

#ifdef USE_I2C

  // Configure the device for i2c.
  void i2cSetup();

  // Handle i2c traffic.
  void loop_i2c();
  
#endif

#endif
