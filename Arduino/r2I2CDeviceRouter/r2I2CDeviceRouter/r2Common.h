#ifndef R2I2C_COMMON_H
#define R2I2C_COMMON_H

#include "Arduino.h"

// -- Debuging

#ifdef R2_PRINT_DEBUG

#define R2_LOG(msg) (Serial.println(msg))

#else

#define R2_LOG(msg) ;

#endif

//Restart
static void(* arghhhh) (void) = 0;

// Set the status led if defined by R2_STATUS_LED
void setStatus(bool on);

// Set the error led if defined by R2_ERROR_LED
void setError(bool on);

// -- Runloop methods

void loop_common();

// -- Various "helper" methods --

// Returns the node id used (will default to DEVICE_HOST_LOCAL if not stored). 
byte getNodeId();

// Store the node id permanently.
void saveNodeId(HOST_ADDRESS id);

// Resets (erasing all devices) and sets the initialized state to 'isInitialized'. If isInitialized is false, any master have to re-create all devices on this host. 
void reset(bool isInitialized);

// Return the initialization state of this node.
bool isInitialized();

// Converts a big endian 16-bit int contained in a byte array.
int toInt16(byte *bytes);

// 16-bit-Int-to-byte-array-conversions (Don't forget to free the result!)
byte *asInt16(int value);

// Set error state with a message.
void err (const char* msg, byte code);

// Attach additional error information to a response.
void err (const char* msg, byte code, byte info);

// Uses the previously stored error information to create an error package.
ResponsePackage createErrorPackage(HOST_ADDRESS host);

// Returns the latest error code
byte getErrorCode();

// Returns true if an error has occured since the last clearError
bool isError();

// Clears any error data
void clearError();

// Returns true if a response is an error
bool isError(ResponsePackage response);

// Sets the error state using a response.
void setError(ResponsePackage response);

// Creates a checksum using the specified request package;
byte createRequestChecksum(RequestPackage *package);

// Creates a checksum using the specified response package;
byte createResponseChecksum(ResponsePackage *package);

#endif
