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

// Counter for messages. For debuging purposes only.
byte messageId = 0;

#ifdef USE_I2C

// Delegate method for I2C event communication.
void i2cReceive(byte* data, size_t data_size) {

  R2_LOG(F("Receiving i2cdata"));
  ResponsePackage out;
  //RequestPackage *in;
  
  if (data == NULL || data_size < MIN_REQUEST_SIZE) {
    
    err("E: size", ERROR_INVALID_REQUEST_PACKAGE_SIZE, data_size);
    out = createErrorPackage(0x0);
    
  } else {
    
//    in = (RequestPackage *)malloc(sizeof(RequestPackage));
//    memcpy((void *)in, data, data_size);
//    out = execute(in);
//    free(in);
    out = execute((RequestPackage *)data);
    
  }
  
  R2_LOG(F("Will write response with action/size:"));
  Serial.println(out.action);
  Serial.println(RESPONSE_PACKAGE_SIZE(out));
  byte *response = (byte *)&out;
  R2I2C.setResponse(response, RESPONSE_PACKAGE_SIZE(out));
  
}

#endif

// Handles the interpretation of the RequestPackage. This includes error detection and RH24.
ResponsePackage execute(RequestPackage *request) {
  
  ResponsePackage response;
   
  response.id = request->id;
  response.action = request->action;
  response.host = request->host;
  response.contentSize = 0;
  response.messageId = messageId++;
  
  if (createRequestChecksum(request) != request->checksum) {
  
    err("E: checksum", ERROR_BAD_CHECKSUM, request->checksum);
    response = createErrorPackage(request->host);
    response.checksum = createResponseChecksum(&response);
    clearError();
    return response;
    
  }
  
  // If the ACTION_SET_NODE_ID is called, this node will be "configured" with a new id. Intended for setting up nodes only.
  if (request->action == ACTION_SET_NODE_ID) {
      
      response.host = request->id;
      response.checksum = createResponseChecksum(&response);
      
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
    
    if (request->action == ACTION_SEND_TO_SLEEP && isSleeping()) {
        pauseSleep();
    }
    
    response = rh24Send(request);
    
    if (isError(response)) { setError(response); }
    if (isError()) { response = createErrorPackage(response.host); }
  
    clearError();
    response.checksum = createResponseChecksum(&response);
    return response;
    
  } else if (!(request->action == ACTION_INITIALIZE || request->action == ACTION_RESET) && !isMaster() && request->host != getNodeId()) {
    
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
    
  #else
  
    //Handle sleep-actions on non-RH24 nodes (implying that it's I2C or Serial only)
    if (request->action == ACTION_CHECK_NODE) {
  
      response.contentSize = 1;
      response.content[RESPONSE_POSITION_HOST_AVAILABLE] = 0x1;
      response.checksum = createResponseChecksum(&response);
      return response;
      
    } else if (request->action == ACTION_PAUSE_SLEEP || request->action == ACTION_CHECK_SLEEP_STATE) { 

       response.contentSize = 1;
       response.content[0] = 0;
       response.checksum = createResponseChecksum(&response);
       return response;
       
    }
    
  #endif
  
     if (!isInitialized() && !(request->action == ACTION_INITIALIZE || request->action == ACTION_RESET)) {
     
       R2_LOG(F("I'm not initialized. Please do so..."));
       R2_LOG(request->host);
       // If initialization wasn't done and if the request was not an initialization request. The remote must initialize prior to any other action.
       response.action = ACTION_INITIALIZE;
       
#ifdef USE_RH24
       // Pause my sleep for a short period of time (PAUSE_SLEEP_DEFAULT_INTERVAL)
       if (isSleeping()) { pauseSleep(); }
#endif
       
       response.checksum = createResponseChecksum(&response);
       return response;
       
     }
     
    switch(request->action) {
    
      case ACTION_CREATE_DEVICE: {
        
        response.id = deviceCount;
  
        // Get the type of the device to create from the args.
        DEVICE_TYPE type = request->args[REQUEST_ARG_CREATE_TYPE_POSITION];
        
        // The parameters are everything (mainly port information) that comes after the type parameter.    
        byte *parameters = request->args + REQUEST_ARG_CREATE_PORT_POSITION;
        
        R2_LOG(F("Creating device of type:"));
        R2_LOG(type);
        R2_LOG(F("With id:"));
        R2_LOG(response.id);
        createDevice(response.id, type, parameters);
        
        deviceCount++;
        
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
          
            err("E: Dev.not found", ERROR_CODE_NO_DEVICE_FOUND, request->id);
            
          }
          
        } break;
        
      case ACTION_CHECK_INTEGRITY: {
        
        response.contentSize = deviceCount + 1;
        
       #ifdef USE_RH24
        
        response.content[0] = isSleeping() << 7 + isInitialized() << 6 + deviceCount;
        
       #else
       
       response.content[0] = 0 << 7 + isInitialized() << 6 + deviceCount;
       
       #endif
       
        for (int i = 0; i < deviceCount; i++) {
        
          Device *device = getDevice(request->id);
          
          // Verify a devicy using it's type, id and first port.
          response.content[i + 1] = (device->type << 4) + (device->id) + (device->IOPorts[0] ^ 0xFF);
          
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
        
          reset(true);
          deviceCount = 0;
          
          response.action = ACTION_INITIALIZATION_OK;
          
          // Ehh.. return the first analogue port name here...
          response.content[0] = A0;
          response.contentSize = 1;
          
          #ifdef USE_RH24
             // Pause my sleep for a short period of time (PAUSE_SLEEP_DEFAULT_INTERVAL)
             if (isSleeping()) { pauseSleep(); }
          #endif
          
          response.checksum = createResponseChecksum(&response);
          return response;
          
        break;
        
       case ACTION_RESET:
        
          reset(false);
          deviceCount = 0;
          
          #ifdef USE_RH24
             // Pause my sleep for a short period of time (PAUSE_SLEEP_DEFAULT_INTERVAL)
             if (isSleeping()) { pauseSleep(); }
          #endif
          
       break;
          
       default:
       
          err("Unknown action", ERROR_CODE_UNKNOWN_ACTION, request->action);
          
    }
  
#ifdef USE_RH24
  }
#endif
  if (isError()) { response = createErrorPackage(response.host); }
  
  clearError();
  response.checksum = createResponseChecksum(&response);
  return response;
  
}


void setup() {

//saveNodeId(0);
/*
pinMode(R2_RESET_LED1, OUTPUT);
pinMode(R2_RESET_LED2, INPUT);
delay(500);

if(digitalRead(R2_RESET_LED2)) {
  R2_LOG(F("Resetting node to master"));
  saveNodeId(0);
  EEPROM.write(SLEEP_MODE_EEPROM_ADDRESS, 0x00);
}

pinMode(R2_RESET_LED1, INPUT);
*/

// Reset all pins. Just in case...
for (int i = 2; i < 9; i++) {
  pinMode(i, OUTPUT);
  digitalWrite(i, LOW);
  pinMode(i, INPUT);
}

#ifdef R2_STATUS_LED
//I'm alive...
pinMode(R2_STATUS_LED, OUTPUT);
for (int i = 0; i < 5; i++) {
  digitalWrite(R2_STATUS_LED, 1);
  delay(500);
  digitalWrite(R2_STATUS_LED, 0);
  delay(500);
}
reservePort(R2_STATUS_LED);
#endif

#ifdef R2_ERROR_LED
pinMode(R2_ERROR_LED, OUTPUT);
reservePort(R2_ERROR_LED);
#endif

  Serial.begin(SERIAL_BAUD_RATE);
  clearError();
  
#ifdef USE_I2C

#ifdef USE_RH24
if (isMaster())
#endif
{
  R2I2C.initialize(DEFAULT_I2C_ADDRESS, i2cReceive);
  R2_LOG(F("Initialized I2C."));
}

#endif

#ifdef USE_RH24
  rh24Setup();
#endif
   
}

#ifdef R2_STATUS_LED
unsigned long blinkTimer = 0;
bool blinkOn = false;
#define blinkTime 1000
#endif

void loop() {
  
  loop_common();
  
  #ifdef USE_SERIAL
    loop_serial();
  #endif
  
  #ifdef R2_STATUS_LED
  
  if (millis() - blinkTimer >= blinkTime) {

    blinkTimer = millis();
    setStatus(blinkOn);
    blinkOn = !blinkOn;
    
  }
  
  #endif

  #ifdef USE_RH24
  loop_rh24();
  #endif
 
 #ifdef USE_RH24
 if (isMaster())
 #endif
 {
  #ifdef USE_I2C
    R2I2C.loop();
  #endif
 }
}
