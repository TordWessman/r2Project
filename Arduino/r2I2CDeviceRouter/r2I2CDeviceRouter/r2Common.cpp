#include "r2I2C_config.h"
#include "r2I2CDeviceRouter.h"
#include <EEPROM.h>
#include "r2Common.h"
#include "r2I2C_config.h"

#ifdef R2_PRINT_DEBUG
  #ifdef USE_SERIAL
     #error USE_SERIAL is not compatible with the R2_PRINT_DEBUG flag. Check r2I2C_config.h.
  #endif
#endif
// If true, the host is ready for operation. This flag is set after ACTION_INITIALIZE has been received.
bool initialized = false;

void reset(bool isInitialized) {
          
    R2_LOG(F("Initializing"));

    initialized = isInitialized;
    clearError();
    
    for(byte i = 0; i < MAX_DEVICES; i++) { deleteDevice(i); }
          
}

bool isInitialized() { return initialized; }

// -- Conversions --

int toInt16(byte *bytes) { return bytes[0] + (bytes[1] << 8);  }
byte* asInt16(int value) { byte* bytes = (byte *) malloc(2 * sizeof(byte)); bytes[0] = value; bytes[1] = value >> 8; return bytes; }

// -- Error handling --

byte errCode = 0;
byte errInfo = 0;

byte getErrorCode() { return errCode; }

bool isError() { return errCode != 0; }

void clearError() {
  
  setError(false);
  errCode = 0;
  errInfo = 0;
  
}

void err(const char* msg, byte code) { err(msg, code, 0); }

void err(const char* msg, byte code, byte info) {

  errInfo = info;  
  errCode = code;
  setError(true);
  
#ifdef R2_PRINT_DEBUG
    if(Serial && msg) { 
      
      R2_LOG(msg);
      
      if(info != 0) {
      
          R2_LOG(F("Info:"));
          R2_LOG(info);
          
      }
      
    }
#endif
    
}

bool isError(ResponsePackage response) { return response.action == ACTION_ERROR; }

void setError(ResponsePackage response) {

   err("E: from node", response.content[RESPONSE_POSITION_ERROR_TYPE], response.content[RESPONSE_POSITION_ERROR_INFO]);
   
}

ResponsePackage createErrorPackage(HOST_ADDRESS host) {

  ResponsePackage response;
  
  response.messageId = 0x0;
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

HOST_ADDRESS getNodeId() { return EEPROM.read(NODE_ID_EEPROM_ADDRESS); }

void saveNodeId(HOST_ADDRESS id) { EEPROM.write(NODE_ID_EEPROM_ADDRESS, id); }

byte createRequestChecksum(RequestPackage *package) {

  byte checksum = 0;
  
  for(int i = 1; i < requestPackageSize(package); i++) {
  
    checksum += ((byte *)package)[i];
    
  }
  
  return checksum;
  
}

byte createResponseChecksum(ResponsePackage *package) {

  byte checksum = 0;
  
  // Omit MessageId when calculating checksum
  for(int i = 2; i < responsePackageSize(package); i++) {
  
    checksum += ((byte *)package)[i];
    
  }
  
  return checksum;
  
}

#ifdef R2_STATUS_LED
unsigned long blinkTimer = 0;
bool blinkOn = false;
#define blinkTime (1000/LED_TIME_DENOMIATOR)

void statusIndicator() {
  if(millis() - blinkTimer >= blinkTime) {
    blinkTimer = millis();
    setStatus(blinkOn);
    blinkOn = !blinkOn;
    
  }
}

#endif


#ifdef R2_USE_LED

  void setupLeds() {
  
    #ifdef R2_ERROR_LED
      pinMode(R2_ERROR_LED, OUTPUT);
      reservePort(R2_ERROR_LED);
    #endif
  
    #ifdef R2_STATUS_LED
      pinMode(R2_STATUS_LED, OUTPUT);
      for(int i = 0; i < 5; i++) {
        digitalWrite(R2_STATUS_LED, 1);
        delay(500 / LED_TIME_DENOMIATOR);
        digitalWrite(R2_STATUS_LED, 0);
        delay(500 / LED_TIME_DENOMIATOR);
      }
      reservePort(R2_STATUS_LED);
    #endif
    
  }

#endif
