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

//initializes the server and alocate resources. returns 1 if successfull
int _ext_init(const char* remote_address, const char* remote_port, 
		const char *width, const char* height);
//start the server and dealocate resources
void _ext_stop();
//start the server and initiates the run loop (this method blocks and should thus be called from an external thread)
void _ext_start();
//sets the error-callback delegate
void _ext_set_report_error_callback (const char*(*report_error_callback)(int type, const char *message));
//returns true if the engine is running
bool _ext_is_running();
