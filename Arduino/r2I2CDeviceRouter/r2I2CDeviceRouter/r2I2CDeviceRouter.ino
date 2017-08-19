#include <Wire.h> // Must be included
#include <r2I2C.h>
#include <stdbool.h>
#include <Servo.h> 

struct Device {

  int id;
  int type;
  int port;
  
};

#define DIGITAL_INPUT 1
#define DIGITAL_OUTPUT 2
#define ANALOGUE_INPUT 3
#define PWM_OUTPUT 4

#define MAX_DEVICES 100

// Variables
struct Device devices[MAX_DEVICES];
int deviceCount = 0;
const char* errMsg;

// Methods
bool addDevice(struct Device device);
void err (const char* msg);
struct Device* getDevice(int id);

void setup() {

  Serial.begin(9600);
  errMsg = NULL;
  
  struct Device d;
  d.id = 42;
  d.type = DIGITAL_OUTPUT;
  d.port = 8;
  addDevice(d);
  
  //R2I2C.initialize(0x04, pData);
  Serial.println("Ready!");
  
}

struct Device* getDevice(int id) {

  for(int i = 0; i < MAX_DEVICES; i++) {
  
    if (devices[i].id == id) {
    
      return &devices[i];
      
    }
    
  }
  
  err("No device found.");
  
  return NULL;
  
}

bool addDevice(struct Device device) {

  if (deviceCount > MAX_DEVICES) {
  
    err("Max devices reached");
    return false;
    
  }
  
  switch (device.type) {
  
    case DIGITAL_INPUT:
      pinMode(device.id, INPUT);
      break;
  case DIGITAL_OUTPUT:
      pinMode(device.id, OUTPUT);
      break;
  }
  devices[deviceCount++] = device;
  
  return true;
  
}

void err (const char* msg) {

    Serial.println(msg);
    errMsg = msg;
}

void loop() {
  
  delay(1000);

}




void pData(byte* data, int data_size) {

  Serial.println(data_size);
  
  byte test_output[data_size];
  
  for (int i = 0; i < data_size; i++) {
  
    test_output[i] = data[i] + 1;
     
  }
  
  //R2I2C.setResponse(test_output, data_size);
  
}
