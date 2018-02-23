#include "r2I2CSerial.h"
#include "r2Common.h"

#ifdef USE_SERIAL
// Contains data during serial read.
byte readBuffer[MAX_RECEIZE_SIZE + 1];

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

bool apa = false;

void loop_serial() {
  
  if (Serial.available() > 0) {
  
    if (!headerReceived) {
      //setStatus(apa); apa = !apa; delay(100);
      
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
    
      //setStatus(apa); apa = !apa; delay(100);
      rx = 0;
      rs = Serial.read();
      
    } else if (rx < rs - 1) {
      
      //setStatus(apa); apa = !apa; delay(100);
      
      readBuffer[rx++] = Serial.read();
      
    } else if (rx == rs - 1) {
      
      //setStatus(apa); apa = !apa; delay(100);
      
      readBuffer[rx] = Serial.read();
      
      headerReceived = false;
      
      rs = rx = rh = 0;
      
      ResponsePackage out = execute((RequestPackage*)readBuffer); 
      
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
#endif
