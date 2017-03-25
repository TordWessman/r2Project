#include <Wire.h>
#include <r2I2C.h>

void setup() {

  Serial.begin(9600);

  R2I2C.initialize(0x04, pData);
  Serial.println("Ready!");
  
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
  
  R2I2C.setResponse(test_output, data_size);
  
}
