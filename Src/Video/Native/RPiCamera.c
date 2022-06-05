// This file is part of r2Poject.
//
// Copyright 2016 Tord Wessman
// 
// r2Project is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
// 
// r2Project is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with r2Project. If not, see <http://www.gnu.org/licenses/>.
// 
#include "RPiCamera.h"
#include <string.h>
#include <gst/gst.h>
#include <signal.h>
#include <unistd.h>
#include <stdlib.h>

// rpicamsrc inline-headers=true preview=false bitrate=524288 keyframe-interval=30 ! video/x-h264, parsed=false, stream-format=\"byte-stream\", level=\"4\", profile=\"high\", framerate=30/1,width=640,height=480 ! queue leaky=2 ! tcpserversink host=0.0.0.0 port=" + @cam_port.to_s
		
static GMainLoop *rpi_loop;
static GstElement *rpi_pipeline, *rpi_src, *rpi_queue_tcp, *rpi_tcpserversink;
static GstBus *rpi_bus;

#define kDefaultKeyframeInterval 30

static int rpi_bitrate = 524288;
static int rpi_framerate = kDefaultKeyframeInterval;
static int rpi_width;
static int rpi_height;
static int rpi_port;
static bool rpi_initiated = false;
static bool rpi_recording = false;

void _ext_rpi_init(int width, int height, int port) {

	rpi_width = width;
	rpi_height = height;
	rpi_port = port;

}

void _ext_rpi_init_extended(int width, int height, int port, int bitrate, int framerate) {

	rpi_width = width;
	rpi_height = height;
	rpi_port = port;
	rpi_bitrate = bitrate;
	rpi_framerate = framerate;

}

static gboolean rpi_message_cb(GstBus * bus, GstMessage * message, gpointer user_data) {

  switch(GST_MESSAGE_TYPE(message)) {

    case GST_MESSAGE_ERROR:{

      GError *err = NULL;
      gchar *name, *debug = NULL;

      name = gst_object_get_path_string(message->src);
      gst_message_parse_error(message, &err, &debug);

      g_printerr("libr2rpicamera ERROR: from element %s: %s\n", name, err->message);
      if(debug != NULL)
        g_printerr("Additional debug info:\n%s\n", debug);

      g_error_free(err);
      g_free(debug);
      g_free(name);

      g_main_loop_quit(rpi_loop);
      break;

    } case GST_MESSAGE_WARNING:{

			GError *err = NULL;
			gchar *name, *debug = NULL;

			name = gst_object_get_path_string(message->src);
			gst_message_parse_warning(message, &err, &debug);

			g_printerr("libr2rpicamera WARNING: from element %s: %s\n", name, err->message);
			if(debug != NULL)
			g_printerr("Additional debug info:\n%s\n", debug);

			g_error_free(err);
			g_free(debug);
			g_free(name);
			break;

    } case GST_MESSAGE_EOS:{

	    rpi_recording = false;
			rpi_initiated = false;
			g_main_loop_quit(rpi_loop);
			gst_element_set_state(rpi_pipeline, GST_STATE_NULL);
			g_main_loop_unref(rpi_loop);
			gst_object_unref(rpi_pipeline);
			break;

		}

    default:
		break;

  }

  return true;
}

int _ext_rpi_setup() {

	rpi_recording = false;
	gst_init(NULL, NULL);

	GstCaps *camera_caps;

	rpi_pipeline = gst_pipeline_new(NULL);
	rpi_src = gst_element_factory_make("rpicamsrc", NULL);
	rpi_tcpserversink = gst_element_factory_make("tcpserversink", NULL);
	rpi_queue_tcp = gst_element_factory_make("queue", "queue_tcp");

	if(!rpi_pipeline || !rpi_src || !rpi_tcpserversink || !rpi_queue_tcp) {
		g_error("Failed to create one or more elements");
		return -1;
	}

	// Set up properties ---
	g_object_set(G_OBJECT(rpi_src), "inline-headers", true, NULL);
	g_object_set(G_OBJECT(rpi_src), "preview", false, NULL);
	g_object_set(G_OBJECT(rpi_src), "bitrate", rpi_bitrate, NULL);
	g_object_set(G_OBJECT(rpi_src), "keyframe-interval", kDefaultKeyframeInterval, NULL);

	g_object_set(G_OBJECT(rpi_tcpserversink), "port", rpi_port, NULL);
	g_object_set(G_OBJECT(rpi_tcpserversink), "host", "0.0.0.0", NULL);
	g_object_set(G_OBJECT(rpi_queue_tcp), "leaky", 2, NULL);

	// Set up caps ---
	camera_caps =  gst_caps_new_simple("video/x-h264",
			"stream-format", G_TYPE_STRING, "byte-stream",
			"width", G_TYPE_INT, rpi_width,
			"height", G_TYPE_INT, rpi_height,
			"level", G_TYPE_STRING, "4",
			"profile", G_TYPE_STRING, "high",
			"parsed", G_TYPE_BOOLEAN, false,
			"framerate", GST_TYPE_FRACTION, rpi_framerate, 1,
			NULL);

	// Combine pipeline

	gst_bin_add_many(GST_BIN(rpi_pipeline), rpi_src, rpi_queue_tcp, rpi_tcpserversink, NULL);

	if(!gst_element_link_filtered(rpi_src, rpi_queue_tcp, camera_caps)) {
		gst_object_unref(rpi_pipeline);
		g_critical("Unable to link rpi_src to queue");
		return -2;
	}

	if(!gst_element_link_many(rpi_queue_tcp, rpi_tcpserversink, NULL)) {
		g_error("Failed to link to tcpserversink");
		return -4;
	}

	rpi_loop = g_main_loop_new(NULL, false);

	rpi_bus = gst_pipeline_get_bus(GST_PIPELINE(rpi_pipeline));
	gst_bus_add_signal_watch(rpi_bus);
	g_signal_connect(G_OBJECT(rpi_bus), "message", G_CALLBACK(rpi_message_cb), NULL);
	gst_object_unref(GST_OBJECT(rpi_bus));

	rpi_initiated = true;
	return 0;

}

void _ext_rpi_stop() {

	gst_element_send_event(rpi_pipeline, gst_event_new_eos());

}

void _ext_rpi_start() {

	gst_element_set_state(rpi_pipeline, GST_STATE_PLAYING);
	rpi_recording = true;
	g_main_loop_run(rpi_loop);
	rpi_recording = false;
	rpi_initiated = false;

}

int _ext_rpi_get_framerate() { return rpi_framerate; }
int _ext_rpi_get_width() { return rpi_width; }
int _ext_rpi_get_height() { return rpi_height; }
bool _ext_rpi_get_initiated() { return rpi_initiated; }
bool _ext_rpi_get_recording() { return rpi_recording; }

int main(int argc, char *argv[]) {

	_ext_rpi_init(640, 480, 4444);

	int setupResult = _ext_rpi_setup(); 

	if(setupResult < 0) {
		return -setupResult;
	} 

	g_print("Starting loop");
	_ext_rpi_start();	

	return 0;
}