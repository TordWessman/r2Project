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

// (c) tord wessman 2012

#include "r2mp3.h"
#include <gio/gio.h>

static GMainLoop *mp3_loop;

GMemoryInputStream *mistream;

static GstElement *mp3_bin, 	// the containing all the elements
		*mp3_pipeline, 	 		// the pipeline for the bin
		*mp3_src, 	 		
		*mp3_decoder, 
		*mp3_echo,	
		*mp3_alsa_sink;

static GstBus *mp3_bus;	//the bus element te transport messages from/to the pipeline

static int mp3_is_playing = 0; //indicates that the player is busy

static int mp3_is_initialized = 0;

static gboolean mp3_bus_call(GstBus *bus, GstMessage *msg, void *user_data)
{

	switch (GST_MESSAGE_TYPE(msg)) {
	case GST_MESSAGE_EOS: {
		g_message("End-of-stream");
		//report the end of stream
		g_main_loop_quit(mp3_loop);
		
		break;
	}
	case GST_MESSAGE_ERROR: {
		GError *err;
		gst_message_parse_error(msg, &err, NULL);
		//report error
		printf ("ERROR: %s", err->message);
		g_error_free(err);
		
		g_main_loop_quit(mp3_loop);
		
		break;
	} 
	default:
	
		break;
	}

		//printf("info: %i %s\n", (int)(msg->timestamp), GST_MESSAGE_TYPE_NAME (msg));

	return true;
}

//this method initiates the gobjects, the pipeline, the bus and the entire bin
int mp3_init(GstElement *src) {

	mp3_is_initialized = 0;

	// create the main loop
	mp3_loop = g_main_loop_new(NULL, FALSE);

	mp3_pipeline = gst_pipeline_new ("mp3_pipeline");
	mp3_bin = gst_bin_new ("mp3_bin");

	//initializing elements
	mp3_src = src;
	mp3_decoder = gst_element_factory_make ("mad", "decoder");
	mp3_alsa_sink = gst_element_factory_make ("alsasink", "alsasink");
	mp3_echo = gst_element_factory_make("audioecho", "echo");
	//g_object_set (G_OBJECT (mp3_echo), "delay", 9000000, NULL);

	//check for successfull creation of elements
	if(!mp3_src) {

		g_critical("Unable to create filesrc!\n");
		return false;

	} else if(!mp3_decoder) {

		g_critical( "Unable create mp3 decoder\n");
		return false;

	} else if(!mp3_alsa_sink) {

		g_critical("Unable create sink.");
		return false;

	} else if(!mp3_echo) {

		g_critical("Unable to create echo!\n");
		return false;
	
	}

	// create the bus for the pipeline:
	mp3_bus = gst_pipeline_get_bus(GST_PIPELINE(mp3_pipeline));
	//add the bus handler method:
	gst_bus_add_watch(mp3_bus, mp3_bus_call, NULL);

	gst_object_unref(mp3_bus);

	// Add the elements to the bin
	gst_bin_add_many (GST_BIN (mp3_bin), 
		mp3_src,
		mp3_decoder, 
		//echo,
		mp3_alsa_sink,
	NULL);

	// add the bin to the pipeline 
	gst_bin_add (GST_BIN (mp3_pipeline), mp3_bin);

	// link the elements and check for success
	if (!gst_element_link_many (
		mp3_src, 
		mp3_decoder,
		//echo,
		mp3_alsa_sink,
	NULL)) {

		g_critical("Unable to link elements!\n");
		return false;

	}

	mp3_is_initialized = 1;


	//creation successfull
	return true;
	
}

//stop the main loop and dealocate resources
void mp3_stop() {
	gst_element_set_state(GST_ELEMENT(mp3_pipeline), GST_STATE_NULL);
	g_main_loop_quit(mp3_loop);
	gst_object_unref(GST_OBJECT(mp3_pipeline));
	g_main_loop_unref (mp3_loop);
}

// Just stops the playback
void mp3_stop_playback() {
	gst_element_set_state(GST_ELEMENT(mp3_pipeline), GST_STATE_NULL);
	g_main_loop_quit(mp3_loop);
}

void mp3_play() {

	gst_element_set_state(GST_ELEMENT(mp3_pipeline), GST_STATE_PLAYING);
	g_main_loop_run(mp3_loop);
	gst_element_set_state(GST_ELEMENT(mp3_pipeline), GST_STATE_NULL);
	
	mp3_is_playing = 0;

}

void mp3_file_play(const char* mp3_file) {

	if (!mp3_is_initialized) {
		g_critical ("Mp3 player Unable to play. You have to initialize first!");
		return; 
	}


	mp3_is_playing = 1;
	g_object_set (G_OBJECT (mp3_src), "location", mp3_file, NULL);

	mp3_play();

}

int apa = 0;

void mp3_memory_play(uint8_t* mp3_pointer, int size) {

	if (!mp3_is_initialized) {
		g_critical ("Mp3 player Unable to play. You have to initialize first!");
		return; 
	}


	mp3_is_playing = 1;
	g_print("Pointer: %p. ", mp3_pointer);
	g_print("A value: %d \n", mp3_pointer[100]);
	
	if (mistream != NULL) {

		g_input_stream_close ((GInputStream *)mistream, NULL, NULL);
		
	}

	mistream = G_MEMORY_INPUT_STREAM(g_memory_input_stream_new_from_data(mp3_pointer, size, (GDestroyNotify) g_free));
	g_print("Input stream pointer: %p. ", mistream);
	g_object_set (G_OBJECT (mp3_src), "stream", G_INPUT_STREAM(mistream), NULL);
	mp3_play();
	
}

void _ext_stop_mp3() {
	mp3_stop();
}

void _ext_stop_playback_mp3() {
	mp3_stop_playback();
}

int _ext_init_mp3_file () {

	gst_init (NULL, NULL);
	return mp3_init(gst_element_factory_make ("filesrc", "src"));

}

int _ext_init_mp3_memory () {

	gst_init (NULL, NULL);
	return mp3_init(gst_element_factory_make ("giostreamsrc", "src"));

}

void _ext_play_file_mp3(const char* mp3_file) {

	if (mp3_is_playing == 0) { mp3_file_play(mp3_file); }

}

void _ext_play_memory_mp3(uint8_t* mp3_pointer, int size) {

	if (mp3_is_playing == 0) { mp3_memory_play(mp3_pointer, size); }

}

void _ext_pause_mp3() {
	g_main_loop_quit(mp3_loop);
}

int _ext_is_playing_mp3 () {
	return mp3_is_playing;
}

int _ext_is_initialized_mp3 () {
	return mp3_is_initialized;
}

int main (int argc, char *argv[]) {

	if (_ext_init_mp3_file()) {

		if (argc > 1) {

			g_print("Now playing file: '%s'", argv[1]);
			mp3_file_play (argv[1]);
			mp3_stop();

		}
		else 
			printf ("Nothing to play...");
	}
	else {
		printf ("unable to initialize");
		return -1;
	}
	
	return 0;
}

