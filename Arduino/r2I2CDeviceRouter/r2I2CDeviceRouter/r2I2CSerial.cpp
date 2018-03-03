#include "r2I2CSerial.h"
#include "r2Common.h"

#ifdef USE_SERIAL

// Contains data during serial read.
byte readBuffer[sizeof(RequestPackage) + 1];

// read input size for serial communication.
int rs = 0;

// read input counter for serial communication.
int rx = 0;

// Flag telling that the header ("checksum") was received.
bool headerReceived = false;

// Header read counter
int rh = 0;

// Response/Request header
byte messageHeader[] = PACKAGE_HEADER_IDENTIFIER;

// Size of header
int headerLength = (sizeof(messageHeader)/sizeof(messageHeader[0]));
 
// Reset the read flags and writes "out" to serial.
void writeResponse(ResponsePackage out);

void loop_serial() {
  
  if (Serial.available() > 0) {
  
    if (!headerReceived) {
      
      rs = rx = 0;
      
      if ((byte) Serial.read() == messageHeader[rh]) {
       
        // if the header is ok, we want the next byte
        rh++;
       
       if (rh == headerLength) {
        
         rh = 0;
         headerReceived = true;
        
       }
       
      } else {
        
        rh = 0;
        
      }
      
    } else if (rs == 0) {
      
      rx = 0;
      rs = Serial.read();
      
       if (rs < MIN_REQUEST_SIZE || rs > sizeof(RequestPackage)) {
    
        err("E: size", ERROR_INVALID_REQUEST_PACKAGE_SIZE, rs);
        ResponsePackage out = createErrorPackage(0x0);
        writeResponse(out);
        
       }
       
    } else if (rx < rs - 1) {
      
      readBuffer[rx++] = Serial.read();
      
    } else if (rx == rs - 1) {
      
      readBuffer[rx] = Serial.read();
      
      ResponsePackage out = execute((RequestPackage*)readBuffer); 
      
      writeResponse(out);
      
    }
    
  }

}

void writeResponse(ResponsePackage out) {
  
  headerReceived = false;
  
  rs = rx = rh = 0;

  byte *outputBuffer = (byte *)&out;
  int outputSize = RESPONSE_PACKAGE_SIZE(out);
  
  // Header
  for (byte i = 0; i < headerLength; i++) {Serial.write(messageHeader[i]); } 
  
  // Size of response
  Serial.write((byte) outputSize);
  
  // Content
  for (int i = 0; i < outputSize; i++) { Serial.write(outputBuffer[i]); }

}
#endif
