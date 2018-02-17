#include "r2I2C_config.h"
#include "r2I2CDeviceRouter.h"
#include <EEPROM.h>
#include "r2Common.h"

// -- Conversions

int toInt16(byte *bytes) { return bytes[0] + (bytes[1] << 8);  }
byte* asInt16(int value) { byte* bytes = (byte *) malloc(2 * sizeof(byte)); bytes[0] = value; bytes[1] = value >> 8; return bytes; }

void r2_debug(const void *msg) { }

// Error handling

byte errCode = 0;
byte errInfo = 0;

byte getErrorCode() {
    return errCode;
}

bool isError() {
  return errCode != 0;
}

void clearError() {
  setError(false);
  errCode = 0;
  errInfo = 0;
}

void err (const char* msg, byte code) {

    err(msg, code, 0);
    
}

void err (const char* msg, byte code, byte info) {

  errInfo = info;  
  errCode = code;
  setError(true);
#ifdef R2_PRINT_DEBUG
    if (Serial && msg) { R2_LOG(msg); }
#endif
    
}

ResponsePackage createErrorPackage(HOST_ADDRESS host) {

  ResponsePackage response;
    
  response.host = host;
  response.action = ACTION_ERROR;
  response.id = 42;
  response.contentSize = 2;
  response.content[RESPONSE_POSITION_ERROR_TYPE] = errCode;
  response.content[RESPONSE_POSITION_ERROR_INFO] = errInfo;

  return response;
  
}
void setStatus(bool on) {

#ifdef R2_STATUS_LED
  digitalWrite(R2_STATUS_LED, on ? 1 : 0); 
#endif

}

void setError(bool on) {

#ifdef R2_ERROR_LED
  digitalWrite(R2_ERROR_LED, on ? 1 : 0); 
#endif

}
HOST_ADDRESS getNodeId() {
  
  return EEPROM.read(NODE_ID_EEPROM_ADDRESS);
  
}

void saveNodeId(HOST_ADDRESS id) {

  EEPROM.write(NODE_ID_EEPROM_ADDRESS, id);
  
}




