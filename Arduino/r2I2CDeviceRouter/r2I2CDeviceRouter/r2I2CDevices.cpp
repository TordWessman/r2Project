#include "Dht11.h"
#include <Servo.h>

#include "r2I2CDeviceRouter.h"
#include "r2I2C_config.h"
#include "r2Common.h"
//xxx #include <Arduino.h>

// -- Variables
Device devices[MAX_DEVICES];
bool portsInUse[30];

// -- Device handling

Device* getDevice(byte id) {

  if (id >= 0 && id < MAX_DEVICES && devices[id].type != DEVICE_TYPE_UNDEFINED) {
  
    return &devices[id];
  
  }
  
  err("No device found", ERROR_CODE_NO_DEVICE_FOUND);
  
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
    
    err("Port in use.", ERROR_CODE_PORT_IN_USE);
    return false;
    
  }
  
  portsInUse[IOPort] = true;
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
  case DEVICE_TYPE_DHT11:
       { 
         device.IOPorts[0] = input[0];
         Dht11 *dht11 =  new Dht11(device.IOPorts[0]);
         device.object = (void *)dht11;
       }
       break;
    
  default:
  
    return err("Unable to create device. Device type not found.", ERROR_CODE_DEVICE_TYPE_NOT_FOUND);;
  
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
   case DEVICE_TYPE_DHT11:
   {
       Dht11 *sensor = ((Dht11 *) device->object); 
       switch (sensor->read()) {
          case Dht11::OK:
            {
            int temp = sensor->getTemperature();;
            int humid = sensor->getHumidity(); 
            Serial.println(temp);
            Serial.println(humid);
            values[DHT11_TEMPERATUR_RESPONSE_POSITION] = temp;
            values[DHT11_HUMIDITY_RESPONSE_POSITION] = humid;
            }
          break;
          case Dht11::ERROR_TIMEOUT:
            values[DHT11_TEMPERATUR_RESPONSE_POSITION] = 0;
            values[DHT11_HUMIDITY_RESPONSE_POSITION] = 0;
            err("Unable to read from device (DHT Timeout).", ERROR_CODE_DEVICE_READ_ERROR);
            break;
          default:
            values[DHT11_TEMPERATUR_RESPONSE_POSITION] = 0;
            values[DHT11_HUMIDITY_RESPONSE_POSITION] = 0;
            err("Unable to read from device (DHT error. Probably checksum).", ERROR_CODE_DEVICE_READ_ERROR);
            break;
       }
       
       // silent error
   }
      break;
   default:
    err("Unable to read from device.", ERROR_CODE_DEVICE_TYPE_NOT_FOUND_READ_DEVICE);
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
    err("Unable to set setDevice (set device value). Specified device does not exist or is of a read-only DEVICE_TYPE.", ERROR_CODE_DEVICE_TYPE_NOT_FOUND_SET_DEVICE);
    
  }

}

