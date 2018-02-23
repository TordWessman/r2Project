#include "Dht11.h"
#include <Servo.h>

#include "r2I2CDeviceRouter.h"
#include "r2I2C_config.h"
#include "r2Common.h"
//xxx #include <Arduino.h>

// A device that has this port reserved is lying.
#define DEVICE_PORT_NOT_IN_USE 0xFF

// -- Variables
Device devices[MAX_DEVICES];

// Defines the maximum number of ports to be reserved
#define MAX_PORTS 15
byte portsInUse[MAX_PORTS];

// -- Device handling

Device* getDevice(byte id) {

  if (id >= 0 && id < MAX_DEVICES && devices[id].type != DEVICE_TYPE_UNDEFINED) {
  
    return &devices[id];
  
  }
  
  return NULL;
  
}

void deleteDevice(byte id) {

  if (devices[id].object != NULL) {

    if (devices[id].type == DEVICE_TYPE_SERVO) {

        ((Servo *) devices[id].object)->detach();
         
    }
    
    free (devices[id].object);
    
  }
  
  for(int i = 0; i < DEVICE_MAX_PORTS; i++) {
    
    // Frees all ports with value != DEVICE_PORT_NOT_IN_USE
    if (devices[id].IOPorts[i] != DEVICE_PORT_NOT_IN_USE) {
      
      for(int j = 0; j < MAX_PORTS; j++) {
        
          if (portsInUse[i] == devices[id].IOPorts[i]) { portsInUse[i] = DEVICE_PORT_NOT_IN_USE; }
      
      }  
    
    }
  
  }
  
  devices[id].id = 0;
  devices[id].type = DEVICE_TYPE_UNDEFINED;
  devices[id].object = NULL;
  
}

bool reservePort(byte IOPort) {
/*
TODO: fix this...
  if (portsInUse[IOPort] == true) {
    
    err("Port in use.", ERROR_CODE_PORT_IN_USE);
    return false;
    
  }
  
  R2_LOG(F("Reserving port: "));
  R2_LOG(IOPort);
  
  portsInUse[IOPort] = true;
  */
  return true;
  
}

void createDevice(byte id, DEVICE_TYPE type, byte* input) {

  if (id >= MAX_DEVICES) {
  
    return err("Id > MAX_DEVICES", ERROR_CODE_MAX_DEVICES_IN_USE);
    
  } 
  
  deleteDevice(id);
  
  Device device;
  device.id = id;
  device.type = type;
  device.object = NULL;
  
  for (int i = 0; i < DEVICE_MAX_PORTS; i++) {
    device.IOPorts[i] = DEVICE_PORT_NOT_IN_USE;
  }
  
  switch (device.type) {
  
    case DEVICE_TYPE_ANALOGUE_INPUT:
    
      if (reservePort(input[0])) { device.IOPorts[0] = input[0]; }
      break;
      
    case DEVICE_TYPE_DIGITAL_INPUT:
    
      if (reservePort(input[0])) {
        
        device.IOPorts[0] = input[0];
        pinMode(device.IOPorts[0], INPUT);
      
      } break;
      
  case DEVICE_TYPE_DIGITAL_OUTPUT:
  
      if (reservePort(input[0])) {
        
        device.IOPorts[0] = input[0];
        pinMode(device.IOPorts[0], OUTPUT);
      
    } break;
   
  case DEVICE_TYPE_HCSR04_SONAR:
  
      if (reservePort(input[HCSR04_SONAR_TRIG_PORT]) && reservePort(input[HCSR04_SONAR_ECHO_PORT])) {
        
        device.IOPorts[HCSR04_SONAR_TRIG_PORT] = input[HCSR04_SONAR_TRIG_PORT];
        device.IOPorts[HCSR04_SONAR_ECHO_PORT] = input[HCSR04_SONAR_ECHO_PORT];
        pinMode(device.IOPorts[HCSR04_SONAR_TRIG_PORT], OUTPUT);
        pinMode(device.IOPorts[HCSR04_SONAR_ECHO_PORT], INPUT);
        
      } break;
      
  case DEVICE_TYPE_SERVO: { 
         
         if (reservePort(input[0])) {
         
           device.IOPorts[0] = input[0];
           Servo *servo =  new Servo();
           servo->attach(device.IOPorts[0]);
           device.object = (void *)servo;
           
         }
         
       } break;
       
       
  case DEVICE_TYPE_DHT11: { 
    
         if (reservePort(input[0])) {
    
           device.IOPorts[0] = input[0];
           Dht11 *dht11 =  new Dht11(device.IOPorts[0]);
           device.object = (void *)dht11;
    
         }
         
       } break;
   
  default:
  
    return err("Device type not found.", ERROR_CODE_DEVICE_TYPE_NOT_FOUND, type);
  
  }
  
  devices[id] = device;

}

int* getValue(Device* device) {

  int *values = (int *)malloc(RESPONSE_VALUE_CONTENT_SIZE);
  
  for (int i = 0; i < RESPONSE_VALUE_COUNT; i++) { values [i] = 0; }
  
  switch (device->type) {
    
    case DEVICE_TYPE_DIGITAL_INPUT:
    
      values[0] = digitalRead(device->IOPorts[0]);   
      break;
      
   case DEVICE_TYPE_ANALOGUE_INPUT:
   
     values[0] = analogRead(device->IOPorts[0]);
     break;
     
   case DEVICE_TYPE_HCSR04_SONAR:
   
     digitalWrite(device->IOPorts[HCSR04_SONAR_TRIG_PORT], HIGH); //Trigger ultrasonic detection 
     delayMicroseconds(10); 
     digitalWrite(device->IOPorts[HCSR04_SONAR_TRIG_PORT], LOW); 
     values[0] = pulseIn(device->IOPorts[HCSR04_SONAR_ECHO_PORT], HIGH) / HCSR04_SONAR_DISTANCE_DENOMIATOR; //Read ultrasonic reflection
     break;
     
   case DEVICE_TYPE_DHT11: {
     
       Dht11 *sensor = ((Dht11 *) device->object); 
       
       switch (sensor->read()) {
         
          case Dht11::OK: {
            
            int temp = sensor->getTemperature();;
            int humid = sensor->getHumidity(); 
            values[RESPONSE_POSITION_DHT11_TEMPERATURE] = temp;
            values[RESPONSE_POSITION_DHT11_HUMIDITY] = humid;
            
          } break;
          
          case Dht11::ERROR_TIMEOUT:
          
            values[RESPONSE_POSITION_DHT11_TEMPERATURE] = 0;
            values[RESPONSE_POSITION_DHT11_HUMIDITY] = 0;
            err("DHT Timeout", ERROR_CODE_DEVICE_READ_ERROR);
            
            break;
            
          default:
          
            values[RESPONSE_POSITION_DHT11_TEMPERATURE] = 0;
            values[RESPONSE_POSITION_DHT11_HUMIDITY] = 0;
            err("DHT error.", ERROR_CODE_DEVICE_READ_ERROR);
       
            break;
       
       }
       // silent error
   } break;
   
   default:
   
    err("Bad device type", ERROR_CODE_DEVICE_TYPE_NOT_FOUND_READ_DEVICE);
    break;
    
  }
  
  return values;
  
}

void setValue(Device* device, int value) {

  switch (device->type) {
  
    case DEVICE_TYPE_DIGITAL_OUTPUT:
    
        digitalWrite(device->IOPorts[0], value > 0 ? HIGH : LOW);
        break;
        
    case DEVICE_TYPE_SERVO:
    
        ((Servo *) device->object)->write(value);
        break;
        
    default:
    
      err("Unable to set setDevice.", ERROR_CODE_DEVICE_TYPE_NOT_FOUND_SET_DEVICE);
    
  }

}

