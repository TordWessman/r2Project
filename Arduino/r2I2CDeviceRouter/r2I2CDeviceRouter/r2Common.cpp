#include "r2I2C_config.h"
#include "r2I2CDeviceRouter.h"
#include <EEPROM.h>
#include "r2Common.h"

// -- Conversions

int toInt16(byte *bytes) { return bytes[0] + (bytes[1] << 8);  }
byte* asInt16(int value) { byte* bytes = (byte *) malloc(2 * sizeof(byte)); bytes[0] = value; bytes[1] = value >> 8; return bytes; }

void r2_debug(const void *msg) { }

// Error handling

const char* errMsg = NULL;
byte errCode = 0;

const char* getErrorMessage() {
    return errMsg;
}

byte getErrorCode() {
    return errCode;
}

bool isError() {
  return errCode != 0;
}

void clearError() {
  setError(false);
  errCode = 0;
  errMsg = NULL;
}

void err (const char* msg, int code) {

  errCode = code;
  setError(true);
#ifdef PRINT_ERRORS_AND_FUCK_UP_SERIAL_COMMUNICATION
    if (Serial && msg) { R2_LOG(msg); }
#endif

    errMsg = msg;
    
}

void setStatus(bool on) {

#ifdef R2_STATUS_LED
  digitalWrite(R2_ERROR_LED, on ? 1 : 0); 
#endif

}

void setError(bool on) {

#ifdef R2_ERROR_LED
  digitalWrite(R2_ERROR_LED, on ? 1 : 0); 
#endif

}

// This is my ID
HOST_ADDRESS nodeIdXYZ = DEVICE_HOST_LOCAL;

// Since the EEPROM operations seems to make stuff go haywire, we should not access it to often.
bool hasReadNodeId = false;

HOST_ADDRESS getNodeId() {
  
  if (!hasReadNodeId) {
    
    nodeIdXYZ = EEPROM.read(NODE_ID_EEPROM_ADDRESS);
    hasReadNodeId = true;

  }
  
  return nodeIdXYZ;

}

void saveNodeId(HOST_ADDRESS id) {

  EEPROM.write(NODE_ID_EEPROM_ADDRESS, id);
  nodeIdXYZ = id;
  
}




