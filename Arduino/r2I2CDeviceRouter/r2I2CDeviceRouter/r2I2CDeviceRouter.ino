#include "r2I2C_config.h"
#include "r2I2CDeviceRouter.h"
#include "r2Common.h"
#include <string.h>
#include "Dht11.h"
#include <Servo.h>
#include <EEPROM.h>
#ifdef RH24
#include "RF24.h"
#include "RF24Network.h"
#include "RF24Mesh.h"
#include <SPI.h>
#endif

#ifdef USE_SERIAL
  #include "r2I2CSerial.h"
#endif

#ifdef USE_I2C
  #include <Wire.h> // Must be included
  #include <r2I2C.h>
#endif

#ifdef USE_RH24
  #include "r2RH24.h"
#endif

// If true, the host is ready for operation. This flag is set after ACTION_INITIALIZE has been received.
bool initialized = false;

// Keeps track of the devices (and thus their id's).
byte deviceCount = 0;

// Counter for messages. For debuging purposes only.
byte messageId = 0;

#ifdef USE_I2C

// Delegate method for I2C event communication.
void i2cReceive(byte* data, int data_size) {

  ResponsePackage out;
  
  if (data_size < MIN_REQUEST_SIZE || data_size > sizeof(RequestPackage)) {
    
    err("E: size", ERROR_INVALID_REQUEST_PACKAGE_SIZE, data_size);
    out = createErrorPackage(0x0);
    
  } else {
  
    out = execute((RequestPackage*)data);
    
  }
  
  R2_LOG(RESPONSE_PACKAGE_SIZE(out));
  byte *response = (byte *)&out;
  R2I2C.setResponse(response, RESPONSE_PACKAGE_SIZE(out));
  
}

#endif

ResponsePackage execute(RequestPackage *request) {
  
  ResponsePackage response;
   
  response.id = request->id;
  response.action = request->action;
  response.host = request->host;
  response.contentSize = 0;
  response.messageId = messageId++;
  
  // If the ACTION_SET_NODE_ID is called, this node will be "configured" with a new id. Intended for setting up nodes only.
  if (request->action == ACTION_SET_NODE_ID) {
      
      response.host = request->id;
      
        // Store the id. This host will now be known as 'request->id'.
      saveNodeId(request->id);
      return response;
        
  }  
  
  #ifdef USE_RH24
  
  if (request->action == ACTION_CHECK_NODE) {
  
    response.contentSize = 1;
    response.content[RESPONSE_POSITION_HOST_AVAILABLE] = nodeAvailable(request->host);
  
  } else if (request->action == ACTION_RH24_PING_SLAVE) {
  
      //TODO: if (isError(response)) { setError(response); }
      
  } else if (request->action == ACTION_GET_NODES) {
  
    HOST_ADDRESS *nodes = getNodes();
    int count = nodeCount() > MAX_CONTENT_SIZE ? MAX_CONTENT_SIZE : nodeCount();
    
    response.contentSize = count;
    
    for (int i = 0; i < count; i++) { response.content[i] = nodes[i];  }
    
    free(nodes);
  
  } else if (isMaster() && request->host != getNodeId()) {
  
    R2_LOG(F("SENDING RH24 PACKAGE TO:"));
    R2_LOG(request->host);
    response = rh24Send(request);
    if (isError(response)) { setError(response); }
    
  } else if (!isMaster() && request->host != getNodeId()) {
    
    err("E: Routing", ERROR_RH24_ROUTING_THROUGH_NON_MASTER, request->host);

  } else if (request->action == ACTION_SEND_TO_SLEEP) {
  
    if (isMaster()) {
      
      err("E: I'm master.", ERROR_FAILED_TO_SLEEP);
      
    } else {
    
       sleep(request->args[SLEEP_MODE_TOGGLE_POSITION], request->args[SLEEP_MODE_CYCLES_POSITION]);
       
    }
    
  } else if (request->action == ACTION_CHECK_SLEEP_STATE) { 
  
    response.contentSize = 1;
    response.content[0] = isSleeping();
    
  } else if (request->action == ACTION_PAUSE_SLEEP) {
  
    pauseSleep(request->args[SLEEP_MODE_TOGGLE_POSITION]);
    
  } else {
  
  #endif
  
     if (!initialized && request->action != ACTION_INITIALIZE) {
     
       R2_LOG(F("I'm not initialized. Please do so..."));
       R2_LOG(request->host);
       // If initialization wasn't done and if the request was not an initialization request. The remote must initialize prior to any other action.
       response.action = ACTION_INITIALIZE;
       
#ifdef USE_RH24
       // Pause my sleep for a short period of time (PAUSE_SLEEP_DEFAULT_INTERVAL)
       if (isSleeping()) { pauseSleep(); }
#endif
       
       return response;
       
     }
     
    switch(request->action) {
    
      case ACTION_CREATE_DEVICE: {
        
        response.id = deviceCount++;
  
        // Get the type of the device to create from the args.
        DEVICE_TYPE type = request->args[REQUEST_ARG_CREATE_TYPE_POSITION];
        
        // The parameters are everything (mainly port information) that comes after the type parameter.    
        byte *parameters = request->args + REQUEST_ARG_CREATE_PORT_POSITION;
        
        R2_LOG(F("Creating device of type:"));
        R2_LOG(type);
        R2_LOG(F("With id:"));
        R2_LOG(response.id);
        createDevice(response.id, type, parameters);
        
      }
      
      // Will fall through here from ACTION_CREATE_DEVICE in order to return the values.
      request->id = response.id;
      
      case ACTION_GET_DEVICE: {
          
            R2_LOG(F("Retrieved device with id:"));
            R2_LOG(request->id);
         
           Device *device = getDevice(request->id);
         
          if (device) {
        
            response.contentSize = RESPONSE_VALUE_CONTENT_SIZE;
            byte *result = (byte *)getValue(device);
            
            for (int i = 0; i < response.contentSize; i++) { response.content[i] = result[i]; }
            
            free (result);
            
          } else {
          
            err("E: Device not found", ERROR_CODE_NO_DEVICE_FOUND, request->id);
            
          }
          
        } break;
        
      case ACTION_SET_DEVICE: {
        
          Device *device = getDevice(request->id);
    
          if (!device) {
            
            err("E: Device not found", ERROR_CODE_NO_DEVICE_FOUND, request->id);
            
          } else {
            
            R2_LOG(F("Setting device with id:"));
            R2_LOG(request->id);
            setValue(device, toInt16 ( request->args ));
          
          }
          
        } break;
        
        case ACTION_INITIALIZE:
          
          R2_LOG(F("Initializing"));
          
          deviceCount = 0;
          initialized = true;
          clearError();
          
          for (byte i = 0; i < MAX_DEVICES; i++) { deleteDevice(i); }
  
          response.action = ACTION_INITIALIZATION_OK;
          
          // Ehh.. return the first analogue port name here...
          response.content[0] = A0;
          response.contentSize = 1;
          
          return response;
          
        break;
        
       case ACTION_RESET:
        
          deviceCount = 0;
          initialized = false;
          clearError();
          
          for (byte i = 0; i < MAX_DEVICES; i++) { deleteDevice(i); }
       
       break;
          
       default:
       
          err("Unknown action", ERROR_CODE_UNKNOWN_ACTION, request->action);
          
    }
  
#ifdef USE_RH24
  }
#endif
  if (isError()) { response = createErrorPackage(response.host); }
  
  // Reset error state
  clearError();
  
  return response;
  
}


void setup() {

#ifdef R2_STATUS_LED
pinMode(R2_STATUS_LED, OUTPUT);
reservePort(R2_STATUS_LED);
#endif

#ifdef R2_ERROR_LED
pinMode(R2_ERROR_LED, OUTPUT);
reservePort(R2_ERROR_LED);
#endif

  Serial.begin(SERIAL_BAUD_RATE);
  clearError();
  
#ifdef USE_I2C  
  R2I2C.initialize(DEFAULT_I2C_ADDRESS, i2cReceive);
  R2_LOG(F("Initialized I2C."));
#endif

#ifdef USE_RH24
  rh24Setup();
#endif
   
}

void loop() {
  
  loop_common();
  
  #ifdef USE_SERIAL
    loop_serial();
  #endif
  
  //Serial.println("Starting RH24");
  
  #ifdef USE_RH24
    loop_rh24();
  #endif
  
}
