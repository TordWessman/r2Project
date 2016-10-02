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

#include <gst/gst.h>

#define kERROR_WARNING 0
#define kERROR_CRITICAL 1

int init_gst();

GstCaps* get_udpsrc_caps();
void start_gst_loop();
void stop_gst_loop();
GstElement* get_appsink();
int is_running_gst_loop ();
void set_cb(const char*(*report_error_callback)(int type, const char *message),
			const char*(*report_eos_callback)());

void set_input_vars (const char* remote_address, int remote_port, const char *local_ip,
		     const char* width, const char* height);
void set_output_vars (const char* remote_address, const char* remote_port,
		     const char* width, const char* height);

int get_video_width();
int get_video_height();

void dealloc ();
void resume_from_eos ();
