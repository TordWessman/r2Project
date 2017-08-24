#include <Wire.h> // Must be included
#include <r2I2C.h>
#include <stdbool.h>
#include <Servo.h> 

typedef int DEVICE_TYPE;

typedef struct Devices {

  // Unique id of the perephial (device).
  int id;
  
  // Type of the device (i e DEVICE_TYPE_DIGITAL_INPUT).
  DEVICE_TYPE type;
  
  // What I/O port is used for the device.
  int IOPort;
  
  // The host containing this device (0x0 if device is local).
  long host;
  
  // Object specific data.
  void* object;
  
} Device;

// Available device types.
#define DEVICE_TYPE_DIGITAL_INPUT 1
#define DEVICE_TYPE_DIGITAL_OUTPUT 2
#define DEVICE_TYPE_ANALOGUE_INPUT 3
#define DEVICE_TYPE_SERVO 4

#define MAX_DEVICES 100

// --- Variables
Device devices[MAX_DEVICES];
const char* errMsg;

// --- Methods

// Creates and stores a Device using the specified parameters.
bool createDevice(int id, DEVICE_TYPE type, int IOPort);
Device* getDevice(int id);
bool setValue(Device* device, int value);
int getValue(Device* device);

void err (const char* msg);

bool error = false;

void setup() {

  Serial.begin(9600);
  errMsg = NULL;
  
  if (!createDevice(1, DEVICE_TYPE_DIGITAL_OUTPUT, 8)) {
    error = true;
    Serial.println("Aet bajs");
  }
  
  createDevice(18, DEVICE_TYPE_DIGITAL_INPUT, 2);
  createDevice(42, DEVICE_TYPE_DIGITAL_INPUT, 10);
  //R2I2C.initialize(0x04, pData);
  Serial.println("Ready!");
  
}

Device* getDevice(int id) {

  return &devices[id];
  
}

bool createDevice(int id, DEVICE_TYPE type, int IOPort) {

  if (id >= MAX_DEVICES) {
  
    err("Id > MAX_DEVICES");
    return false;
    
  }
  
  Device device;
  device.id = 42;
  device.type = type;
  device.IOPort = IOPort;
  
  switch (device.type) {
  
    case DEVICE_TYPE_ANALOGUE_INPUT:
    case DEVICE_TYPE_DIGITAL_INPUT:
      pinMode(device.IOPort, INPUT);
      break;
      
  case DEVICE_TYPE_DIGITAL_OUTPUT:
      pinMode(device.IOPort, OUTPUT);
      break;
      
  case DEVICE_TYPE_SERVO:
       {
         Servo *servo = (Servo *) malloc(sizeof(Servo));
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

    Serial.println(msg);
    errMsg = msg;
}

void loop() {
  
  Device *lamp = getDevice(1);
  Device *button = getDevice(18);
  int val = getValue(button);
  if (val == HIGH) {
    
    setValue(lamp,1);
    
  } else {
  
    setValue(lamp,0);
  }
  
  delay(100);

}




void pData(byte* data, int data_size) {

  Serial.println(data_size);
  
  byte test_output[data_size];
  
  for (int i = 0; i < data_size; i++) {
  
    test_output[i] = data[i] + 1;
     
  }
  
  //R2I2C.setResponse(test_output, data_size);
  
}
