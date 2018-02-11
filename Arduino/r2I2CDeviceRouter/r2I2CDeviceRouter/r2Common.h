#ifndef R2I2C_COMMON_H
#define R2I2C_COMMON_H

#include "Arduino.h"

// -- Debuging

#ifdef PRINT_ERRORS_AND_FUCK_UP_SERIAL_COMMUNICATION

#define R2_LOG(msg) (Serial.println(msg))

#else

#define R2_LOG(msg) (r2_debug(&msg))

#endif

// Set the status led if defined by R2_STATUS_LED
void setStatus(bool on);

// Set the error led if defined by R2_ERROR_LED
void setError(bool on);

// -- Various "helper" methods.

void r2_debug(const void *msg);

// Returns the node id used (will default to DEVICE_HOST_LOCAL if not stored). 
byte getNodeId();

// Store the node id permanently.
void saveNodeId(byte id);

// Converts a big endian 16-bit int contained in a byte array.
int toInt16(byte *bytes);

// 16-bit-Int-to-byte-array-conversions (Don't forget to free the result!)
byte *asInt16(int value);

// Set error state with a message.
void err (const char* msg, int code);

// Returns the current error message.
const char* getErrorMessage();

// Returns the latest error code
byte getErrorCode();

// Returns true if an error has occured since the last clearError
bool isError();

// Clears any error data
void clearError();

#endif
