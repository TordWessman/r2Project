#include <Wire.h> // Must be included
#include <r2I2C.h>
#include <Servo.h>
#include <string.h>
#include "r2I2CDeviceRouter.h"

// --- Variables
Device devices[MAX_DEVICES];
byte readBuffer[MAX_RECEIZE_SIZE];

void err (const char* msg);
const char* errMsg;
bool error = false;

// read input sizesize
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

Device* getDevice(byte id) {

  if (id >= 0 && id < MAX_DEVICES && devices[id].type != DEVICE_TYPE_UNDEFINED) {
  
    return &devices[id];
  
  }
  
  err("No device found");
  
  return NULL;
  
}

bool createDevice(byte id, DEVICE_TYPE type, byte IOPort) {

  if (id >= MAX_DEVICES) {
  
    err("Id > MAX_DEVICES");
    return false;
    
  } else if (devices[id].type != DEVICE_TYPE_UNDEFINED && devices[id].object != NULL) {
  
    free (devices[id].object);
    
  }
  
  Device device;
  device.id = id;
  device.type = type;
  device.IOPort = IOPort;
  device.object = NULL;
  
  switch (device.type) {
  
    case DEVICE_TYPE_ANALOGUE_INPUT:
      break;
      
    case DEVICE_TYPE_DIGITAL_INPUT:
      pinMode(device.IOPort, INPUT);
      break;
      
  case DEVICE_TYPE_DIGITAL_OUTPUT:
      pinMode(device.IOPort, OUTPUT);
      break;
      
  case DEVICE_TYPE_SERVO:
       { 
         Servo *servo =  new Servo();
         servo->attach(device.IOPort);
         device.object = (void *)servo;
       }
       break;
       
  default:
  
    err("Unable to create device. Device type not found.");
    return false;
  
  }
  
  devices[id] = device;
  
  return true;
  
}

int getValue(Device* device) {

  switch (device->type) {
    
    case DEVICE_TYPE_DIGITAL_INPUT:
      return digitalRead(device->IOPort);
      
   case DEVICE_TYPE_ANALOGUE_INPUT:
     return analogRead(device->IOPort);
  }
  
  err("Unable to read from device.");
  return 0;
  
}

bool setValue(Device* device, int value) {

  switch (device->type) {

  case DEVICE_TYPE_DIGITAL_OUTPUT:
      digitalWrite(device->IOPort, value > 0 ? HIGH : LOW);
      break;
      
  case DEVICE_TYPE_SERVO:
      ((Servo *) device->object)->write(value);
      break;
      
  default:
    err("Unable to set setDevice (set device value). Specified device does not exist or is of a read-only DEVICE_TYPE.");
    return false;
    
  }
  
  return true;
  
}

void err (const char* msg) {

#ifdef PRINT_ERRORS_AND_FUCK_UP_SERIAL_COMMUNICATION
    if (Serial) { Serial.println(msg); }
#endif

    errMsg = msg;
    
}

int toInt16(byte *bytes) { return bytes[0] + (bytes[1] << 8);  }

byte* asInt16(int value) { byte* bytes = (byte *) malloc(2 * sizeof(byte)); bytes[0] = value; bytes[1] = value >> 8; return bytes; }

ResponsePackage execute(RequestPackage *request) {

  ResponsePackage response;
  
   response.id = request->id;
   response.action = request->action;
   response.host = request->host;
   response.contentSize = 0;
   
  switch(request->action) {
  
    case ACTION_CREATE_DEVICE:
      
      if (!createDevice(request->id, request->args[REQUEST_ARG_CREATE_TYPE_POSITION], request->args[REQUEST_ARG_CREATE_PORT_POSITION])) {
        
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
  
  if (errMsg != NULL) {
  
    response.action = ACTION_ERROR;
          
  }
  
  return response;
  
}


ResponsePackage interpret(byte* input) {

  RequestPackage *request = (RequestPackage*) input;

  ResponsePackage out = execute(request);

  if (out.action == ACTION_ERROR) {
  
    out.contentSize = strlen(errMsg);
    
    for (int i = 0; i < out.contentSize; i++) { out.content[i] = ((byte*) errMsg)[i]; }
    
  }
  
  errMsg = NULL;

  return out;
  
}

void setup() {

  
#ifdef USE_SERIAL
  Serial.begin(9600);
#else
  R2I2C.initialize(DEFAULT_I2C_ADDRESS, i2cReceive);
#endif

  errMsg = NULL;

  for (int i = 0; i < MAX_DEVICES; i++) { devices[i].type = DEVICE_TYPE_UNDEFINED; }
 
}

void loop() {
  
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
      
      // Size
      Serial.write((byte) outputSize);
      
      // Content
      for (int i = 0; i < outputSize; i++) { Serial.write(outputBuffer[i]); }
      
    }
    
  }
  
}

void i2cReceive(byte* data, int data_size) {

  ResponsePackage out = interpret(data); 
  byte *response = (byte *)&out;
  int responseSize = sizeof(ResponsePackage) - MAX_CONTENT_SIZE + out.contentSize;
  
  R2I2C.setResponse(response, responseSize);
  
}
