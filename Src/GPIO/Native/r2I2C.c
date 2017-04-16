// This file is part of r2Poject.
//
// Copyright 2016 Tord Wessman
// 
// r2Project is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// r2Project is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with r2Project. If not, see <http://www.gnu.org/licenses/>.
// 

#include "r2I2C.h"

#include <sys/ioctl.h>
#include <unistd.h>
#include <linux/i2c-dev.h>
#include <stdio.h>
#include <fcntl.h>
#include <errno.h>
#include <string.h>
#include <unistd.h>

static int _r2I2C_i2caddr;
static int _r2I2C_i2cbus;
static char _r2I2C_busfile[64];
static uint8_t _r2I2C_dataBuffer[R2I2C_MAX_BUFFER_SIZE];
static bool _r2I2C_should_run = true;
static bool _r2I2C_is_busy = false;
static bool _r2I2C_is_initialized = false;
static bool _r2I2C_is_reading = false;

int r2I2C_open_bus ();

int r2I2C_init (int bus, int address) {

	_r2I2C_i2cbus = bus;
	_r2I2C_i2caddr = address;
	snprintf(_r2I2C_busfile, sizeof(_r2I2C_busfile), "/dev/i2c-%d", bus);

	int status = r2I2C_open_bus();
 
	if (status < 0) {
		
		_r2I2C_is_initialized = false;
		return R2I2C_BUS_ERROR;

	} 

	_r2I2C_is_initialized = true;

	return status;

}

int r2I2C_open_bus () {

	int fd;

	if ((fd = open(_r2I2C_busfile, O_RDWR)) < 0) {

		printf ("Couldn't open I2C Bus %d [r2I2C_open_bus():open %d]\n", _r2I2C_i2cbus, errno);
		return -1;

	} else if (ioctl(fd, I2C_SLAVE, _r2I2C_i2caddr) < 0) {

		printf ("I2C slave %d failed [r2I2C_open_bus():ioctl %d]\n", _r2I2C_i2caddr, errno);
		return -1;

	}

	return fd;
}

int r2I2C_receive(int data_size) {

	if (!_r2I2C_should_run) { 
		
		printf("I2C Slave 0x%x operations was canceled using r2I2C_should_run(false).\n", _r2I2C_i2caddr);
		return R2I2C_SHOULD_NOT_RUN_ERROR;

	} else if (_r2I2C_is_busy) { 
		
		printf("I2C Slave 0x%x operations was is busy %s.\n", _r2I2C_i2caddr, _r2I2C_is_reading ? "reading" : "writing");
		return R2I2C_BUSY_ERROR;

	}

	_r2I2C_is_reading = true;
	_r2I2C_is_busy = true;

	uint8_t read_buff[1];

	int fd = r2I2C_open_bus();

	int status = R2I2C_OPERATION_OK;

	if (fd == R2I2C_BUS_ERROR) {
		
		// Open bus failed.

		_r2I2C_is_busy = false;
		return R2I2C_BUS_ERROR;
	} 

	if (data_size > R2I2C_MAX_BUFFER_SIZE) {

		// Caller requests too many bytes.

		printf("Maximum response size exceeded: %d bytes requested. Max: %d.\n", data_size, R2I2C_MAX_BUFFER_SIZE);
		status = R2I2C_READ_ERROR;

	} else { 

		// Wait for the R2I2C_READY_TO_READ_FLAG from slave prior to data fetching.

		read_buff[0] = 0x0;
		bool transmission_failed = false;

		if (R2I2C_USE_READY_FLAG) {

			do { 

				if (read(fd, read_buff, 1) != 1) { transmission_failed = true; break; }
			
				if (!_r2I2C_should_run) { break; }

				usleep(10 * 1000); 

			} while (read_buff[0] != R2I2C_READY_TO_READ_FLAG);

		}

		int i = 0;
		if (!transmission_failed) {

			if (read(fd, _r2I2C_dataBuffer, data_size) != data_size) { transmission_failed = true; }
			else { i = data_size; }
		}
	
		if (!_r2I2C_should_run) {
		
			// Operation canceled.
			for (i = 0; i < data_size; i++) {

				_r2I2C_dataBuffer[i] = 0;

			}
			
			status = R2I2C_OPERATION_CANCELED;

		} else if (transmission_failed || i != data_size) {

			// An error occured
			printf ("Could not read from I2C slave 0x%x. Error: %d. Expected %d bytes. Got %d bytes.\n", _r2I2C_i2caddr, errno, data_size, i);
			status = R2I2C_READ_ERROR;

		} 

	}


	close (fd);

	_r2I2C_is_busy = false;
	_r2I2C_is_reading = false;

	return status;

}

int r2I2C_send(uint8_t data[], int data_size) {

	if (!_r2I2C_should_run) { 
		
		printf("I2C Slave 0x%x operations was canceled using r2I2C_should_run(false).\n", _r2I2C_i2caddr);
		return R2I2C_SHOULD_NOT_RUN_ERROR;

	} else if (_r2I2C_is_busy) { 
		
		printf("I2C Slave 0x%x operations was is busy %s.\n", _r2I2C_i2caddr, _r2I2C_is_reading ? "reading" : "writing");
		return R2I2C_BUSY_ERROR;

	}

	_r2I2C_is_busy = true;

	int fd = r2I2C_open_bus();

	int status = R2I2C_OPERATION_OK;

	if (fd == R2I2C_BUS_ERROR) {
	
		// Open bus failed.

		_r2I2C_is_busy = false;
		return R2I2C_BUS_ERROR;
	
	} else if (write(fd, data, data_size) != data_size) {

		// Send message failed

		printf("Failed to write to I2C Slave 0x%x Error: %d.\n", _r2I2C_i2caddr, errno);
		
		status = R2I2C_WRITE_ERROR;
		
	} 

	close (fd);

	_r2I2C_is_busy = false;

	return status;

}

uint8_t* r2I2C_get_response() {

	return _r2I2C_dataBuffer;

}

void r2I2C_should_run(bool should_run) {

	_r2I2C_should_run = should_run;

}

bool r2I2C_is_ready() {

	return !_r2I2C_is_busy && _r2I2C_is_initialized;

}

int main(void)
{
	printf( "Raspberry Pi i2C test program\n" );
 	
	r2I2C_init (1, 0x04);
	uint8_t buff[4];
	buff[0] = 65;
	buff[1] = 66;
	buff[2] = 44;
	buff[3] = 45;
	//buff[2] = 44;
	
	int status = r2I2C_send(buff, 4);

	int r_size = 4;
	
	if (status == 0) {
		
		status = r2I2C_receive(r_size);

		if (status == 0) {
			int i = 0;
			uint8_t* r = r2I2C_get_response();
			for (i = 0; i < r_size; i++) {
				printf("Got response: %d\n", r[i]);			
			} 
			
		
		}
		//uint8_t[receive_size] resp = r2I2C_get_response();
		
		//

	}

	return status;

}

/*
#include <linux/i2c-dev.h>
#include <linux/i2c.h>

#include <unistd.h>
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <errno.h>
#include <string.h>
#include <fcntl.h>
#include <sys/ioctl.h>
#include <asm/ioctl.h>

int main( void )
{
	printf( "Raspberry Pi i2C test program\n" );
 	

  int file;
  int adapter_nr = 1; // probably dynamically determined 
  char filename[20];
  
  snprintf(filename, 19, "/dev/i2c-%d", adapter_nr);
  file = open(filename, O_RDWR);
  if (file < 0) {
	printf ("ARGH!");
    // ERROR HANDLING; you can check errno to see what went wrong
    exit(1);
  }
 

__u8 reg = 0x40; // Device register to access 
  __s32 res;
  char buf[10];

  // Using SMBus commands 
  res = i2c_smbus_read_word_data(file, reg);
  if (res < 0) {
    printf (" ERROR HANDLING: i2c transaction failed");
  } else {
    // res contains the read word 
  }

  // Using I2C Write, equivalent of 
     i2c_smbus_write_word_data(file, reg, 0x6543) 
  buf[0] = reg;
  buf[1] = 0x43;
  buf[2] = 0x65;
  if (write(file, buf, 3) != 3) {
    printf (" ERROR HANDLING: i2c transaction failed");
  }

  // Using I2C Read, equivalent of i2c_smbus_read_byte(file) 
  if (read(file, buf, 1) != 1) {
    printf ("ERROR HANDLING: i2c transaction failed 2");
  } else {
    // buf[0] contains the read byte 
  }
	return(0);
}
*/
