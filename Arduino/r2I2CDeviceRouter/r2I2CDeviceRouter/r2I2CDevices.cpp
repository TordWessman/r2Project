#include "Dht11.h"
#include <Servo.h>

#ifdef ESP8266
  #include <NewPingESP8266.h>
#else
  #include <NewPing.h>
#endif

#include "r2I2CDeviceRouter.h"
#include "r2I2C_config.h"
#include "r2Common.h"

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

  R2_LOG(F("Deleting device with id:"));
  R2_LOG(id);
  if (devices[id].object != NULL) {

    if (devices[id].type == DEVICE_TYPE_SERVO) {

        ((Servo *) devices[id].object)->detach();
         
    } else if (devices[id].type == DEVICE_TYPE_DIGITAL_OUTPUT) {
    
      digitalWrite(devices[id].IOPorts[0], LOW);
      
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

bool createDevice(byte id, DEVICE_TYPE type, byte* input) {

  if (id >= MAX_DEVICES) {
    
    reset(false);
    err("MAX_DEVICES", ERROR_CODE_MAX_DEVICES_IN_USE);
    return false;
    
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
  case DEVICE_TYPE_SONAR: {
    
      if (reservePort(input[SONAR_TRIG_PORT]) && reservePort(input[SONAR_ECHO_PORT])) {

        #ifdef ESP8266
          NewPingESP8266 *sonar = new NewPingESP8266(SONAR_TRIG_PORT, SONAR_ECHO_PORT, input[SONAR_MAX_DISTANCE]);
        #else
          NewPing *sonar = new NewPing(SONAR_TRIG_PORT, SONAR_ECHO_PORT, input[SONAR_MAX_DISTANCE]);
        #endif
        
        device.IOPorts[SONAR_TRIG_PORT] = input[SONAR_TRIG_PORT];
        device.IOPorts[SONAR_ECHO_PORT] = input[SONAR_ECHO_PORT];
        device.object = (void *)sonar;
      
      }
  
    } break;
  
  case DEVICE_TYPE_HCSR04_SONAR:
  
      if (reservePort(input[SONAR_TRIG_PORT]) && reservePort(input[SONAR_ECHO_PORT])) {
        
        device.IOPorts[SONAR_TRIG_PORT] = input[SONAR_TRIG_PORT];
        device.IOPorts[SONAR_ECHO_PORT] = input[SONAR_ECHO_PORT];
        
        
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
       
  case DEVICE_TYPE_SIMPLE_MOIST:
  
      if (reservePort(input[SIMPLE_MOIST_ANALOGUE_IN]) && reservePort(input[SIMPLE_MOIST_DIGITAL_OUT])) {
        
        device.IOPorts[SIMPLE_MOIST_ANALOGUE_IN] = input[SIMPLE_MOIST_ANALOGUE_IN];
        device.IOPorts[SIMPLE_MOIST_DIGITAL_OUT] = input[SIMPLE_MOIST_DIGITAL_OUT];
    
        pinMode(device.IOPorts[SIMPLE_MOIST_DIGITAL_OUT], OUTPUT);
      
    } break;
    case DEVICE_TYPE_ANALOGE_OUTPUT:
  
      if (reservePort(input[0])) {
        
        device.IOPorts[0] = input[0];
    
        pinMode(device.IOPorts[0], OUTPUT);
      
    } break;
    
  default:
  
    err("Device type not found.", ERROR_CODE_DEVICE_TYPE_NOT_FOUND, type);
    return false;
  
  }
  
  devices[id] = device;
  return true;
  
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
   
   case DEVICE_TYPE_SONAR: {
     
     #ifdef ESP8266
       NewPingESP8266 *sonar = ((NewPingESP8266 *) device->object);
     #else
       NewPing *sonar = ((NewPing *) device->object);
     #endif
     int value = sonar->ping_cm();
     if (value > 255) { value = 255; }
     values[0] = value;
     break;
   }
   case DEVICE_TYPE_HCSR04_SONAR:
   
     digitalWrite(device->IOPorts[SONAR_TRIG_PORT], HIGH); //Trigger ultrasonic detection 
     delayMicroseconds(10); 
     digitalWrite(device->IOPorts[SONAR_TRIG_PORT], LOW); 
     values[0] = pulseIn(device->IOPorts[SONAR_ECHO_PORT], HIGH) / HCSR04_SONAR_DISTANCE_DENOMIATOR; //Read ultrasonic reflection
     break;
     
   case DEVICE_TYPE_DHT11: {

       Dht11 *sensor = ((Dht11 *) device->object);
       
       switch (sensor->read()) {
         
          case Dht11::OK: {
            
            int temp = sensor->getTemperature();
            int humid = sensor->getHumidity(); 
            values[RESPONSE_POSITION_DHT11_TEMPERATURE] = temp;
            values[RESPONSE_POSITION_DHT11_HUMIDITY] = humid;
            
          } break;
          
          case Dht11::ERROR_TIMEOUT:
          
            values[RESPONSE_POSITION_DHT11_TEMPERATURE] = 0;
            values[RESPONSE_POSITION_DHT11_HUMIDITY] = 0;
            err("DHT Timeout", ERROR_CODE_DHT11_READ_ERROR);
            
            break;

           case Dht11::ERROR_CHECKSUM:
          
            values[RESPONSE_POSITION_DHT11_TEMPERATURE] = 0;
            values[RESPONSE_POSITION_DHT11_HUMIDITY] = 0;
            err("DHT CHCKSM", ERROR_CODE_DHT11_READ_ERROR);
            
            break;
            
          default:
          
            values[RESPONSE_POSITION_DHT11_TEMPERATURE] = 0;
            values[RESPONSE_POSITION_DHT11_HUMIDITY] = 0;
            err("DHT error.", ERROR_CODE_DHT11_READ_ERROR);
       
            break;
       
       }
       // silent error
   } break;
   
   
  case DEVICE_TYPE_SIMPLE_MOIST: {
    
    digitalWrite(device->IOPorts[SIMPLE_MOIST_DIGITAL_OUT], HIGH); // Start measurement cycle
    delayMicroseconds(2);
    analogRead(device->IOPorts[SIMPLE_MOIST_ANALOGUE_IN]); // First reading tends to be invalid
    delayMicroseconds(2);
    values[0] = analogRead(device->IOPorts[SIMPLE_MOIST_ANALOGUE_IN]);
    digitalWrite(device->IOPorts[SIMPLE_MOIST_DIGITAL_OUT], LOW); // End measurement
    
  } break;
  
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
        
    case DEVICE_TYPE_ANALOGE_OUTPUT:

        analogWrite(device->IOPorts[0], value > 255 ? 255 : value);
        break;
        
    default:
    
      err("Unable to set setDevice.", ERROR_CODE_DEVICE_TYPE_NOT_FOUND_SET_DEVICE);
    
  }

}
