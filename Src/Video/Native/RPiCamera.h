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

void _ext_rpi_start();
void _ext_rpi_stop();
int _ext_rpi_setup();
void _ext_rpi_init(int width, int height, int port);
void _ext_rpi_init_extended(int width, int height, int port, int bitrate, int framerate);

int _ext_rpi_get_framerate();
int _ext_rpi_get_width();
int _ext_rpi_get_height();
bool _ext_rpi_get_initiated();
bool _ext_rpi_get_recording();

