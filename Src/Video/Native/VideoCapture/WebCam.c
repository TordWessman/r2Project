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

#include "WebCam.h"
#include "WebCamGst.h"
#include <stdio.h>
#include <glib-2.0/glib-object.h>
#include <opencv/cv.h>
#include <opencv/highgui.h>
#include <opencv/cxcore.h>
//#include <opencv/cvaux.h>
//#include <iostream>
//#include <cstdio>
#include <gst/gst.h>
#include "gst/app/gstappsink.h"


//#define CV_NO_BACKWARD_COMPATIBILITY

//using namespace std;
//using namespace cv;

//void process_frame (IplImage *frame);

static bool is_reading = false, is_processing = false, pause_fetching = false;

/** Used by the frame fetcher: **/
static GstSample* sample;
static GstMapInfo* info;
static GstBuffer* buffer;
static GstCaps* buffer_caps;
static IplImage* tmp_frame;
static GstElement *appsink;

static IplImage* current_frame;

GstFlowReturn fetch_frame(GstAppSink *asink, gpointer notUsed );
void send_report(int type, const char* msg);

static const char*(*report_error)(int type, const char *message);	//delegate method for reporting back error


GstFlowReturn fetch_frame(GstAppSink *asink, gpointer notUsed ) {

	if (!is_reading && !is_processing && !pause_fetching) {

		is_reading = true;
		
		gint height, width;
		GstStructure* structure;
		
		if (sample) {
			gst_sample_unref(sample);
		}

		sample = gst_app_sink_pull_sample (asink);

		if (sample) {
			
			buffer = gst_sample_get_buffer(sample);
			
			/* if tmp_frame is null, create a new imag header by
			   extracting meta data from the caps
			*/
			if (!tmp_frame) {

				if (buffer_caps) {
					gst_caps_unref(buffer_caps);
				}
			
				buffer_caps = gst_sample_get_caps(sample);

				assert(gst_caps_get_size(buffer_caps) == 1);

				structure = gst_caps_get_structure(buffer_caps, 0);

				//Make sure width > 0 && height > 0
				assert (gst_structure_get_int(structure, "width", &width) && gst_structure_get_int(structure, "height", &height));
			
				int depth = 0;
				const gchar* name = gst_structure_get_name(structure);
				const gchar* format = gst_structure_get_string(structure, "format");

				if (!(name && format)) {
					g_critical ("Name and format could not be retrieved.");
					return GST_FLOW_CUSTOM_ERROR;					
				}
				  
				// we support 3 types of data:
				// video/x-raw, format=BGR -> 8bit, 3 channels
				// video/x-raw, format=GRAY8 -> 8bit, 1 channel
				// video/x-bayer -> 8bit, 1 channel
				// bayer data is never decoded, the user is responsible for that
				// everything is 8 bit, so we just test the caps for bit depth

				if (strcasecmp(name, "video/x-raw") == 0)
				{
				    if (strcasecmp(format, "BGR") == 0) {
					depth = 3;
				    }
				    else if(strcasecmp(format, "GRAY8") == 0){
					depth = 1;
				    }
				}
				else if (strcasecmp(name, "video/x-bayer") == 0)
				{
				    depth = 1;
				}

				if (depth < 1) {
					
					g_critical ("Caps not compatible: %s", format);
					return GST_FLOW_CUSTOM_ERROR;
				}

				tmp_frame = cvCreateImageHeader(cvSize(width, height), IPL_DEPTH_8U, depth);
			}

			assert (gst_buffer_map(buffer,info, (GstMapFlags)GST_MAP_READ));

			//now we have the frame:

			tmp_frame->imageData = (char*)info->data;

			//
			// Here be your image processing if you like to
			//
			// process_frame(tmp_frame);
			//

			gst_buffer_unmap(buffer,info);

		}

		is_reading = false;
	}
	
	return GST_FLOW_OK;
}



int init_opencv () {
	
	//initialize the variables used by OpenCv
	tmp_frame = NULL;
	buffer = NULL;
	buffer_caps = NULL;
	sample = NULL;
	info = calloc(sizeof(GstMapInfo), 1); //GST_MAP_INFO_INIT;//new GstMapInfo();
	appsink = get_appsink();

	if (!appsink) {
		g_critical ("Unable to fetch appsink. Object initialized?\n");
		return 0;
	}

	g_signal_connect(appsink, "new-sample", G_CALLBACK(fetch_frame), NULL);
	
	return true;
}

/**
* 	External methods:
**/

void _ext_resume_from_eos() {
	resume_from_eos ();
}


void _ext_set_callbacks (
	const char*(*report_error_callback)(int type, const char *message),
	const char*(*report_eos_callback)())
{
	report_error = report_error_callback;

	set_cb(report_error_callback, report_eos_callback);

}


int _ext_init(){
	if (!init_gst()) {
		g_critical ("Unable to init gstreamer\n");
		return false;
	} else if (!init_opencv()) {
		g_critical ("Unable to init opencv\n");
		return false;
	}
	
	return true;
}

void _ext_stop() {
	stop_gst_loop();
}

void _ext_start() {
	start_gst_loop();
}

void* _ext_get_frame() {
	if (is_processing) {
		send_report(kERROR_WARNING, "Unable to create dump. Processing in progress.\n");
	} else {
		is_processing = true;
		pause_fetching = true;
		while (is_reading) {/* "sleep" */ }
		
		if (current_frame) {
			cvReleaseImage(&current_frame);
		} else if (!tmp_frame) {
			g_critical ("First frame not yet initialized!");
			is_processing = false;
			return NULL;
		} else if ( tmp_frame->width != get_video_width() || tmp_frame->height != get_video_height()) {
			g_critical ("Input frame size mismatch. The input height/width (%i,%i) differs from expected (%i,%i)", tmp_frame->width,  tmp_frame->height , get_video_width(),get_video_height());
			is_processing = false;
			return NULL;
		}

		current_frame = cvCreateImage(cvGetSize(tmp_frame),
                           tmp_frame->depth,
                           tmp_frame->nChannels);		
		

		cvCopy(tmp_frame, current_frame, NULL);
		is_processing = false;
		pause_fetching = false;
		return current_frame;//ptr;
	}

	send_report(kERROR_WARNING, "Unable to retrieve frame.\n");

	return NULL;
}


void send_report(int type, const char* msg) {

	if (type == kERROR_WARNING) {
		g_warning ("%s", msg);
	} else {
		g_critical ("%s", msg);
	}

	if (report_error != NULL) {
		report_error(type , msg);
	}

}

int main (int argc, char** argv) {

	set_input_vars("640", "480");


	if (init_gst() &&
	    init_opencv()) {
		start_gst_loop ();
		stop_gst_loop();
	}
	else {
		printf ("unable to initialize");
		return -1;
	}
	
	return 0;
}
/**/

void _ext_dealloc() {
	dealloc();
}

bool _ext_is_running() {
	return is_running_gst_loop();
}

void _ext_set_input_vars (const char* width, const char* height) {
	set_input_vars(width, height);
}


int _ext_get_video_width() {
	return get_video_width();
}

int _ext_get_video_height() {
	return get_video_height();
}
