
#include "r2I2CDeviceRouter.h"
#include <Arduino.h>
#include <Servo.h>

// Use with caution. If defined, serial errors will be printed through serial port.
#define PRINT_ERRORS_AND_FUCK_UP_SERIAL_COMMUNICATION

// -- Variables
Device devices[MAX_DEVICES];
bool portsInUse[30];

// -- Conversions

int toInt16(byte *bytes) { return bytes[0] + (bytes[1] << 8);  }
byte* asInt16(int value) { byte* bytes = (byte *) malloc(2 * sizeof(byte)); bytes[0] = value; bytes[1] = value >> 8; return bytes; }

// Error handling

const char* errMsg;

const char* getError() {
    return errMsg;
}

void err (const char* msg) {

#ifdef PRINT_ERRORS_AND_FUCK_UP_SERIAL_COMMUNICATION
    if (Serial && msg) { Serial.println(msg); }
#endif

    errMsg = msg;
    
}

// -- Device handling

Device* getDevice(byte id) {

  if (id >= 0 && id < MAX_DEVICES && devices[id].type != DEVICE_TYPE_UNDEFINED) {
  
    return &devices[id];
  
  }
  
  err("No device found");
  
  return NULL;
  
}

void deleteDevice(byte id) {

  if (devices[id].object != NULL) {

    if (devices[id].type == DEVICE_TYPE_SERVO) {

        ((Servo *) devices[id].object)->detach();
         
    }
    
    free (devices[id].object);
    
  }
  
  portsInUse[devices[id].IOPorts[0]] = false;
  
  devices[id].id = 0;
  devices[id].type = DEVICE_TYPE_UNDEFINED;
  devices[id].object = NULL;
  
}

// TODO: im
bool reservePort(byte IOPort) {

  if (portsInUse[IOPort] == true) {
    
    err("Port in use.");
    return false;
  }
  
  portsInUse[IOPort] = true;
  return true;
  
}

bool createDevice(byte id, DEVICE_TYPE type, byte* input) {

  if (id >= MAX_DEVICES) {
  
    err("Id > MAX_DEVICES");
    return false;
    
  } 
  
  deleteDevice(id);
  
  Device device;
  device.id = id;
  device.type = type;
  device.object = NULL;
  
  switch (device.type) {
  
    case DEVICE_TYPE_ANALOGUE_INPUT:
      device.IOPorts[0] = input[0];
      break;
      
    case DEVICE_TYPE_DIGITAL_INPUT:
      device.IOPorts[0] = input[0];
      pinMode(device.IOPorts[0], INPUT);
      break;
      
  case DEVICE_TYPE_DIGITAL_OUTPUT:
      device.IOPorts[0] = input[0];
      pinMode(device.IOPorts[0], OUTPUT);
      break;
      
  case DEVICE_TYPE_HCSR04_SONAR:
      device.IOPorts[HCSR04_SONAR_TRIG_PORT] = input[HCSR04_SONAR_TRIG_PORT];
      device.IOPorts[HCSR04_SONAR_ECHO_PORT] = input[HCSR04_SONAR_ECHO_PORT];
      pinMode(device.IOPorts[HCSR04_SONAR_TRIG_PORT], OUTPUT);
      pinMode(device.IOPorts[HCSR04_SONAR_ECHO_PORT], INPUT);  
      break;
      
  case DEVICE_TYPE_SERVO:
       { 
         device.IOPorts[0] = input[0];
         Servo *servo =  new Servo();
         servo->attach(device.IOPorts[0]);
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
      return digitalRead(device->IOPorts[0]);
      
   case DEVICE_TYPE_ANALOGUE_INPUT:
     return analogRead(device->IOPorts[0]);
   
   case DEVICE_TYPE_HCSR04_SONAR:
     digitalWrite(device->IOPorts[HCSR04_SONAR_TRIG_PORT], HIGH); //Trigger ultrasonic detection 
     delayMicroseconds(10); 
     digitalWrite(device->IOPorts[HCSR04_SONAR_TRIG_PORT], LOW); 
     return pulseIn(device->IOPorts[HCSR04_SONAR_ECHO_PORT], HIGH) / HCSR04_SONAR_DISTANCE_DENOMIATOR; //Read ultrasonic reflection
     
  }
  
  err("Unable to read from device.");
  return 0;
  
}

bool setValue(Device* device, int value) {

  switch (device->type) {

  case DEVICE_TYPE_DIGITAL_OUTPUT:
      digitalWrite(device->IOPorts[0], value > 0 ? HIGH : LOW);
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
