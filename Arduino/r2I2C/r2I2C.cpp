/** C language used since I'm planning for support with other platforms */

#include "r2I2C.h"
#include <../Wire/Wire.h>
#include <stdbool.h>

#define DEFAULT_SLAVE_ADDRESS 0x04


    
    void _transmissionCleanup();
    void _receiveData(int data_size);
    void _sendData();
    void _initialize(int slave_address, void (*onProcess)(byte*, int));
    void _setResponse(byte* data, int data_size);

    static int state = 0;
    static byte output_data[0xFF];
    static unsigned int output_size = 0;
    
    static int send_count = 0;
    // Set to true when the the response us reaady to be transmitted.
    static bool response_ready = false;
    static bool ready_to_send_flag_sent = false;
    static int _slave_address = DEFAULT_SLAVE_ADDRESS;
    void (*onProcessI2C)(byte*, int);

R2I2CCom :: R2I2CCom() {

	_transmissionCleanup();

}

void R2I2CCom :: initialize(int slave_address, void (*onProcess)(byte*, int)) {

	Serial.println("ahaha");
	_initialize(slave_address, onProcess);
	
}

void R2I2CCom :: setResponse(byte* data, int data_size) {

	_setResponse(data, data_size);

}

void _initialize(int slave_address, void (*onProcess)(byte*, int)) {

 	_slave_address = slave_address;
 	Wire.begin(_slave_address);
	onProcessI2C = onProcess;
	Wire.onReceive(_receiveData);
	Wire.onRequest(_sendData);
  
}

void _receiveData(int data_size){

  _transmissionCleanup();
  
  if (data_size == 0) { return; }
  
  byte received_data[data_size];
  int i = 0;
  
  while(Wire.available()) {
  
    received_data[i] = Wire.read();
    i++;
    
  }
 
  if (onProcessI2C) {
  	
    onProcessI2C(received_data, i);
    
  }
  
}

void _sendData() {

	// For some reason, this line must be here...
	Serial.print("");

  if (response_ready) {
  
    if (!ready_to_send_flag_sent) {
      
        ready_to_send_flag_sent = true;
        Wire.write(READY_TO_SEND_FLAG);
         
         
    } else {

	Wire.write(output_data, output_size);
	_transmissionCleanup();
    /*
      Wire.write(output_data[send_count]);
      send_count++;
      
      if (send_count == output_size) {
        
          _transmissionCleanup();
      
      }*/
      
    }
     
  } else {
  
     Wire.write(0);
   
  }
  
}

void _setResponse(byte* data, int data_size) {

  output_size = data_size;
  
  for (int i = 0; i < data_size; i++) {
  
    output_data[i] = data[i];
     
  }
  
  response_ready = true;
  
}

void _transmissionCleanup() {

    ready_to_send_flag_sent = false;
    response_ready = false;
    send_count = 0;
    output_size = 0;
    
}

R2I2CCom R2I2C = R2I2CCom();