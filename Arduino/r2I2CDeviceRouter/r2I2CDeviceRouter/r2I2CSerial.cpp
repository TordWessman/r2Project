#include "r2I2CSerial.h"

// Contains data during serial read.
byte readBuffer[MAX_RECEIZE_SIZE];

// read input size for serial communication.
int rs = 0;

// read input counter for serial communication.
int rx = 0;

// Flag telling that the header ("checksum") was received.
bool headerReceived = false;

// Header read counter
int rh;

// Response/Request header
byte messageHeader[] = PACKAGE_HEADER_IDENTIFIER;

// Size of header
int headerLength = (sizeof(messageHeader)/sizeof(messageHeader[0]));

void serialCommunicate() {
  
  if (Serial.available() > 0) {

    if (!headerReceived) {
      
      rs = rx = 0;
      
      if ((byte) Serial.read() == messageHeader[rh]) {
       
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
      
    } else if (rx < rs - 1) {
    
      readBuffer[rx++] = Serial.read();
      
    } else if (rx == rs - 1) {
  
      readBuffer[rx] = Serial.read();
      
      headerReceived = false;
      
      rs = rx = 0;
      
      ResponsePackage out = interpret(readBuffer); 
      byte *outputBuffer = (byte *)&out;
      int outputSize = RESPONSE_PACKAGE_SIZE(out);
  
      // Header
      for (byte i = 0; i < headerLength; i++) {Serial.write(messageHeader[i]); } 
      
      // Size of response
      Serial.write((byte) outputSize);
      
      // Content
      for (int i = 0; i < outputSize; i++) { Serial.write(outputBuffer[i]); }
      
    }
    
  }
}

