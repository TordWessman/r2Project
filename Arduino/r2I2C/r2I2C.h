#ifndef r2I2C_h
#define r2I2C_h

#include <Arduino.h>

// Used as first byte in retransmission to the master, informing that slave is ready to reply.
#define READY_TO_SEND_FLAG 0xFF

class R2I2CCom {

	public:
		// Initializes the I2C bus as slave. The onProcess delegate will be called if the master sends bytes.
		void initialize(int slave_address, void (*onProcess)(byte*, int)); 

		// When application is ready to respond to master, invoke this method. (set data_size to 0 if no response needed).
		void setResponse(byte* data, int data_size);

		R2I2CCom();

};

extern R2I2CCom R2I2C;

#endif
