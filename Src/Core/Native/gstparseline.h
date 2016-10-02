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

/**
*	General pipeline created as a parsable element.	
**/

#include <gst/gst.h>
#include <stdbool.h>

struct PLC {

	GMainLoop *loop;

	GstElement *pipeline;

	GstBus *bus;	//the bus element te transport messages from/to the pipeline

	int is_playing; //indicates that the player is busy

	int is_stopped;
	int is_initialized;

	const char* pipelineString;
	
};

void* _ext_create_gstream(const char* pipelineString);
int _ext_destroy_gstream(struct PLC* plc);
int _ext_init_gstream(struct PLC* plc);
int _ext_stop_gstream(struct PLC* plc);
int _ext_play_gstream(struct PLC* plc);
int _ext_is_playing_gstream(struct PLC* plc);
int _ext_is_initialized_gstream (struct PLC* plc);

