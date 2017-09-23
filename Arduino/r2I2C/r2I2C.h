#ifndef r2I2C_h
#define r2I2C_h

#include <Arduino.h>

// Default address used by this slave as I2C port.
#define DEFAULT_I2C_ADDRESS 0x04

class R2I2CCom {

	public:
		// Initializes the I2C bus as slave. The onProcess(byte <received bytes>, int <data_size>) delegate will be called when the master sends data.
		void initialize(int slave_address, void (*onProcess)(byte*, int)); 

		// When application is ready to respond to master, invoke this method. (set data_size to 0 if no response is needed).
		void setResponse(byte* data, int data_size);

		R2I2CCom();

};

extern R2I2CCom R2I2C;

#endif
