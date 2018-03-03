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

#include <stdbool.h>
#include <inttypes.h>

#define R2I2C_BUS_ERROR -1
#define R2I2C_WRITE_ERROR -2
#define R2I2C_READ_ERROR -4

#define R2I2C_OPERATION_OK 0
#define R2I2C_OPERATION_CANCELED 1

// Returned if an receive/send operation has been commenced after should_run has been set to false. 
#define R2I2C_SHOULD_NOT_RUN_ERROR -8

// The receive/send operation was busy.
#define R2I2C_BUSY_ERROR -16

// Defines the maximum length of the read buffer.
#define R2I2C_MAX_BUFFER_SIZE 0xFF

// If true, the receive call expects the R2I2C_READY_TO_READ_FLAG to be read from slave prior to transmission.
#define R2I2C_USE_READY_FLAG false

// The I2C slave should begin every response with R2I2C_READY_TO_READ_FLAG, telling the receive operation that it's ready to receive data.
#define R2I2C_READY_TO_READ_FLAG 0xF0

// Initializes the bus and address variables. Will return the status of the bus request operation.
int r2I2C_init (int bus, int address);

// Requests data from slave. Returns 0 if successful. Will block until R2I2C_READY_TO_READ_FLAG has been received from slave.
// ´timeout´ is the the timeout in ms before a transmission fails.
int r2I2C_receive(long timeout);

// Returns the size of the last successfull transmission.
uint8_t r2I2C_get_response_size();

// Sends ´data_size´ bytes of ´data´ to slave. Returns 0 if successful.
int r2I2C_send(uint8_t data[], int data_size);

// Returns true if the bus is ready for operations. 
bool r2I2C_is_ready();

// If false, pending operations will not be executed.
void r2I2C_should_run(bool should_run);

// Will return the value of the array containing data. Will be populated with the latest data retrieved.
uint8_t r2I2C_get_response(int position);
