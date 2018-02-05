#include "r2I2C_config.h"
#include "r2I2CDeviceRouter.h"
#include "r2Common.h"
#include <string.h>

#ifdef USE_SERIAL
  #include "r2I2CSerial.h"
#endif

#ifdef USE_I2C
  #include <Wire.h> // Must be included
  #include <r2I2C.h>
#endif

#ifdef USE_RF24
  #include "r2RH24.h"
#endif


// Keeps track of the available devices.
byte deviceCount = 0;

// If true, the host is ready for operation. This flag is set after ACTION_INITIALIZE has been received.
bool initialized = false;

ResponsePackage interpret(byte* input) {

  RequestPackage *request = (RequestPackage*) input;

  ResponsePackage out = execute(request);
  
  // Reset error state
  clearError();

  return out;
  
}

ResponsePackage execute(RequestPackage *request) {
  
  ResponsePackage response;
   
  response.id = request->id;
  response.action = request->action;
  response.host = request->host;
  response.contentSize = 0;
   
  // If the ACTION_SET_NODE_ID is called, this node will be "configured" with a new id. Intended for setting up nodes only.
  if (request->action == ACTION_SET_NODE_ID) {
      
      response.host = request->id;
      
        // Store the id. This host will now be known as 'request->id'.
      saveNodeId(request->id);
      
      return response;
        
  }
  
  #ifdef USE_RH24
  if (request->host != getNodeId()) {
  
    return rh24Send(request);
    
  }
  #endif

   if (!initialized && request->action != ACTION_INITIALIZE) {
   
     // If initialization wasn't done and if the request was not an initialization request. The remote must initialize prior to any other action.
     response.action = ACTION_INITIALIZE;
     return response;
     
   }
   
  switch(request->action) {
  
    case ACTION_CREATE_DEVICE:
    {
      response.id = deviceCount++;

       byte *parameters = request->args + REQUEST_ARG_CREATE_PORT_POSITION;
   
      createDevice(response.id, request->args[REQUEST_ARG_CREATE_TYPE_POSITION], parameters);
      
    }
     
    break;
      
    case ACTION_SET_DEVICE:
      {
        Device *device = getDevice(request->id);
  
        if (!device) {
          
          err("Device not found when trying to set value", ERROR_CODE_NO_DEVICE_FOUND);
          
        } else {
          
          setValue(device, toInt16 ( request->args ));
        
        }
        
      }
      
      break;
      
      case ACTION_GET_DEVICE:
      {
        Device *device = getDevice(request->id);
  
        if (device) {
      
          response.contentSize = RESPONSE_VALUE_CONTENT_SIZE;
          byte *result = (byte *)getValue(device);
          
          for (int i = 0; i < response.contentSize; i++) { response.content[i] = result[i]; }
          
          free (result);
          
        } else {
        
          err("Device not found when trying to get value", ERROR_CODE_NO_DEVICE_FOUND);
          
        }
        
      }
      break;
      case ACTION_INITIALIZE:
      
        initialized = true;
        clearError();
        deviceCount = 0;
        
        for (int i = 0; i < MAX_DEVICES; i++) { void deleteDevice(byte i); }

        response.action = ACTION_INITIALIZATION_OK;
        return response;
        
      break;
    
     default:
     
        err("Unknown action sent to device router.", ERROR_CODE_UNKNOWN_ACTION);
      
  }
  
    if (isError()) {
  
      response.contentSize = strlen(getErrorMessage()) > MAX_CONTENT_SIZE ? MAX_CONTENT_SIZE : strlen(getErrorMessage());
      response.id = getErrorCode();
    
      for (int i = 0; i < response.contentSize; i++) { response.content[i] = ((byte*) getErrorMessage())[i]; }
    
    }
  
  return response;
  
}


void setup() {

  clearError();
  
#ifdef USER_RH24
  rh24Setup();
#endif
  
#ifdef USE_SERIAL
  Serial.begin(9600);
#endif

#ifdef USE_I2C
  Serial.begin(9600);
  R2I2C.initialize(DEFAULT_I2C_ADDRESS, i2cReceive);
#endif
 
}

void loop() {
  
  #ifdef USE_SERIAL
    serialCommunicate();
  #endif
  
  #ifdef USE_RH24
    rh24Communicate();
  #endif
  
}

#include "Dht11.h"
#include <Servo.h>
#include <EEPROM.h>
#include "RF24.h"
#include "RF24Network.h"
#include "RF24Mesh.h"
#include <SPI.h>

