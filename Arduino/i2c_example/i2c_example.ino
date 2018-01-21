#include <Wire.h>
#include <r2I2C.h>

void setup() {

  Serial.begin(9600);

  R2I2C.initialize(0x04, pData);
  //Serial.println("Ready!");
  
}

static  byte test_output[0xFF];
static int sszz = 0;

void loop() {
  
  delay(5000);
   if (sszz > 0) {
     for (int i = 0; i < sszz; i++) {
        Serial.print(test_output[i]);
        Serial.print(" - "); 
        delay(100);    
    }
    Serial.println("");
   }
  //Serial.print (".");
}



void pData(byte* data, int data_size) {

  Serial.print("Input size: ");    
  Serial.println(data_size);
  sszz = data_size;
  for (int i = 0; i < data_size; i++) {
  
    test_output[i] = data[i] + 1;
  
 }
  
  R2I2C.setResponse(test_output, data_size);
  
}
