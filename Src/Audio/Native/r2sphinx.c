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

#include "r2sphinx.h"
#include <string.h>
#include <stdlib.h>

static const char*(*text_received)(const char *message);				//delegate method for sending messages
static const char*(*report_error)(int type, const char *message);	//delegate method for reporting back error

static bool is_active = true, is_running = false, use_tcp_server = false, should_turn_off = false, dry_run = false;
static int m_port = 5002, error_count = 0;
static const char *m_hostIp;
static GMainLoop *loop;
GstElement *pipeline;

#define MAX_ERROR_RETRIES 10

static GstBus *bus;	//the bus element te transport messages from/to the pipeline
guint bus_watch_id;

static const char *lm_file_name, *dic_file_name, *hmm_file_name;

//dealocates and shuts down
void turn_off () {

	should_turn_off = true;
	g_main_loop_quit(loop);

}

//the send error method WILL send errors back to the caller
int send_error (int error_type, const char *error_message) {
	if (report_error != NULL) {
		report_error(error_type, error_message);
	}
	printf ("-------------------------- ERROR: '%s', code: %i\n", error_message, error_type);
	return true;
}

char *strdupa (const char *s) {
    char *d = malloc (strlen (s) + 1);   // Allocate memory
    if (d != NULL)
        strcpy (d,s);                    // Copy string if okay
    return d;                            // Return new memory
}

static gboolean bus_call(GstBus * bus, GstMessage * msg, gpointer data) {
    GMainLoop *loop = (GMainLoop *) data;

    switch (GST_MESSAGE_TYPE(msg)) {

    case GST_MESSAGE_EOS:
        g_print("End of stream\n");
	turn_off();
        break;

    case GST_MESSAGE_ERROR:{
		should_turn_off = true;
            gchar *debug;
            GError *error;

            gst_message_parse_error(msg, &error, &debug);
            g_free(debug);

            g_printerr("Error: %s\n", error->message);
		send_error(error->code, error->message);

		turn_off();
            g_error_free(error);
		
            g_main_loop_quit(loop);
            break;
        }
    default:
        break;
    }
    
    const GstStructure *st = gst_message_get_structure(msg);

    if (st && strcmp(gst_structure_get_name(st), "pocketsphinx") == 0) {

	if (g_value_get_boolean(gst_structure_get_value(st, "final"))) {

		long confidence = g_value_get_long(gst_structure_get_value(st, "confidence")) - 4294000000;
		 g_print("Got result: %s, %ld\n", g_value_get_string(gst_structure_get_value(st, "hypothesis")), confidence);

		if (text_received != NULL) {
			text_received(g_value_get_string(gst_structure_get_value(st, "hypothesis")));
		}
	} else {
	
		long confidence = g_value_get_long(gst_structure_get_value(st, "confidence")) - 4294000000;
	 	g_print("Partial: %s, %ld\n", g_value_get_string(gst_structure_get_value(st, "hypothesis")), confidence);
	
	}

    }

    return true;

}

int _ext_asr_turn_off() {
	turn_off ();
	return 1;
}

GstCaps* asr_create_caps(int rate) {

	return gst_caps_new_simple("audio/x-raw",
		"rate", G_TYPE_INT, rate,
		"depth", G_TYPE_INT, 8,	
		"width", G_TYPE_INT, 8,
		"channels", G_TYPE_INT, 1,
		//"endianess", G_TYPE_INT, 1234,
		"signed", G_TYPE_BOOLEAN, true,
		"layout", G_TYPE_STRING, "interleaved",
		"format", G_TYPE_STRING, "S16LE",
		NULL);
}

int init_elements (const char *lm_file, const char *dict_file, const char *hmm_file) {

	GstElement *source, *gdpdepay,  *decoder, *sink, *audioconvert, *audioresample;

	if (use_tcp_server) {

		if (!m_hostIp) { return send_error (0, "No host ip assigned!"); }

		source = gst_element_factory_make("tcpserversrc", "audiosrc");
		g_object_set(G_OBJECT(source), "port", m_port, NULL);
		g_object_set(G_OBJECT(source), "host", m_hostIp, NULL);
		gdpdepay = gst_element_factory_make("gdpdepay", "gdpdepay");

	} else {

		source = gst_element_factory_make("autoaudiosrc", "audiosrc");
	
	}

    	pipeline = gst_pipeline_new("asr_pipeline");
	audioconvert = gst_element_factory_make("audioconvert", "audioconvert");
	audioresample = gst_element_factory_make("audioresample", "audioresample");

	if (dry_run) {

	    	decoder = gst_element_factory_make("queue", "queue");
	    	sink = gst_element_factory_make("autoaudiosink", "output");	
		g_object_set(G_OBJECT(sink), "sync", false, NULL);
		
	} else {

	    	decoder = gst_element_factory_make("pocketsphinx", "asr");
	    	sink = gst_element_factory_make("fakesink", "output");

	}

	if (!source || !audioconvert) { return send_error(ASR_ERROR_INIT_FAILED, "Unable create src or audioconvert"); }

    	

	if (use_tcp_server) {

		gst_bin_add_many(GST_BIN(pipeline), source, gdpdepay, audioconvert , audioresample , decoder, sink, NULL);

		if (!gst_element_link ( source, gdpdepay)) { return send_error(ASR_ERROR_LINK_FAILED, "Unable to link to source to gdpdepap!"); }
		if (!gst_element_link_filtered( gdpdepay, audioconvert, asr_create_caps(44100))) { return send_error(ASR_ERROR_LINK_FAILED, "Unable to link gdpdepay to audioconvert!"); }
		if (!gst_element_link ( audioconvert, audioresample)) { return send_error(ASR_ERROR_LINK_FAILED, "Unable to link audioconvert to audioresample!"); }
		if (!gst_element_link_filtered( audioresample, decoder, asr_create_caps(16000))) { return send_error(ASR_ERROR_LINK_FAILED, "Unable to link audioresample to decoder!"); }
		if (!gst_element_link (decoder, sink)) { return send_error(ASR_ERROR_LINK_FAILED, "Unable to link decoder to sink!"); }

	} else {
		
		gst_bin_add_many(GST_BIN(pipeline), source, audioconvert, audioresample, decoder, sink, NULL);

		if (!gst_element_link_filtered( source, audioconvert, asr_create_caps(16000))) { return send_error(ASR_ERROR_LINK_FAILED, "Unable to link to converter!"); }
		if (!gst_element_link_many(audioconvert , audioresample , decoder, sink, NULL)) { return send_error(ASR_ERROR_LINK_FAILED, "Unable to link elements!"); }
		
	} 
	
	

	if (hmm_file && lm_file && dict_file) {

		//set the directory containing acoustic model parameters
		g_object_set(G_OBJECT(decoder), "hmm",  hmm_file , NULL);
		//set the language model of the asr
		g_object_set(G_OBJECT(decoder), "lm",  lm_file , NULL);
		//set the dictionary of the asr
		g_object_set(G_OBJECT(decoder), "dict",  dict_file , NULL);
		//set the asr to be configured before receiving data
		g_object_set(G_OBJECT(decoder), "configured",  true , NULL);
	}


	return 0;

}

int _ext_asr_start () {
	if (is_running) {
		send_error (0, "Unable to start ASR - already running!");
		return 1;  
	}

	is_running = true;

	
    loop = g_main_loop_new(NULL, FALSE);

    bus = gst_pipeline_get_bus(GST_PIPELINE(pipeline));
    bus_watch_id = gst_bus_add_watch(bus, bus_call, loop);
    gst_object_unref(bus);

    	if (GST_STATE_CHANGE_FAILURE == gst_element_set_state(pipeline, GST_STATE_PLAYING)) {
		error_count++;
	}

	if (error_count > MAX_ERROR_RETRIES) {
		return send_error(ASR_ERROR_GST, "Unable to start pipeline.");
	} else {
		error_count = 0;
	}

    g_main_loop_run(loop);

	is_running = false;
    	gst_element_set_state(pipeline, GST_STATE_NULL);
	g_main_loop_unref(loop);
	//gst_object_unref(GST_OBJECT(pipeline));
	g_source_remove(bus_watch_id);
	if (should_turn_off) {

	    
	} else {

		return _ext_asr_start();
	}
	
	should_turn_off = false;
	
	
	return 0;
}

void _ext_asr_set_text_received_callback(const char*(*text_received_callback)(const char *message )) {

	text_received = text_received_callback;

}

void _ext_asr_set_report_error_callback (const char*(*report_error_callback)(int type, const char *message)) {

	report_error = report_error_callback;

}

int _ext_asr_init (const char*(*text_received_callback)(const char *message ),
		  const char*(*report_error_callback)(int type, const char *message),
		  const char *lm_file, const char *dict_file, const char *hmm_file, int port, const char *hostIp, int configuration_flags) {

	_ext_asr_set_text_received_callback(text_received_callback);
	_ext_asr_set_report_error_callback(report_error_callback);

	if (configuration_flags == (ASR_INPUT_LOCAL | ASR_OUTPUT_LOCAL)) {
		
		return send_error(ASR_ERROR_INIT_FAILED, "Bad ASR configuration: using both local input and output.");
	}

	use_tcp_server = !(configuration_flags & ASR_INPUT_LOCAL);
	dry_run = configuration_flags & ASR_OUTPUT_LOCAL;

	m_port = port;

	if (hostIp) { m_hostIp = strdupa (hostIp); }

	if (lm_file == NULL || dict_file == NULL || hmm_file == NULL) {
	
		printf("lm/dict/hmm files missing. Using default settings. \n");
		
		lm_file_name = NULL;
		dic_file_name = NULL;
		hmm_file_name = NULL;

	} else {

		FILE *fh;

		fh = fopen(lm_file, "rb");

	    	if (lm_file && fh == NULL) {

			return send_error(ASR_ERROR_INIT_FAILED, "Failed to open lm_file. Check if file exists.");

		} else { fclose(fh); }

		fh = fopen(dict_file, "rb");

	    	if (dict_file && fh == NULL) {

			return send_error(ASR_ERROR_INIT_FAILED, "Failed to open dict_file. Check if file exists.");

		} else { fclose(fh); }	
	
		fh = fopen(hmm_file, "rb");
	    	
		if (hmm_file && fh == NULL) {

			return send_error(ASR_ERROR_INIT_FAILED, "Failed to open hmm_file. Check if file exists.");

		} else { fclose(fh); }

		lm_file_name = strdupa(lm_file); // copy and use stack! (gnu only)
		dic_file_name = strdupa(dict_file);
		hmm_file_name = strdupa(hmm_file);

	}

	gst_init (NULL, NULL);
	
	return init_elements(lm_file_name , dic_file_name, hmm_file_name);

}

void _ext_asr_set_is_active (bool report_mode) {
	printf (report_mode ? " ASR is active\n" : " ASR is inactive\n");
	is_active = report_mode;
	GstState currentState = GST_STATE(GST_ELEMENT(pipeline));

	if (is_active && currentState == GST_STATE_PAUSED) {

		gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_PLAYING);	

	} else if (!is_active && currentState== GST_STATE_PLAYING) {

		gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_PAUSED);

	}
	
}

bool _ext_asr_get_is_active ()  { return is_active; }

bool _ext_asr_get_is_running () { return is_running; }

// Compile without the -shared flag if you want to test-run this locally
int main (int argc, char *argv[]) {

	// Test server: gst-launch-1.0 -v tcpserversrc host=192.168.0.10 port=9876 ! gdpdepay !  audio/x-raw,format=S16LE,channels=1,layout=interleaved,width=16,depth=16,rate=16000,signed=true  ! audioconvert ! audioresample ! alsasink
	// Test client: gst-launch-1.0 -v autoaudiosrc ! audio/x-raw,format=S16LE,channels=1,layout=interleaved,width=16,depth=16,rate=16000,signed=true ! gdppay ! tcpclientsink host=192.168.0.10 port=3333

	if (_ext_asr_init (0,0,
			NULL, //arpa
			NULL, //dict
			NULL, //language model
			0, NULL, ASR_INPUT_LOCAL) == 0) {
		_ext_asr_start ();

		while (is_running) {
			printf ("x");
			}
		_ext_asr_start();
	
	} else {
		printf("Init failed...");
		return -1;
	}

	return 0;
}
