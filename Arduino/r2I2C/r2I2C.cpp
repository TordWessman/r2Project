/** C language used since I'm planning for support with other platforms */

#include "r2I2C.h"
#include <../Wire/Wire.h>
#include <stdbool.h>

// Used as first byte in retransmission to the master, informing that slave is ready to reply.
#define READY_TO_SEND_FLAG 0xFF

// The maximum size of the output buffer.
#define MAX_OUTPUT_SIZE 0xFF

    void _transmissionCleanup();
    void _receiveData(int data_size);
    void _sendData();
    void _initialize(int slave_address, void (*onProcess)(byte*, int));
    void _setResponse(byte* data, int data_size);

    static int state = 0;
    static byte output_data[MAX_OUTPUT_SIZE];
    static byte output_size = 0;

    // Set to true when the the response us reaady to be transmitted.
    static bool response_ready = false;
    static bool ready_to_send_flag_sent = false;
    static int _slave_address = DEFAULT_I2C_ADDRESS;
    void (*onProcessI2C)(byte*, int);

R2I2CCom :: R2I2CCom() {

	_transmissionCleanup();

}

void R2I2CCom :: initialize(int slave_address, void (*onProcess)(byte*, int)) {

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
      
	// Sending the first READY_TO_SEND_FLAG which will initiate a read at the master. 
        ready_to_send_flag_sent = true;
        Wire.write(READY_TO_SEND_FLAG);
         
         
    } else {
	
	// Send the actual response buffer.
	Wire.write(output_data, output_size);
	_transmissionCleanup();
      
    }
     
  } else {
  
     // No response set. Waiting for it...
     Wire.write(0);
   
  }
  
}

// Prepare response data
void _setResponse(byte* data, byte data_size) {

  output_size = data_size + 1;

  // The first byte in the transaction has the size value.  
  output_data[0] = data_size;

  for (byte i = 1; i < output_size; i++) {
  
    output_data[i] = data[i];
     
  }
  
  response_ready = true;
  
}

// Prepare for next incomming transmission
void _transmissionCleanup() {

    ready_to_send_flag_sent = false;
    response_ready = false;
    output_size = 0;
    
}

R2I2CCom R2I2C = R2I2CCom();
