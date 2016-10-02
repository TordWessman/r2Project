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
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <gst/gst.h>
#include "gst/app/gstappsink.h"
#include "VideoSource.h"

#define kTURN_OFF_BUS_MESSAGE "turn-off"
#define kERROR_WARNING 0
#define kERROR_CRITICAL 1

static GMainLoop *loop;

static GstElement *pipeline, 	 		// the pipeline for the bin
		*src,			// source
		*csp,
		*jpegenc,
		*rtpjpegpay,
		*sink;

static GstCaps *udp_caps;

static GstBus *bus;	//the bus element te transport messages from/to the pipeline

static int is_running;

// UDP-data:

static const char *ip, *port, *width, *height;

static gboolean bus_call(GstBus *bus, GstMessage *msg, void *user_data);
static const char*(*report_error)(int type, const char *message);	//delegate method for reporting back error

int init_gst(const char* remote_address, const char* local_port, const char *out_width, const char* out_height) {


	//UDP-init:
	ip = strdup (remote_address);
	port = strdup (local_port);
	width = strdup (out_width);
	height = strdup (out_height);

	gst_init (NULL, NULL);

	GstCaps *caps;

	/* create the main loop */
	loop = g_main_loop_new(NULL, FALSE);

	pipeline = gst_pipeline_new ("video_pipeline");

	/* create the bus for the pipeline */
	bus = gst_pipeline_get_bus(GST_PIPELINE(pipeline));

	/* add the bus handler method */
	gst_bus_add_watch(bus, bus_call, NULL);

	gst_object_unref(bus);

	//initializing elements
	src = gst_element_factory_make ("v4l2src", "src");
	rtpjpegpay =  gst_element_factory_make ("rtpjpegpay", "rtpjpegpay0");
	sink = gst_element_factory_make ("udpsink", "udpsink0");
	csp = gst_element_factory_make("videoconvert", "csp");
	jpegenc = gst_element_factory_make ("jpegenc", "jpegenc0");

	if (src == NULL || sink == NULL) {
		g_critical ("Unable to create src/sink elements.");
		return 0;
	} else 	if (!csp) {
		g_critical ("Unable to create csp");
		return 0;
	} else if (!jpegenc || !rtpjpegpay) {
		g_critical ("Unable to create other elements");
		return 0;
	} 


	//Set up the Sink:

	g_object_set(G_OBJECT(sink), "port", atoi(port), NULL);
	g_object_set(G_OBJECT(sink), "host", ip, NULL);
	g_object_set(G_OBJECT(sink), "auto_multicast", true, NULL);
	g_object_set(G_OBJECT(sink), "sync", false, NULL);

	//Set up queue:
	//g_object_set(G_OBJECT(queue), "max_size_buffers", 2, NULL);
	//g_object_set(G_OBJECT(queue), "leaky", true, NULL);

	//set up quality
	g_object_set(G_OBJECT(jpegenc), "quality", 90, NULL);

	//Set up rtpvrawpay:
	g_object_set(G_OBJECT(rtpjpegpay), "ssrc", 1, NULL);
	g_object_set(G_OBJECT(rtpjpegpay), "timestamp_offset", 0, NULL);
	g_object_set(G_OBJECT(rtpjpegpay), "seqnum_offset", 0, NULL);

	/* Add the elements to the pipeline prior to linking them */	

	gst_bin_add_many(GST_BIN(pipeline), src, csp, jpegenc, rtpjpegpay, sink, NULL);

	/* Link the camera source and colorspace filter using capabilities
	 * specified */


	if(!gst_element_link_many(src, csp, NULL))
	{
		gst_object_unref (pipeline);
		g_critical ("Unable to link src to csp ");
		return 0;
	}


	caps = gst_caps_new_simple("video/x-raw",
		"width", G_TYPE_INT, atoi(width),
		"height", G_TYPE_INT, atoi(height),
		//"interlaced", G_TYPE_BOOLEAN, false,
		//"framerate",  GST_TYPE_FRACTION, 10, 1,
		NULL);


	if(!gst_element_link_filtered(csp, jpegenc, caps))
	{
		gst_object_unref (pipeline);
		g_critical ("Unable to link csp to queue. check your caps.");
		return 0;
	}

	if(!gst_element_link_many(jpegenc, rtpjpegpay, sink, NULL))
	{
		gst_object_unref (pipeline);
		g_critical ("Unable to link queue, rtprawpay, sink");
		return 0;
	}
	

	printf ("Server initialization successfull!\n");
	return 1;
}


void start_gst_loop() {

	is_running = 1;
	
	gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_PLAYING);
	
	g_main_loop_run(loop);
	
	gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_NULL);
	gst_object_unref(GST_OBJECT(pipeline));
	g_main_loop_unref (loop);
	is_running = 0;
}

//stop the main loop and dealocate resources

void stop_gst_loop() {

	GValue text_gv = {0};
	g_value_init (&text_gv, G_TYPE_STRING);
	g_value_set_string(&text_gv, "turn_off");

	//creates message structure for the partial result:
	GstStructure *messageStruct = gst_structure_new_empty (kTURN_OFF_BUS_MESSAGE);
	//set the hypothesis (the guessed text)
	gst_structure_set_value (messageStruct, "hyp", &text_gv);	

	//post message to the pipeline bus
	
	if (gst_bus_post(
		gst_element_get_bus(src), 
		gst_message_new_application((GstObject *) src, messageStruct)))
	{
		//TODO: something if post failed...
	}
		//unref elements
	
	printf("---------------------------------turned off video server.");

}

int is_running_gst_loop () {
	return is_running;
}


static gboolean bus_call(GstBus *bus, GstMessage *msg, void *user_data)
{

	//printf ("hahahah");
	switch (GST_MESSAGE_TYPE(msg)) {
	case GST_MESSAGE_EOS: {
		//g_message("End-of-stream");
		g_main_loop_quit(loop);
		break;
	}
	case GST_MESSAGE_ERROR: {
		GError *err;
		gst_message_parse_error(msg, &err, NULL);
		
		if (report_error != NULL) {
			report_error(kERROR_CRITICAL , "VideoSource encountered an error and will stop:");
		}

		g_critical ("ERROR: %s", err->message);
		g_error_free(err);
		
		g_main_loop_quit(loop);
		
		break;
	} 
	case GST_MESSAGE_APPLICATION: {

		const GstStructure *str;
		str = gst_message_get_structure (msg);
		 if (gst_structure_has_name(str, kTURN_OFF_BUS_MESSAGE))
			{
				g_main_loop_quit(loop);
			}

		break;
	}
	default:
	
		break;
	}
		if (msg->type == GST_MESSAGE_STATE_CHANGED ) {
			GstState old, news, pending;
     			gst_message_parse_state_changed (msg, &old, &news, &pending);
			//printf ("State changed. Old: %i New: %i Pending: %i.\n", old, news, pending); 
		} else {
			//printf("info: %i %s type: %i\n", (int)(msg->timestamp), GST_MESSAGE_TYPE_NAME (msg), msg->type);
		}

	return true;
}




int main (int argc, char** argv) {

	
	if (init_gst("192.168.0.19", "5005" , "640" , "480")) {
		start_gst_loop ();
		//stop_gst_loop();
	}
	else {
		printf ("unable to initialize");
		return -1;
	}
	
	return 0;
}
/**/


/**
* 	External methods:
**/
void _ext_set_report_error_callback (const char*(*report_error_callback)(int type, const char *message))
{
	report_error = report_error_callback;
}


int _ext_init(const char* remote_address, const char* local_port, const char *out_width, const char* out_height){
	if (!init_gst(remote_address, local_port, out_width, out_height)) {
		g_critical ("Unable to init gstreamer\n");
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

bool _ext_is_running() {
	return is_running_gst_loop();
}
