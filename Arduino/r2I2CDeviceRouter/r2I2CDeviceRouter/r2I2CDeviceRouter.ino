#ifndef USE_SERIAL
#include <Wire.h> // Must be included
#include <r2I2C.h>
#endif

#include "r2I2CDeviceRouter.h"

#include <Servo.h>
#include <string.h>

// If defined, serial communication (not I2C) will be used for master/slave communication. 
#define USE_SERIAL

// -- Private method declarations

// Decides what to do with the incoming data. Preferably, it should call "execute".
ResponsePackage interpret(byte* input);

// Performs the actions requested by the RequestPackage.
ResponsePackage execute(RequestPackage *request);

// -- Variables

byte readBuffer[MAX_RECEIZE_SIZE];

// read input size
int rs = 0;

// read input counter
int rx = 0;

// Flag telling that the header ("checksum") was received
bool headerReceived = false;
// Header read counter
int rh;

// Response/Request header
byte messageHeader[] = PACKAGE_HEADER_IDENTIFIER;

// Size of header
int headerLength = (sizeof(messageHeader)/sizeof(messageHeader[0]));

ResponsePackage execute(RequestPackage *request) {

  ResponsePackage response;
  
   response.id = request->id;
   response.action = request->action;
   response.host = request->host;
   response.contentSize = 0;
   
  switch(request->action) {
  
    case ACTION_CREATE_DEVICE:
      
      if (!createDevice(request->id, request->args[REQUEST_ARG_CREATE_TYPE_POSITION], &request->args[REQUEST_ARG_CREATE_PORT_POSITION])) {
        
        response.action = ACTION_ERROR;
        
      } 
      
      break;
      
    case ACTION_SET_DEVICE:
      {
        Device *device = getDevice(request->id);
  
        if (!device || !setValue(device, toInt16 ( request->args ))) {
          
          response.action = ACTION_ERROR;
          
        } 
        
      }
      
      break;
      
      case ACTION_GET_DEVICE:
      {
        Device *device = getDevice(request->id);
  
        if (device) {
      
          response.contentSize = sizeof(int);
          byte *result = asInt16(getValue(device));
          
          for (int i = 0; i < response.contentSize; i++) { response.content[i] = result[i]; }
          
          free (result);
          
        } else {
        
          response.action = ACTION_ERROR;
          
        }
        
      }
      break;
    
     default:
     
        err("Unknown action sent to device router.");
      
  }
  
  if (getError()) {
  
    response.action = ACTION_ERROR;
          
  }
  
  return response;
  
}


ResponsePackage interpret(byte* input) {

  RequestPackage *request = (RequestPackage*) input;

  ResponsePackage out = execute(request);

  if (out.action == ACTION_ERROR) {
  
    out.contentSize = strlen(getError());
    
    for (int i = 0; i < out.contentSize; i++) { out.content[i] = ((byte*) getError())[i]; }
    
  }
  
  // Reset error state
  err(NULL);

  return out;
  
}

void setup() {
  
#ifdef USE_SERIAL
  Serial.begin(9600);
#else
  R2I2C.initialize(DEFAULT_I2C_ADDRESS, i2cReceive);
#endif

  err(NULL);

  for (int i = 0; i < MAX_DEVICES; i++) { void deleteDevice(byte i); }
 
}

void loop() {
  
  #ifdef USE_SERIAL
  serialCommunicate();
  #else
	//TODO: Add radio here?
  delay(1000);
  #endif
  
}

#ifndef USE_SERIAL

// Delegate method for I2C event communication.
void i2cReceive(byte* data, int data_size) {

  ResponsePackage out = interpret(data); 
  byte *response = (byte *)&out;
  int responseSize = sizeof(ResponsePackage) - MAX_CONTENT_SIZE + out.contentSize;
  
  R2I2C.setResponse(response, responseSize);
  
}

#else

// Handles the serial read/write operations. Prefarbly used in run loop.
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
      int outputSize = PACKAGE_SIZE + out.contentSize;
  
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
