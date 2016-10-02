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

#include "WebCamGst.h"
#include <stdbool.h>
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include "gst/app/gstappsink.h"

#define kTURN_OFF_BUS_MESSAGE "turn-off"
#define DEFAULT_PORT "5005"
#define DEFAULT_PREFIX "udp://"

#define kUSE_APP_SINK
//#define kUSE_phone_sink
//#define kUSE_DEBUG_SINK
//#define kUSE_TEST_SINK

#if (defined(kUSE_TEST_SINK) && defined(kUSE_phone_sink))
#error "EN SINK DUMMER!"
#endif
static GMainLoop *loop;

static GstElement
		*pipeline, 	 		// the pipeline for the bin
		*src,			// source
		*csp_src, *csp_out, *csp_app, *csp_rtp, *csp_xvimg,
		*tee,
		*queue_out, // queue between tee and
		*queue_app, 
		*queue_rtp, 
		*queue_xvimg,
		*appsink,
		*jpegenc,
		*videorate, *videoscale,
		*xvimagesink, //for debugging
		*phone_sink,
		*test_sink;

static GstBus *bus;	//the bus element te transport messages from/to the pipeline

static int is_running;

// input data:

static const char *input_width;
static const char *input_height;
static bool input_is_set;

// Client Data

static const char *client_ip;
static const char *client_port;
static const char *output_width;
static const char *output_height;
static bool output_is_set;

static gboolean bus_call(GstBus *bus, GstMessage *msg, void *user_data);

static const char*(*report_error)(int type, const char *message);	//delegate method for reporting back error
static const char*(*report_eos)();	//delegate method for reporting eos

void set_cb(const char*(*report_error_callback)(int type, const char *message),
			const char*(*report_eos_callback)()) {
	report_error = report_error_callback;
	report_eos = report_eos_callback;
}



void set_input_vars (const char* width, const char* height) {

		input_width = strdup (width);
	input_height = strdup (height);
	input_is_set = true;


} 

void set_output_vars (const char* remote_address, const char* remote_port,
		     const char* width, const char* height) {

	client_ip = strdup (remote_address);
	client_port = strdup (remote_port);
	output_width = strdup (width);
	output_height = strdup (height);
	output_is_set = true;
} 

int init_gst() {

	gst_init (NULL, NULL);

	GstCaps *caps_app, *caps_in;
	
	if (!input_is_set) {
		g_critical ("Input variables not set!\n");
		return 0;
	} 
#ifdef kUSE_phone_sink
	if (!output_is_set) {
		g_critical ("Output variables not set!\n");
		return 0;
	}
	GstCaps *caps_phone;
#endif


#ifdef kUSE_TEST_SINK		
#endif

#if defined(kUSE_phone_sink) || defined(kUSE_TEST_SINK)
	GstCaps *caps_out;
#endif
		
//#ifdef kUSE_phone_sink
		
//#endif
#ifndef kUSE_V4L2SRC
		
#endif


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

	xvimagesink = gst_element_factory_make ("xvimagesink", "fpsdisplaysink"); 
	// xvimagesink = gst_element_factory_make ("fpsdisplaysink", "fpsdisplaysink"); 
	//xvimagesink = gst_element_factory_make ("fakesink", "xvimagesink");
	phone_sink = gst_element_factory_make ("tcpserversink", "phone_sink");
	appsink = gst_element_factory_make ("appsink", "appsink");
	csp_src = gst_element_factory_make("videoconvert", "csp_src");
	csp_out = gst_element_factory_make("videoconvert", "csp_out");
	csp_app = gst_element_factory_make("videoconvert", "csp_app");
	csp_rtp = gst_element_factory_make("videoconvert", "csp_rtp");
	csp_xvimg = gst_element_factory_make("videoconvert", "csp_xvimg");
	tee = gst_element_factory_make ("tee", "videotee");
	queue_out = gst_element_factory_make ("queue", "queue_out");
	queue_app = gst_element_factory_make ("queue", "queue_app");
	queue_rtp = gst_element_factory_make ("queue", "queue_rtp");
	queue_xvimg = gst_element_factory_make ("queue", "queue_xvimg");
	videorate = gst_element_factory_make ("videorate", "videorate");
	videoscale = gst_element_factory_make ("videoscale", "videoscale");
	jpegenc = gst_element_factory_make ("jpegenc", "jpegenc");
	
	test_sink = gst_element_factory_make ("udpsink", "testsink");


	if (src == NULL || phone_sink == NULL || appsink == NULL) {
		g_critical ("Unable to create src/sink elements.");
		return 0;
	} else 	if (!csp_out || !csp_src || !csp_app) {
		g_critical ("Unable to create csps");
		return 0;
	} else if (!queue_out || !queue_app || !queue_rtp || !tee || !appsink || !jpegenc || !videorate || !videoscale) {
		g_critical ("Unable to create other elements");
		return 0;
	} 
	 else if (!queue_xvimg || !csp_xvimg || !xvimagesink) {
		g_critical ("Unable to create debug elements");
		return 0;
	}

	


	// set up elements: ------ 
	//Set up the udpsrc:

	// Make sure the "new-sample" signal is emitted
	gst_app_sink_set_emit_signals((GstAppSink*)appsink, true);
	// Tell the app sink to drop samples when the internal queue is full
	gst_app_sink_set_drop((GstAppSink*)appsink, true);
	// Set the internal queue to one element
	gst_app_sink_set_max_buffers((GstAppSink*)appsink, 1);

	
	//set up video decoding
	g_object_set(G_OBJECT(jpegenc), "quality", 10, NULL);
	g_object_set(G_OBJECT(videorate), "max_rate", 2, NULL);


	/* Add the elements to the pipeline prior to linking them */	

	//gst_bin_add_many(GST_BIN(pipeline), src, rtpdepay , csp_src, tee, queue_out, csp_out, sink, queue_app, csp_app, appsink, queue_rtp, csp_rtp, videorate, videoscale, xvimagesink, csp_xvimg, queue_xvimg, NULL);
	gst_bin_add_many(GST_BIN(pipeline), src, csp_src, tee, 

#ifdef kUSE_DEBUG_SINK
			xvimagesink, csp_xvimg, queue_xvimg, 
#endif
#ifdef kUSE_APP_SINK
			queue_app, csp_app, appsink,
#endif
#ifdef kUSE_phone_sink
			jpegenc, queue_out, csp_out, queue_rtp, csp_rtp, videoscale, phone_sink,
#endif
#ifdef kUSE_TEST_SINK
			
			queue_out, csp_out, videoscale , jpegenc, test_sink,
#endif
//phone_sink,
			NULL);



	/* Specify caps for the csp-filters (modify these if you require).
	   Currently, the first videoconvert (csp) changes resolution. 
	*/

	caps_in = gst_caps_new_simple("video/x-raw",
		"width", G_TYPE_INT, atoi(input_width),
		"height", G_TYPE_INT, atoi(input_height),
		//"interlaced", G_TYPE_BOOLEAN, false,
		//"framerate",  GST_TYPE_FRACTION, 10, 1,
		NULL);


#if defined(kUSE_phone_sink) || defined(kUSE_TEST_SINK)
	// Does nothing really... 
	caps_out = gst_caps_new_simple("video/x-raw", 
				"format", G_TYPE_STRING, "YUY2",
				"width", G_TYPE_INT, atoi(output_width),
				"height", G_TYPE_INT, atoi(output_height),
				NULL);
#endif
	

#ifdef kUSE_phone_sink
	// Caps for scaling before rtpvrawdepay
	caps_phone = gst_caps_new_simple("video/x-raw",
			"width", G_TYPE_INT, atoi(output_width),
			"height", G_TYPE_INT, atoi(output_height),
			NULL);

		/* Set the Output queue */
	g_object_set(G_OBJECT(queue_rtp), "max_size_buffers", 2, NULL);
	g_object_set(G_OBJECT(queue_rtp), "leaky", true, NULL);

	/* Set up the udpsink */
	g_object_set(G_OBJECT(phone_sink), "port", atoi(client_port), NULL);
	g_object_set(G_OBJECT(phone_sink), "host", client_ip, NULL);

#endif
	/* OpenCV requires BGR format */
	caps_app = gst_caps_new_simple("video/x-raw",
			"format", G_TYPE_STRING, "BGR",
			NULL);



	
	/* Link the camera source and colorspace filter using capabilities
	 * specified */

	if(!gst_element_link_filtered(src, csp_src, caps_in))
	{
		gst_object_unref (pipeline);
		g_critical ("Unable to link src");
		return 0;
	}

	if(!gst_element_link_many(csp_src, tee, NULL))
	{
		gst_object_unref (pipeline);
		g_critical ("Unable to linkcsp_src to tee ");
		return 0;
	}

	
	// Link the udp-sink
#ifdef kUSE_phone_sink	
	if(!gst_element_link_filtered(queue_out, csp_out, caps_out))
	{
		gst_object_unref (pipeline);
		g_critical ("Unable to link first for the queue 1");
		return 0;
	}

	if(!gst_element_link_many(csp_out, videoscale, NULL))
	{
		gst_object_unref (pipeline);
		g_critical ("Unable to link second. for queue1");
		return 0;
	}

	if(!gst_element_link_filtered(videoscale, jpegenc, caps_phone))
	{
		gst_object_unref (pipeline);
		g_critical ("Unable to link third. check your caps.");
		return 0;
	}

	if(!gst_element_link_many( jpegenc, phone_sink,NULL))
	{
		gst_object_unref (pipeline);
		g_critical ("Unable to link middle. for queue1");
		return 0;
	}

#endif
#ifdef kUSE_TEST_SINK
	if(!gst_element_link_filtered(queue_out, csp_out, caps_out))
	{
		gst_object_unref (pipeline);
		g_critical ("TEST: Unable to link first for the queue 1");
		return 0;
	}

	if(!gst_element_link_many(csp_out, videoscale, jpegenc, test_sink,NULL))
	{
		gst_object_unref (pipeline);
		g_critical ("TEST: Unable to link middle. for queue1");
		return 0;
	}

#endif

#ifdef kUSE_APP_SINK
	// Link the app-sink

	if(!gst_element_link_many(queue_app, csp_app, NULL))
	{
		gst_object_unref (pipeline);
		g_critical ("Unable to link queue_app to csp");
		return 0;
	}

	if(!gst_element_link_filtered(csp_app, appsink, caps_app))
	{
		gst_object_unref (pipeline);
		g_critical ("Unable to link csp_app to appsink. check your caps.");
		return 0;
	}
#endif

#ifdef kUSE_DEBUG_SINK	
	/* Link the (debug) xvimagesink */

	if(!gst_element_link_many(queue_xvimg, csp_xvimg,xvimagesink, NULL))
	{
		gst_object_unref (pipeline);
		g_critical ("Unable to link queue_xvimg to csp");
		return 0;
	}

#endif
	/* Manually link the Tee, which has "Request" pads */

	GstPadTemplate *tee_src_pad_template;

	if ( !(tee_src_pad_template = gst_element_class_get_pad_template (GST_ELEMENT_GET_CLASS (tee), "src_%u"))) {
		gst_object_unref (pipeline);
		g_critical ("Unable to get pad template");
		return 0;		
	}
	
#if defined(kUSE_phone_sink) || defined(kUSE_TEST_SINK)
	GstPad *tee_queue_out_pad, *queue_out_pad;

	tee_queue_out_pad = gst_element_request_pad (tee, tee_src_pad_template, NULL, NULL);
	g_print ("Obtained request pad %s for queue_out branch.\n", gst_pad_get_name (tee_queue_out_pad));
	queue_out_pad = gst_element_get_static_pad (queue_out, "sink");

	if (gst_pad_link (tee_queue_out_pad, queue_out_pad) != GST_PAD_LINK_OK ){
	
		g_critical ("Tee for queue_out could not be linked.\n");
		gst_object_unref (pipeline);
		return 0;

	}
	
	gst_object_unref (queue_out_pad); 
#endif

	


#ifdef kUSE_APP_SINK
	
	GstPad  *tee_queue_app_pad, *queue_app_pad;

	tee_queue_app_pad = gst_element_request_pad (tee, tee_src_pad_template, NULL, NULL);
	g_print ("Obtained request pad %s for queue_app branch.\n", gst_pad_get_name (tee_queue_app_pad));
	queue_app_pad = gst_element_get_static_pad (queue_app, "sink");

	if (gst_pad_link (tee_queue_app_pad, queue_app_pad) != GST_PAD_LINK_OK) {

		g_critical ("Tee for queue_app could not be linked.\n");
		gst_object_unref (pipeline);
		return 0;
	}

	gst_object_unref (queue_app_pad);
	
#endif

#ifdef kUSE_DEBUG_SINK

	GstPad   *tee_queue_xvimg_pad, *queue_xvimg_pad;

	tee_queue_xvimg_pad = gst_element_request_pad (tee, tee_src_pad_template, NULL, NULL);
	g_print ("Obtained request pad %s for queue_out branch.\n", gst_pad_get_name (tee_queue_xvimg_pad));
	queue_xvimg_pad = gst_element_get_static_pad (queue_xvimg, "sink");

	if (gst_pad_link (tee_queue_xvimg_pad, queue_xvimg_pad) != GST_PAD_LINK_OK) {

		g_critical ("Tee for queue_xvimg could not be linked.\n");
		gst_object_unref (pipeline);
		return 0;
	}
	
	
	gst_object_unref (queue_xvimg_pad);

#endif
	printf ("WebCam initialization successfull!\n");
	return 1;
}

GstElement* get_appsink() {
	return appsink;
}

void dealloc () {
	if (is_running) {
		g_main_loop_quit(loop);
	}
	is_running = 0;
	gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_NULL);
	gst_object_unref(GST_OBJECT(pipeline));
	g_main_loop_unref (loop);
}

void start_gst_loop() {

	g_message ("Starting loop.");
	gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_PLAYING);
	is_running = 1;
	g_main_loop_run(loop);
	gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_READY);
	
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
/*
	g_main_loop_quit(loop);
	gst_object_unref(GST_OBJECT(pipeline));
	g_main_loop_unref (loop);
*/
}

int is_running_gst_loop () {
	return is_running;
}

void resume_from_eos () 
{
	
	gst_element_set_state (pipeline, GST_STATE_PLAYING);
	is_running = true;
}


static gboolean bus_call(GstBus *bus, GstMessage *msg, void *user_data)
{

	switch (GST_MESSAGE_TYPE(msg)) {
	case GST_MESSAGE_EOS: {
		
		is_running = false;

		g_main_loop_quit(loop);
		if (report_eos != NULL) {
			report_eos();		
		}
		g_message("End-of-stream - Dealocating everything");
		dealloc();
		break;
	}
	case GST_MESSAGE_ERROR: {
		GError *err;
		gst_message_parse_error(msg, &err, NULL);
		is_running = false;
		if (report_error != NULL) {
			report_error(kERROR_CRITICAL , "VideoServer encountered an error and will stop:");
		}

		g_critical ("ERROR: %s", err->message);
		g_error_free(err);
		
		g_main_loop_quit(loop);
		dealloc();
		
		break;
	} 
	case GST_MESSAGE_APPLICATION: {

		const GstStructure *str;
		str = gst_message_get_structure (msg);
		 if (gst_structure_has_name(str, kTURN_OFF_BUS_MESSAGE))
			{
				is_running = false;
				g_main_loop_quit(loop);
			}

		break;
	}
	default:
	
		break;
	}
		if (msg->type == GST_MESSAGE_STATE_CHANGED ) {
			GstState old_state, new_state, pending_state;
     			gst_message_parse_state_changed (msg, &old_state, &new_state, &pending_state);
/*
			g_print ("Element %s changed state from %s to %s with pending: %s.\n",
        			GST_OBJECT_NAME (msg->src),
        			gst_element_state_get_name (old_state),
       				 gst_element_state_get_name (new_state),
				gst_element_state_get_name (pending_state));
*/

		} else {
//			g_print ("info: %i %s type: %i\n", (int)(msg->timestamp), GST_MESSAGE_TYPE_NAME (msg), msg->type);
		}

	return true;
}


void dealloc_gst() {
//	free (ip);
//	free (multicast);
//	free (port);
}


int get_video_width() {
	return atoi(input_width);
}

int get_video_height() {
	return atoi(input_height);
}
