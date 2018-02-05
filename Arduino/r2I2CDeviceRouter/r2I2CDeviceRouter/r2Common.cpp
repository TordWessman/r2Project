#include "r2I2C_config.h"
#include "r2I2CDeviceRouter.h"
#include <EEPROM.h>

// -- Conversions

int toInt16(byte *bytes) { return bytes[0] + (bytes[1] << 8);  }
byte* asInt16(int value) { byte* bytes = (byte *) malloc(2 * sizeof(byte)); bytes[0] = value; bytes[1] = value >> 8; return bytes; }

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
  errCode = 0;
  errMsg = NULL;
}

void err (const char* msg, int code) {

  errCode = code;
  
#ifdef PRINT_ERRORS_AND_FUCK_UP_SERIAL_COMMUNICATION
    if (Serial && msg) { Serial.println(msg); }
#endif

    errMsg = msg;
    
}

// This is my ID
byte nodeId = DEVICE_HOST_LOCAL;

byte getNodeId() {
  
  return EEPROM.read(NODE_ID_EEPROM_ADDRESS);

}

void saveNodeId(byte id) {

  EEPROM.write(NODE_ID_EEPROM_ADDRESS, id);
  
}




