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
static uint8_t _r2I2C_responseBuffer[R2I2C_MAX_BUFFER_SIZE];
static uint8_t _r2I2C_responseSize;
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

int r2I2C_receive() {

	if (!_r2I2C_should_run) { 
		
		printf("I2C Slave 0x%x operations was canceled using r2I2C_should_run(false).\n", _r2I2C_i2caddr);
		return R2I2C_SHOULD_NOT_RUN_ERROR;

	} else if (_r2I2C_is_busy) { 
		
		printf("I2C Slave 0x%x operations was is busy %s.\n", _r2I2C_i2caddr, _r2I2C_is_reading ? "reading" : "writing");
		return R2I2C_BUSY_ERROR;

	}

	_r2I2C_is_reading = true;
	_r2I2C_is_busy = true;
	_r2I2C_responseSize = 0;

	uint8_t read_buff[1];

	int fd = r2I2C_open_bus();

	int status = R2I2C_OPERATION_OK;

	if (fd == R2I2C_BUS_ERROR) {
		
		// Open bus failed.

		_r2I2C_is_busy = false;
		return R2I2C_BUS_ERROR;
	} 

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

		uint8_t response_size[1] = {0x0};
		
		// Read the size of the incomming data, which must be the first byte of the transaction.
		if (read(fd, response_size, 1) != 1) { transmission_failed = true; }
		else {
		
			_r2I2C_responseSize = response_size[0];

			//Read the rest of the data
			i = read(fd, _r2I2C_responseBuffer, _r2I2C_responseSize);
			if (i != _r2I2C_responseSize) { transmission_failed = true; }

		}
	}

	if (!_r2I2C_should_run) {
	
		// Operation canceled.
		for (i = 0; i < _r2I2C_responseSize; i++) {

			_r2I2C_responseBuffer[i] = 0;

		}
		
		status = R2I2C_OPERATION_CANCELED;

	} else if (transmission_failed || i != _r2I2C_responseSize) {

		// An error occured
		printf ("Could not read from I2C slave 0x%x. Error: %d. Got %d bytes. Expected: %d bytes.\n", _r2I2C_i2caddr, errno, i, _r2I2C_responseSize);
		status = R2I2C_READ_ERROR;

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

uint8_t r2I2C_get_response_size() {

	return _r2I2C_responseSize;

}

uint8_t* r2I2C_get_response() {

	return _r2I2C_rBuffer;

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
	
	int status = r2I2C_send(buff);

	if (status == 0) {
		
		status = r2I2C_receive();

		if (status == 0) {
			int i = 0;
			uint8_t* r = r2I2C_get_response();
			for (i = 0; i < r2I2C_get_response_size(); i++) {
				printf("Got response: %d\n", r[i]);			
			} 
			
		
		}

	}

	return status;

}
