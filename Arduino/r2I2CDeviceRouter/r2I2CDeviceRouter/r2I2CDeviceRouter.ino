#include "r2I2C_config.h"
#include "r2I2CDeviceRouter.h"
#include "r2Common.h"

#ifdef RH24
  #include "RF24.h"
  #include "RF24Network.h"
  #include "RF24Mesh.h"
#endif

#if defined(USE_ESP8266_WIFI_AP) || defined(USE_ESP8266_WIFI)
  #include "r2ESP8266.h"
#endif

#if defined(USE_SERIAL) || defined(USE_I2C)
  #include "r2I2CSerial.h"
#endif

#ifdef USE_RH24
  #include "r2RH24.h"
#endif

#ifdef RPI_POWER_DETECTION_PORT
  #include "r2PowerReading.h"
#endif

// Counter for messages. For debuging purposes only.
byte messageId = 0;

void setup() {

  //saveNodeId(0);
#ifdef R2_PRINT_DEBUG
  Serial.begin(SERIAL_BAUD_RATE);
  #ifdef USE_SRIAL
    #error "R2_PRINT_DEBUG and USE_SRIAL cant be defined simultaneously"
  #endif
#else
  #ifdef USE_SERIAL
    Serial.begin(SERIAL_BAUD_RATE);
  #endif
#endif

  clearError();

  #ifdef RPI_POWER_DETECTION_PORT
    powerReadingSetup();
  #endif

  #ifdef R2_USE_LED
    setupLeds();
  #endif
  
  #ifdef USE_ESP8266
    wifiSetup();
  #endif
  
  #ifdef USE_I2C
  
    #ifdef USE_RH24
      if (isMaster())
    #endif
    {
      i2cSetup();
    }
  
  #endif
  
  #ifdef USE_RH24
    rh24Setup();
  #endif
   
}

// Handles the interpretation of the RequestPackage. This includes error detection and RH24.
ResponsePackage execute(RequestPackage *request) {
  
  ResponsePackage response;
   
  response.id = request->id;
  response.action = request->action;
  response.host = request->host;
  response.contentSize = 0;
  response.messageId = messageId++;

  // If true, the `action` is ACTION_CREATE_DEVICE.
  bool createAction = false;
    
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

  #ifdef RPI_POWER_DETECTION_PORT
  
  if (request->action == ACTION_ACTIVATE_RPI_CONTROLLER) {

    enableRPiPowerControll(request->args[0]);
    response.checksum = createResponseChecksum(&response);
    
    return response;
  
  }
  
  #endif
  
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
        
        if (!createDevice(response.id, type, parameters)) {
          
          // Unable to create!
          break;
        
        }

        R2_LOG(F("Creating device of type:"));
        R2_LOG(type);
        R2_LOG(F("With id:"));
        R2_LOG(response.id);

        createAction = true;
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

            byte *result;

            // Only provide "args" to getValue, if it's not a fall-through from ACTION_CREATE_DEVICE.
            if (request->action == ACTION_CREATE_DEVICE) { result = (byte *)getValue(device, NULL); }
            else { result = (byte *)getValue(device, request->args); }
            
            for (int i = 0; i < response.contentSize; i++) { response.content[i] = result[i]; }
            
            free (result);

            // If getValue generated an error and if this was a "create device action", delete the device.
            if (getErrorCode() > 0 && createAction) {
              
                deleteDevice(request->id);
                deviceCount--;
              
            }
            
          } else {
          
            err("E: Dev.not found", ERROR_CODE_NO_DEVICE_FOUND, request->id);
            
          }
          
        } break;

        case ACTION_DELETE_DEVICE: {

          deleteDevice(request->id);
          deviceCount--;
          
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

void loop() {

  #ifdef RPI_POWER_DETECTION_PORT
    loop_PowerReading();
  #endif
  
  #ifdef R2_STATUS_LED
    statusIndicator();
  #endif

  #ifdef USE_SERIAL
    loop_serial();
  #endif

  #ifdef USE_ESP8266
    loop_tcp();
  #endif
  
  #ifdef USE_RH24
    loop_rh24();
  #endif
 
 #ifdef USE_RH24
 if (isMaster())
 #endif
 {
  #ifdef USE_I2C
    loop_i2c();
  #endif
 }
}
