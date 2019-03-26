/** C language used since I'm planning for support with other platforms */

#include "r2I2C.h"
#include <../Wire/Wire.h>
#include <stdbool.h>

// Used as first byte in retransmission to the master, informing that slave is ready to reply.
#define READY_TO_SEND_FLAG 0xF0

    void _transmissionCleanup();
    void _receiveData(int data_size);
    void _sendData();
    void _initialize(int slave_address, void (*onProcess)(byte*, size_t));
    void _setResponse(byte* data, size_t data_size);

    static int state = 0;
    static byte* output_data = NULL;
    static byte* input_data = NULL;
    static size_t output_size = 0;
    static size_t input_size = 0;

    // If true, the size part of the transmission has been sent.
    static bool size_sent_flag = false;
    // Set to true when the the response us reaady to be transmitted.
    static bool response_ready = false;
    // If true, the READY_TO_SEND_FLAG has been transmitted to the host, indicating that I'm ready to transmit data.
    static bool ready_to_send_flag_sent = false;
    static int _slave_address = DEFAULT_I2C_ADDRESS;
    void (*onProcessI2C)(byte*, size_t);

R2I2CCom :: R2I2CCom() {

	_transmissionCleanup();

}

void R2I2CCom :: initialize(int slave_address, void (*onProcess)(byte*, size_t)) {

	_initialize(slave_address, onProcess);

}

void R2I2CCom :: setResponse(byte* data, size_t data_size) {

	_setResponse(data, data_size);

}

void R2I2CCom :: loop() {

	if (input_data && input_size > 0) {

		if (onProcessI2C) { onProcessI2C(input_data, input_size); }
    
		free(input_data);
		input_data = NULL;
		input_size = 0;

	}

}
void _initialize(int slave_address, void (*onProcess)(byte*, size_t)) {

 	_slave_address = slave_address;
 	Wire.begin(_slave_address);
	onProcessI2C = onProcess;
	Wire.onReceive(_receiveData);
	Wire.onRequest(_sendData);

}

void _receiveData(int data_size){

  _transmissionCleanup();

  if (data_size == 0) { return; }

  input_data = (byte*)malloc(sizeof(byte) * data_size);
  input_size = data_size;
  int i = 0;

  while(Wire.available() && i < data_size) {

	   input_data[i++] = Wire.read();
  
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

	if (!size_sent_flag) {

	    Wire.write(output_size);
	    size_sent_flag = true;

	} else {

		Wire.write(output_data, output_size);
		_transmissionCleanup();

	}

    }

  } else {

	Wire.write(0x0);
  }

}

// Prepare response data
void _setResponse(byte* data, size_t data_size) {

  output_size = data_size;
  output_data = (byte*) malloc(sizeof(byte) * output_size);
  memcpy(output_data, data, output_size);

  response_ready = true;

}

// Prepare for next incomming transmission
void _transmissionCleanup() {

    ready_to_send_flag_sent = false;
    response_ready = false;
    output_size = 0;
    input_size = 0;
    size_sent_flag = false;
    if (output_data) { free(output_data); }
    if (input_data) { free(input_data); }
    output_data = NULL;
    input_data = NULL;

}

R2I2CCom R2I2C = R2I2CCom();
