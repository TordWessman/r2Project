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
#include <time.h>
#include <stdlib.h>
#include <math.h>

#define R2_PRINT_DEBUG

#ifdef R2_PRINT_DEBUG

#define R2_LOG(msg) (printf(msg))
#define R2_LOG1(msg,p1) (printf(msg,p1))
#define R2_LOG2(msg,p1,p2) (printf(msg,p1,p2))
#define R2_LOG3(msg,p1,p2,p3) (printf(msg,p1,p2,p3))

#else

#define R2_LOG(msg) (msg == 0)
#define R2_LOG1(msg,p1) (msg == 0)
#define R2_LOG2(msg,p1,p2) (msg == 0)
#define R2_LOG3(msg,p1,p2,p3) (msg == 0)
#endif

static int _r2I2C_i2caddr;
static int _r2I2C_i2cbus;
static char _r2I2C_busfile[64];
static uint8_t _r2I2C_responseBuffer[R2I2C_MAX_BUFFER_SIZE];
static uint8_t _r2I2C_responseSize;
static bool _r2I2C_should_run = true;
static bool _r2I2C_is_busy = false;
static bool _r2I2C_is_initialized = false;
static bool _r2I2C_is_reading = false;

static bool transmission_failed;

int r2I2C_open_bus ();

// reads the number of bytes defined by ´size´. Will timeout after ´timeout´ ms.
uint8_t* r2I2C_read(int fd, long timout, int size);

int r2I2C_init (int bus, int address) {

	R2_LOG2("Initializing R2I2C using bus `%d` and address `%d`\n", bus, address);
	usleep(1000 * 1000);
	_r2I2C_i2cbus = bus;
	_r2I2C_i2caddr = address;
	snprintf(_r2I2C_busfile, sizeof(_r2I2C_busfile), "/dev/i2c-%d", bus);

	int fd = r2I2C_open_bus();

	if (fd < 0) {

		R2_LOG("Error: r2I2C_open_bus() failed.\n");
		_r2I2C_is_initialized = false;
		return R2I2C_BUS_ERROR;

	}

	_r2I2C_is_initialized = true;

	R2_LOG("R2I2C initialization succeeded.\n");
	return fd;

}

int r2I2C_open_bus () {

	int fd;

	if ((fd = open(_r2I2C_busfile, O_RDWR)) < 0) {

		R2_LOG2 ("Error: Couldn't open I2C Bus %d [r2I2C_open_bus():open %s]\n", _r2I2C_i2cbus, strerror(errno));
		return R2I2C_BUS_ERROR;

	} else if (ioctl(fd, I2C_SLAVE, _r2I2C_i2caddr) < 0) {

		R2_LOG2 ("Error: I2C slave %d failed [r2I2C_open_bus():ioctl %s]\n", _r2I2C_i2caddr, strerror(errno));
		return R2I2C_BUS_ERROR;

	}

	usleep(50000);

	return fd;

}

uint8_t* r2I2C_read(int fd, long timeout, int size) {

	R2_LOG1("Will try to read '%d' bytes: ", size);

	uint8_t* response = (uint8_t*)malloc(sizeof(uint8_t) * size);

	// instantiate the timeout
	struct timespec timeSpec;
	clock_gettime(CLOCK_REALTIME, &timeSpec); 
	long timer = round(timeSpec.tv_nsec / 1.0e6);

	do {
		
		clock_gettime(CLOCK_REALTIME, &timeSpec);

		if (timer + timeout < round(timeSpec.tv_nsec / 1.0e6) ) {

			transmission_failed = true;
			R2_LOG("Error: Transmission failed due to timeout.\n");
			break;

		}

		if (!_r2I2C_should_run) { break; }

		uint8_t buffer[size];

		int bytesRead = read(fd, buffer, size);

		if (bytesRead == size) { 

			R2_LOG("Successfully read all bytes.");
			for (int i = 0; i < size; i++) { response[i] = buffer[i]; } 
			break;

		} else if (bytesRead > 1) { 

			R2_LOG2("Error: read wrong number of bytes. Expected %d, but got %d\n", size, bytesRead); 
			transmission_failed = true; 
			break; 

		} else if (bytesRead < 0) { 

			if (errno == 121 || errno == 5) {
	
				// sleep for 50 ms.			
				usleep(50 * 1000);

			} else {
				
				transmission_failed = true;
				R2_LOG2("Error: Transmission failed. Returned error: '%s' (code: %d). \n", strerror(errno), errno);
				break;
			
			}

		}

	} while (true);
	
	return response;

}
 
int r2I2C_receive(long timeout) {

	R2_LOG1("Will try to read data from slave using timeout: '%ld'\n", timeout);

	if (!_r2I2C_should_run) {

		R2_LOG1("Error: I2C Slave 0x%x operations was canceled using r2I2C_should_run(false).\n", _r2I2C_i2caddr);
		return R2I2C_SHOULD_NOT_RUN_ERROR;

	} else if (_r2I2C_is_busy) {

		R2_LOG2("Error: I2C Slave 0x%x operations was is busy %s.\n", _r2I2C_i2caddr, _r2I2C_is_reading ? "reading" : "writing");
		return R2I2C_BUSY_ERROR;

	}

	_r2I2C_is_reading = true;
	_r2I2C_is_busy = true;
	_r2I2C_responseSize = 0;
	transmission_failed = false;

	int fd = r2I2C_open_bus();

	int status = R2I2C_OPERATION_OK;

	if (fd == R2I2C_BUS_ERROR) {

		// Open bus failed.
		_r2I2C_is_busy = false;
		return R2I2C_BUS_ERROR;

	}

	// read buffer
	uint8_t* buffer;

	// Wait for the R2I2C_READY_TO_READ_FLAG from slave prior to data fetching.
	if (R2I2C_USE_READY_FLAG) {
		
		buffer = r2I2C_read(fd, timeout, 1);

		if (!transmission_failed && buffer[0] != R2I2C_READY_TO_READ_FLAG) {

			transmission_failed = true;
			R2_LOG1("Error: Transmission failed. Did not receive the expected R2I2C_READY_TO_READ_FLAG. Instead i got: %d \n", buffer[0]);

		} else { status = R2I2C_READ_ERROR; }

		free(buffer);

	}

	if (!transmission_failed) {

		buffer =  r2I2C_read(fd, timeout,1);
		_r2I2C_responseSize = buffer[0];
		free(buffer);
	} else { status = R2I2C_READ_ERROR; }
	
	if (!transmission_failed) {
		
		R2_LOG1("Will now read response with size: %d\n",  _r2I2C_responseSize);
		buffer = r2I2C_read(fd, timeout, _r2I2C_responseSize);
		for (int i = 0; i < _r2I2C_responseSize; i++) { _r2I2C_responseBuffer[i] = buffer[i]; }
		free(buffer);

	} else { status = R2I2C_READ_ERROR; }

	if (!_r2I2C_should_run) {

		// Operation canceled.
		for (int i = 0; i < _r2I2C_responseSize; i++) { _r2I2C_responseBuffer[i] = 0; }

		status = R2I2C_OPERATION_CANCELED;

	} 

	close (fd);

	_r2I2C_is_busy = false;
	_r2I2C_is_reading = false;

	return status;

}

int r2I2C_send(uint8_t data[], int data_size) {

	if (!_r2I2C_should_run) {

		R2_LOG1("Error: I2C Slave 0x%x operations was canceled using r2I2C_should_run(false).\n", _r2I2C_i2caddr);
		return R2I2C_SHOULD_NOT_RUN_ERROR;

	} else if (_r2I2C_is_busy) {

		R2_LOG2("Error: I2C Slave 0x%x operations was is busy %s.\n", _r2I2C_i2caddr, _r2I2C_is_reading ? "reading" : "writing");
		return R2I2C_BUSY_ERROR;

	}

	_r2I2C_is_busy = true;

	int fd = r2I2C_open_bus();

	int status = R2I2C_OPERATION_OK;

	if (fd == R2I2C_BUS_ERROR) {

		// Open bus failed.

		_r2I2C_is_busy = false;
		return R2I2C_BUS_ERROR;

	}

	R2_LOG1("Got file descriptor: %d", fd);

	R2_LOG1(" -- Write size: %d. Writing bytes:\n", data_size);

	for (int i = 0; i < data_size; i++) {
		R2_LOG1("[%d] ", data[i]);
	}

	R2_LOG("\n");
	int bytes_written = write(fd, data, data_size);

	if (bytes_written != data_size) {

		// Send message failed

		R2_LOG3("Error: Failed to write to I2C Slave 0x%x. Bytes written: %d, Error: %s.\n", _r2I2C_i2caddr, bytes_written, errno == ENOMSG ? "<none>" : strerror(errno));

		status = R2I2C_WRITE_ERROR;

	}

	close (fd);

	_r2I2C_is_busy = false;

	return status;

}

uint8_t r2I2C_get_response_size() {

	return _r2I2C_responseSize;

}

uint8_t r2I2C_get_response(int position) {

	return _r2I2C_responseBuffer[position];

}

void r2I2C_should_run(bool should_run) {

	_r2I2C_should_run = should_run;

}

bool r2I2C_is_ready() {

	return !_r2I2C_is_busy && _r2I2C_is_initialized;

}

int main(void)
{

	R2_LOG( "Raspberry Pi i2C test program\n" );

	int status = r2I2C_init (1, 0x04);
	if (status < 0) {
		R2_LOG1 ("Error: Bad status: %d", status);
		return status;
	}

	int count = 5;
        uint8_t buff[5];
	
	buff[0] = 0x0; // host
	buff[1] = 0x4; // action: init
	buff[2] = 0x0; // id
	buff[3] = 0x0; //arg1 
	buff[4] = 0x0; //arg2

	status = r2I2C_send(buff, count);

	if (status == 0) {
		
		//return 0;

		status = r2I2C_receive(2000);

		if (status == 0) {
			int i = 0;
			R2_LOG1("Response size: %d.\nResponse data: \n", r2I2C_get_response_size());
			for (i = 0; i < r2I2C_get_response_size(); i++) {
				R2_LOG1("[%d] ", r2I2C_get_response(i));
			}
			R2_LOG("\n");


		}

	}

	return status;

}

