#include <Wire.h> // Must be included
#include <r2I2C.h>
#include <Servo.h> 
#include "r2I2CDeviceRouter.h"

// --- Variables
Device devices[MAX_DEVICES];

void err (const char* msg);
const char* errMsg;
bool error = false;

Device* getDevice(byte id) {

  return &devices[id];
  
}

bool createDevice(byte id, DEVICE_TYPE type, byte IOPort) {

  if (id >= MAX_DEVICES) {
  
    err("Id > MAX_DEVICES");
    return false;
    
  }
  
  Device device;
  device.id = id;
  device.type = type;
  device.IOPort = IOPort;
  
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

    if (Serial) { Serial.println(msg); }
    errMsg = msg;
    
}

int toInt16(byte *bytes) { return bytes[0] + (bytes[1] << 8);  }

byte* asInt16(int value) { byte* bytes = (byte *) malloc(2 * sizeof(byte)); bytes[0] = value; bytes[1] = value >> 8; return bytes; }

ResponsePackage parsePackage(RequestPackage *request) {

  ResponsePackage response;
  
   response.id = request->id;
   response.action = request->action;
   response.host = request->host;
   
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
  
        if (!device) {
          
          response.action = ACTION_ERROR;
          
        }
        
        response.contentSize = 2;
        response.content = asInt16(getValue(device));
        
      }
      break;
    
     default:
     
        err("Unknown action sent to device router.");
        response.action = ACTION_ERROR;
     
  }
  
  
  return response;
  
}


void setup() {

  Serial.begin(9600);
  errMsg = NULL;

  Serial.println("Starting...");
  delay(1000);
  byte port = 8;
  byte id = 0xA;
  byte tst[] = {DEVICE_HOST_LOCAL, ACTION_CREATE_DEVICE , id, DEVICE_TYPE_DIGITAL_OUTPUT, port};
  
  RequestPackage *inp = (RequestPackage*) &tst;
  
  Serial.println(inp->id);
  Serial.println(inp->action);
  Serial.println(inp->args[REQUEST_ARG_CREATE_PORT_POSITION]);

  //R2I2C.initialize(0x04, pData);
  Serial.println("Ready!");
  
}


void loop() {
  
  delay(10000);

}

void pData(byte* data, int data_size) {

  Serial.println(data_size);
  
  byte test_output[data_size];
  
  for (int i = 0; i < data_size; i++) {
  
    test_output[i] = data[i] + 1;
     
  }
  
  //R2I2C.setResponse(test_output, data_size);
  
}
