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
#include "RPiCameraRecorder.h"
#include <gst/gst.h>
#include <string.h>
#include <stdlib.h>

static GMainLoop *rpir_loop;
static GstElement *rpir_pipeline, *rpir_src, *rpir_queue_tcp, *rpir_filesink;
static GstBus *rpir_bus;

static int rpir_port;
static bool rpir_initiated = false;
static bool rpir_recording = false;
static const char* rpir_address;
static const char* rpir_filename;
static const void*(*rpir_recording_done_callback)(const char *filename);

char *strdupa (const char *s) {
    char *d = malloc (strlen (s) + 1);   // Allocate memory
    if (d != NULL)
        strcpy (d,s);                    // Copy string if okay
    return d;                            // Return new memory
}

void _ext_rpir_init(int port, const char *address) {

	rpir_address = strdupa(address);
	rpir_port = port;

}

static gboolean
rpir_message_cb (GstBus * bus, GstMessage * message, gpointer user_data)
{
  switch (GST_MESSAGE_TYPE (message)) {
    case GST_MESSAGE_ERROR:{
    	
      GError *err = NULL;
      gchar *name, *debug = NULL;

      name = gst_object_get_path_string (message->src);
      gst_message_parse_error (message, &err, &debug);

      g_printerr ("libr2rpicamerarecorder ERROR: from element %s: %s\n", name, err->message);
      if (debug != NULL)
        g_printerr ("Additional debug info:\n%s\n", debug);

      g_error_free (err);
      g_free (debug);
      g_free (name);

      g_main_loop_quit (rpir_loop);
      break;
    }
    case GST_MESSAGE_WARNING:{

		GError *err = NULL;
		gchar *name, *debug = NULL;

		name = gst_object_get_path_string (message->src);
		gst_message_parse_warning (message, &err, &debug);

		g_printerr ("libr2rpicamerarecorder WARNING: from element %s: %s\n", name, err->message);
		if (debug != NULL)
		g_printerr ("Additional debug info:\n%s\n", debug);

		g_error_free (err);
		g_free (debug);
		g_free (name);
		break;

    } case GST_MESSAGE_EOS:{

    	rpir_recording = false;
		rpir_initiated = false;

		g_main_loop_quit (rpir_loop);
		gst_element_set_state (rpir_pipeline, GST_STATE_NULL);
		g_main_loop_unref (rpir_loop);
		gst_object_unref (rpir_pipeline);

		if (rpir_recording_done_callback) {

			rpir_recording_done_callback(rpir_filename);

		}

		break;
	}
    default:
		break;
  }

  return TRUE;
}

int rpir_setup(const char* filename) {

	rpir_filename = strdupa(filename);
	rpir_recording = false;
	gst_init (NULL, NULL);

	rpir_pipeline = gst_pipeline_new(NULL);
	rpir_src = gst_element_factory_make("tcpclientsrc", NULL);
	rpir_filesink = gst_element_factory_make("filesink", NULL);
	rpir_queue_tcp = gst_element_factory_make("queue", "queue_tcp");

	if (!rpir_pipeline || !rpir_src || !rpir_filesink || !rpir_queue_tcp) {
		g_error("Failed to create one or more elements");
		return -1;
	}

	if (!rpir_filename) {
		g_error("No filename set");
		return -42;
	}

	// Set up properties ---
	g_object_set(G_OBJECT(rpir_filesink), "location", rpir_filename, NULL);
	g_object_set(G_OBJECT(rpir_src), "port", rpir_port, NULL);
	g_object_set(G_OBJECT(rpir_src), "host", rpir_address, NULL);

	gst_bin_add_many(GST_BIN(rpir_pipeline), rpir_src, rpir_queue_tcp, rpir_filesink, NULL);

	if (!gst_element_link_many(rpir_src, rpir_queue_tcp, rpir_filesink, NULL)) {
		g_error("Failed to link to tcpserversink");
		return -4;
	}

	rpir_loop = g_main_loop_new(NULL, FALSE);

	rpir_bus = gst_pipeline_get_bus(GST_PIPELINE (rpir_pipeline));
	gst_bus_add_signal_watch(rpir_bus);
	g_signal_connect(G_OBJECT(rpir_bus), "message", G_CALLBACK(rpir_message_cb), NULL);
	gst_object_unref(GST_OBJECT(rpir_bus));

	rpir_initiated = true;
	return 0;
}

void _ext_rpir_stop() {

	if (rpir_recording) {

		gst_element_send_event(rpir_pipeline, gst_event_new_eos());
	
	}

}

const char* _ext_rpir_get_filename() {

	return rpir_filename;

}

int _ext_rpir_record(const char* filename, const void*(*recording_done_callback)(const char *filename)) {

	rpir_recording_done_callback = recording_done_callback;
	int status = rpir_setup(filename);
	if (status != 0) {
		return status;
	}
	
	gst_element_set_state(rpir_pipeline, GST_STATE_PLAYING);
	rpir_recording = true;
	g_main_loop_run(rpir_loop);
	rpir_recording = false;
	rpir_initiated = false;

	return 0;
}

bool _ext_rpir_get_initiated() { return rpir_initiated; }
bool _ext_rpir_get_recording() { return rpir_recording; }

int main(int argc, char *argv[]) {

	_ext_rpir_init(4444, "127.0.0.1");

	int status = _ext_rpir_record("test2.264", NULL); 

	if (status != 0) {
		return status;
	} 

	return 0;
}