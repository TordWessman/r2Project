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
#include <sys/select.h>

//#define R2_PRINT_DEBUG

#ifdef R2_PRINT_DEBUG

#define R2_LOG(msg, ...) printf(msg, ##__VA_ARGS__)

#else

#define R2_LOG(msg, ...)

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

typedef struct i2c_read_result {

	uint8_t* data;
	int status;

} i2c_read;

static bool transmission_failed;

int r2I2C_open_bus ();

// reads the number of bytes defined by ´size´. Will timeout after ´timeout´ ms.
i2c_read r2I2C_read(int fd, long timout, size_t size);

int r2I2C_init (int bus, int address) {

	R2_LOG("%d Initializing R2I2C using bus `%d` and address `%d`\n", EINTR, bus, address);
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
	_r2I2C_should_run = true;

	R2_LOG("R2I2C initialization succeeded.\n");
	return R2I2C_OPERATION_OK;

}

bool fd_set_blocking(int fd, bool blocking) { int flags = fcntl(fd, F_GETFL, 0); if (flags == -1) { return 0; } if (blocking) { flags &= ~O_NONBLOCK; } else { flags |= O_NONBLOCK; } return fcntl(fd, F_SETFL, flags) != -1; }

int r2I2C_open_bus (int mode) {

	int fd;

	if ((fd = open(_r2I2C_busfile, mode)) < 0) {

		R2_LOG("Error: Couldn't open I2C Bus %d [r2I2C_open_bus():open %s]\n", _r2I2C_i2cbus, strerror(errno));
		return R2I2C_BUS_ERROR;

	} else if (ioctl(fd, I2C_SLAVE, _r2I2C_i2caddr) < 0) {

		R2_LOG("Error: I2C slave %d failed [r2I2C_open_bus():ioctl %s]\n", _r2I2C_i2caddr, strerror(errno));
		return R2I2C_BUS_ERROR;

	}

	//if (!fd_set_blocking(fd, true)) {
	//	R2_LOG("Vem är gud?");
	//}
	
	return fd;

}


 
static int recordTime = 0;

i2c_read r2I2C_read(int fd, long timeout, size_t size) {

	i2c_read response;
	response.data = NULL;
	response.status = R2I2C_OPERATION_OK;
	int timer = 0;
	
	// delay before retrying a read operation after a 5 or 121 error
	int delay = 50;

	//R2_LOG("Will try to read '%d' bytes.\n", size);

	do {

		if (timer > timeout) {

			response.status = R2I2C_READ_ERROR;
			R2_LOG("Error: Transmission failed due to timeout.\n");
			break;

		}

		if (!_r2I2C_should_run) { break; }

		uint8_t buffer[size];

		int bytesRead = read(fd, buffer, size);

		if (bytesRead == size) { 

			if (timer > recordTime) { recordTime = timer; }
			
			response.data = (uint8_t*)malloc(sizeof(uint8_t) * size);

			//R2_LOG("Successfully read all bytes after %d ms (record: %d).\n", timer, recordTime);
			for (int i = 0; i < size; i++) { response.data[i] = buffer[i]; } 
			//R2_LOG("\n");
			break;

		} else if (bytesRead > 1) { 

			R2_LOG("Error: read wrong number of bytes. Expected %d, but got %d\n", size, bytesRead); 
			response.status = R2I2C_READ_ERROR; 
			break; 

		} else if (bytesRead < 0) { 

			if (errno == 121 || errno == 5) {
	
				R2_LOG("[%d,%d]",errno,bytesRead);
				timer += delay;
				// sleep for 50 ms.			
				usleep(delay * 1000);

			} else {
				
				response.status = R2I2C_READ_ERROR;
				R2_LOG("Error: Transmission failed. Returned error: '%s' (code: %d). \n", strerror(errno), errno);
				break;
			
			}

		}

	} while (true);
	
	return response;

}


int r2I2C_receive(long timeout) {

	R2_LOG("Will try to read data from slave using timeout: '%ld'\n", timeout);

	if (!_r2I2C_should_run) {

		R2_LOG("Error: I2C Slave 0x%x operations was canceled using r2I2C_should_run(false).\n", _r2I2C_i2caddr);
		return R2I2C_SHOULD_NOT_RUN_ERROR;

	} else if (_r2I2C_is_busy) {

		R2_LOG("Error: I2C Slave 0x%x operations was is busy %s.\n", _r2I2C_i2caddr, _r2I2C_is_reading ? "reading" : "writing");
		return R2I2C_BUSY_ERROR;

	}

	_r2I2C_is_reading = true;
	_r2I2C_is_busy = true;
	_r2I2C_responseSize = 0;
	transmission_failed = false;

	int fd = r2I2C_open_bus(O_RDONLY);

	if (fd == R2I2C_BUS_ERROR) {

		// Open bus failed.
		_r2I2C_is_busy = false;
		return R2I2C_BUS_ERROR;

	}

	// read buffer
	i2c_read response;
	
	response.data = NULL;
	response.status = R2I2C_OPERATION_OK;

	
	int c = 0;

	if (_r2I2C_should_run) {

		int delay = 10;

		do {

			response = r2I2C_read(fd, timeout,1);
			
			if (response.status == R2I2C_OPERATION_OK && response.data && response.data[0] != R2I2C_READY_TO_READ_FLAG) {

				usleep(delay * 1000);
				c += delay;
		
			}

			if (response.data) {
				//R2_LOG("[%d]", response.data[0]);
			}

			if (c > timeout) { 
				R2_LOG("Error: Timeout. Receive failed. \n");
				response.status = R2I2C_READ_ERROR;
			}
			
		
		} while (_r2I2C_should_run && (response.status == R2I2C_OPERATION_OK) && response.data && (response.data[0] != R2I2C_READY_TO_READ_FLAG) && c <= timeout);
	}

	R2_LOG("\n");

	R2_LOG("Got status: %d, flag: %d, time: %d\n", response.status,  response.status == R2I2C_OPERATION_OK && response.data ? response.data[0] : -666, c );
	
	// Fetch response size:
	if (_r2I2C_should_run && response.status == R2I2C_OPERATION_OK) {

		response =  r2I2C_read(fd, timeout,1);
		_r2I2C_responseSize = (response.status != R2I2C_OPERATION_OK) ? 0 : response.data[0];

		if (response.data) { free(response.data); };

	}
	
	// Fetch the response
	if (_r2I2C_should_run && response.status == R2I2C_OPERATION_OK) {
		
		response = r2I2C_read(fd, timeout, _r2I2C_responseSize);
		
		if (response.status == R2I2C_OPERATION_OK) {

			for (int i = 0; i < _r2I2C_responseSize; i++) { _r2I2C_responseBuffer[i] = response.data[i]; }

		}
		
		if (response.data) { free(response.data); };

	}

	if (!_r2I2C_should_run) {

		// Operation canceled.
		for (int i = 0; i < _r2I2C_responseSize; i++) { _r2I2C_responseBuffer[i] = 0; }

		response.status = R2I2C_OPERATION_CANCELED;

	} 

	close (fd);

	_r2I2C_is_busy = false;
	_r2I2C_is_reading = false;

	return response.status;

}

int r2I2C_send(uint8_t data[], int data_size) {

	if (!_r2I2C_should_run) {

		R2_LOG("Error: I2C Slave 0x%x operations was canceled using r2I2C_should_run(false).\n", _r2I2C_i2caddr);
		return R2I2C_SHOULD_NOT_RUN_ERROR;

	} else if (_r2I2C_is_busy) {

		R2_LOG("Error: I2C Slave 0x%x operations was is busy %s.\n", _r2I2C_i2caddr, _r2I2C_is_reading ? "reading" : "writing");
		return R2I2C_BUSY_ERROR;

	}

	_r2I2C_is_busy = true;

	int fd = r2I2C_open_bus(O_WRONLY);

	// ehh clear the input buffer
	//uint8_t bajs[10];
	//int bajspappa = read(fd, bajs, 10);
	//while (bajspappa > 0) {bajspappa = read(fd, bajs, 10); printf("[%d %d]", bajspappa, bajs[0]);}

	int status = R2I2C_OPERATION_OK;

	if (fd == R2I2C_BUS_ERROR) {

		// Open bus failed.

		_r2I2C_is_busy = false;
		return R2I2C_BUS_ERROR;

	}

	R2_LOG(" -- Write size: %d. Writing bytes:\n", data_size);

	for (int i = 0; i < data_size; i++) {
		R2_LOG("[%d] ", data[i]);
	}

	R2_LOG("\n");
	int bytes_written = write(fd, data, data_size);

	if (bytes_written != data_size) {

		// Send message failed

		R2_LOG("Error: Failed to write to I2C Slave 0x%x. Bytes written: %d, Error: %s.\n", _r2I2C_i2caddr, bytes_written, errno == ENOMSG ? "<none>" : strerror(errno));

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

int sleepNode(uint8_t nodeId) {

	int count = 5;
        uint8_t buff[count];
	
	buff[0] = nodeId; // host
	buff[1] = 0x0A; // send to sleep: 0x0A;
	buff[2] = 0x0; // id
	buff[3] = 0x1; // arg1 
	buff[4] = 0xFF; //arg2

	int status = r2I2C_send(buff, count);

	if (status == 0) {
		
		return r2I2C_receive(16000);
	}

	return status;

}

int main(void)
{

	R2_LOG( "Raspberry Pi i2C test program: %d, %d\n", EAGAIN, EINTR );

	int status = r2I2C_init (1, 0x04);
	if (status < 0) {
		R2_LOG ("Error: Bad status: %d", status);
		return status;
	}

	//return sleepNode(0x3);

	int count = 3;
        uint8_t buff[count];
	
	buff[0] = 0x3; // host
	buff[1] = 0x0B; // check sleep state
	 //0x4; // action: init // send to sleep: 0x0A;
	buff[2] = 0x0; // id
	//buff[3] = 0x1; // arg1 
	//buff[4] = 0xFF; //arg2


    while (status == 0) {

	usleep(3000 * 1000);

	status = r2I2C_send(buff, count);

	if (status == 0) {
		
		//return 0;

		status = r2I2C_receive(16000);
		
		if (status == 0) {
	
			
			int i = 0;
			R2_LOG("Response size: %d.\nResponse data: \n", r2I2C_get_response_size());
			for (i = 0; i < r2I2C_get_response_size(); i++) {
				R2_LOG("[%d] ", r2I2C_get_response(i));
			}
			R2_LOG("\n");


		} else { break; }

	} else { break; }

    }

    R2_LOG("I died with status: %d", status);
    return status;

}

