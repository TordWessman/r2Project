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

#include "sphinx_stream.h"
#include <string.h>

//#define MODELDIR "./language"
//#define MODELDIR "/home/olga/workspace/robot/native/language"
#define MODELDIR ""
#define NATIVE_MODELDIR "/usr/share/pocketsphinx/model"

#define USE_RAW_AUDIO

static const char*(*text_received)(const char *message);				//delegate method for sending messages
static const char*(*report_error)(int type, const char *message);	//delegate method for reporting back error

static bool is_active = true, is_running = false;
static int m_port = 5002;
static const char *m_hostIp;
static GMainLoop *loop;

static GstElement 
		//*bin, 	// the containing all the elements
		*pipeline, 	 		// the pipeline for the bin
		*udpsrc,
		*audio_resample, 	// ...since the capabilities of the audio sink usually vary depending on the environment (output used, sound card, driver etc.)
		*vader,				// for threasholding the input to the pocketsphinx
		*asr,				// the main asr (speech to text) engine
		*rtpmp4adepay,
		*conv0, 			// audioconvert0
		*queue,
		*faad,
		*decodebin,
		*fakesink;

static GstBus *bus;	//the bus element te transport messages from/to the pipeline

static const char *lm_file_name, *dic_file_name, *hmm_file_name;

static const char *encoding_name = 0, *udp_config = 0;

//dealocates and shuts down
void turn_off () {
	//is_running = false;
	g_main_loop_quit(loop);
	gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_NULL);
	gst_object_unref(GST_OBJECT(pipeline));
	g_main_loop_unref (loop);
}

//the send error method WILL send errors back to the caller
int send_error (int error_type, const char *error_message)
{
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

void set_udp_config (const char *enc_name, const char *udp_cfg) {
	encoding_name = strdupa(enc_name);
	udp_config = strdupa(udp_cfg);
}

static gboolean bus_call(GstBus *bus, GstMessage *msg, void *user_data)
{

	switch (GST_MESSAGE_TYPE(msg)) {
	case GST_MESSAGE_EOS: {
		g_message("ASR: End-of-stream");
		//report the end of stream
		//bool tmp = send_error(ASR_EOS, "Unexpected end of stream");
		turn_off();
		break;
	}
	case GST_MESSAGE_ERROR: {
		GError *err;
		gst_message_parse_error(msg, &err, NULL);
		//report error
		g_critical ("WTF: %s" , GST_OBJECT_NAME (msg->src));
		bool tmp = send_error(ASR_ERROR_GST, err->message);
		g_error_free(err);
		
		turn_off();
		
		break;
	} //indicating an element specific message
	case GST_MESSAGE_APPLICATION: {
		const GstStructure *str;
		str = gst_message_get_structure (msg);
			if (gst_structure_has_name(str,"partial_result"));
				//TODO: do something on partial results?
			else if (gst_structure_has_name(str,"result"))
			{
				g_print ("       ------           RESULT: %s\n", gst_structure_get_string(str,"hyp"));
				//trigger the text_received method with the hypothesis as input
				
				if (text_received != NULL)
					text_received (gst_structure_get_string(str,"hyp"));
				else
					printf ("WARNING: (text_received not set): %s\n", gst_structure_get_string(str,"hyp"));
				
								
			}
			else if (gst_structure_has_name(str,"turn_off"))
			{
				turn_off();
				return false;
			}
		break;
	}
	default:
	
		break;
	}

		if (msg->type == GST_MESSAGE_STATE_CHANGED ) {
			GstState old_state, new_state, pending_state;
     			gst_message_parse_state_changed (msg, &old_state, &new_state, &pending_state);
			g_print ("Element %s changed state from %s to %s with pending: %s.\n",
        			GST_OBJECT_NAME (msg->src),
        			gst_element_state_get_name (old_state),
       				 gst_element_state_get_name (new_state),
				gst_element_state_get_name (pending_state));
		} else {
			printf("info: %s %s type: %i\n",GST_OBJECT_NAME (msg->src), GST_MESSAGE_TYPE_NAME (msg), msg->type);
		}

	return true;
}

int asr_turn_off()
{
	turn_off ();
	return 1;
	//hmm... there's probably a better way to create a gvalue containing a string from the gchararray...
	GValue text_gv = {0};
	g_value_init (&text_gv, G_TYPE_STRING);
	g_value_set_string(&text_gv, "turn_off");

	//creates message structure for the partial result:
	GstStructure *messageStruct = gst_structure_empty_new ("turn_off");
	//set the hypothesis (the guessed text)
	gst_structure_set_value (messageStruct, "hyp", &text_gv);	

	//post message to the pipeline bus
	
	if (gst_bus_post(
		gst_element_get_bus(asr), 
		gst_message_new_application((GstObject *) asr, messageStruct)))
	{
		//TODO: something if post failed...
	}
		//unref elements
	
	printf("---------------------------------turned off asr engine.");
	return 1;
	
}


//the partial result method is triggered when a comprehensive part of the audio input is transcribed
void asr_partial_result (GstElement* asr,  gchararray text, gchararray uttid, gpointer user_data)
{
	//hmm... there's probably a better way to create a gvalue containing a string from the gchararray...
	GValue text_gv = {0};
	g_value_init (&text_gv, G_TYPE_STRING);
	g_value_set_string(&text_gv, text);

	//creates message structure for the partial result:
	GstStructure *messageStruct = gst_structure_empty_new ("partial_result");
	//set the hypothesis (the guessed text)
	gst_structure_set_value (messageStruct, "hyp", &text_gv);	

	//post message to the pipeline bus
	if (is_active)
		if (gst_bus_post(
			gst_element_get_bus(asr), 
			gst_message_new_application((GstObject *) asr, messageStruct)))
		{
			//TODO: something if post failed...
		}
}

/* the result method is triggered when a full part of the audio input is transcribed */
void asr_result (GstElement* asr,  gchararray text, gchararray uttid, gpointer user_data)
{
	//
	//hmm... there's probably a better way to create a gvalue containing a string from the gchararray...
	GValue text_gv = {0};
	g_value_init (&text_gv, G_TYPE_STRING);
	g_value_set_string(&text_gv, text);

	//creates message structure for the partial result:
	GstStructure *messageStruct = gst_structure_empty_new ("result");
	//set the hypothesis (the guessed text)
	gst_structure_set_value (messageStruct, "hyp", &text_gv);	

	//post message to the pipeline bus
	if (is_active)
		if (gst_bus_post(
			gst_element_get_bus(asr), 
			gst_message_new_application((GstObject *) asr, messageStruct)))
		{
			//TODO: something if post failed...
		}
}

/*	The initElements configures all the elements of the pipeline, as well as the pipline
	and the bin it self.
	
	it also configures the element/pipeline bus message methods
*/

static void
on_pad_added (GstElement *element,
              GstPad     *pad,
              gpointer    data)
{

	g_print ("Dynamic pad created, linking demuxer/decoder\n");

 GstPad *audiopad;

  /* link audiopad */
  audiopad = gst_element_get_static_pad (queue, "sink");
  if (!GST_PAD_IS_LINKED (audiopad)) {
      gst_pad_link (pad, audiopad);
      g_print("audiopad has been linked");
    g_object_unref (audiopad);
    return;
  }
/*
  GstPad *sinkpad;
  GstElement *decoder = (GstElement *) data;

 
  g_print ("Dynamic pad created, linking demuxer/decoder\n");

  sinkpad = gst_element_get_static_pad (decoder, "sink");

  gst_pad_link (pad, sinkpad);

  gst_object_unref (sinkpad);*/
}


int linkWithBin() {
	if (!encoding_name || !udp_config) {
		return send_error(43, "Encoding name and/or udp_config not set! Use set_udp_config(str,str)");
	}

	//link vader->asr
	GstCaps *asr_caps = gst_caps_new_simple("audio/x-raw-int",
			"rate" , G_TYPE_INT, 8000, 
			"channels", G_TYPE_INT, 1, 
			"endianness", G_TYPE_INT, 1234, 
			"width", G_TYPE_INT, 16, 
			"depth", G_TYPE_INT, 16, 
			"signed", G_TYPE_BOOLEAN, true,
			//"width", G_TYPE_INT, atoi(input_width),
			
			NULL);
	if (!gst_element_link_filtered(vader,asr,asr_caps)) {
		return send_error(ASR_ERROR_LINK_FAILED, "Unable to link ASR!");
	}

	// link the elements and check for success

	if (!gst_element_link_many (
		udpsrc, 
		rtpmp4adepay,
		decodebin,
	NULL))
	 	return send_error(ASR_ERROR_LINK_FAILED, "Unable to link to decoder!");

	
	

	if (!gst_element_link_many (
		queue,
		conv0,
		audio_resample,  
		vader,
	NULL))
	 	return send_error(ASR_ERROR_LINK_FAILED, "Unable to link elements!");

	if (!gst_element_link_many(
		asr,
		fakesink,
	NULL))
	 	return send_error(ASR_ERROR_LINK_FAILED, "Unable to link last elements!");
	

	//set up the dynamic pad callback for the demuxerer
	//g_signal_connect (demuxer, "pad-added", G_CALLBACK (on_pad_added), faad);

	//creation successfull
	
	return 0;
}

int linkRaw(){
	
	if (!gst_element_link_filtered( udpsrc, conv0,
	gst_caps_new_simple("audio/x-raw-int",
		"rate", G_TYPE_INT, 8000,
		"depth", G_TYPE_INT, 16,	
		"width", G_TYPE_INT, 16,
		"channels", G_TYPE_INT, 1,
		"endianess", G_TYPE_INT, 1234,
		"endianness", G_TYPE_INT, 1234,
		"signed", G_TYPE_BOOLEAN, true,
		NULL)))
		return send_error(ASR_ERROR_LINK_FAILED, "Unable to link to converter!"); 
	if (!gst_element_link_many(
		//udpsrc,
		conv0,
		//audio_resample,
		vader,
		asr,
		fakesink,
	NULL))
	 	return send_error(ASR_ERROR_LINK_FAILED, "Unable to link elements!");
	
	return 0;
}

int init_elements (const char *lm_file, const char *dict_file, const char *hmm_file)
{

	if (!m_hostIp) {
		return send_error (0, "No host ip assigned!");
	} 

	// create the main loop
	loop = g_main_loop_new(NULL, FALSE);

	pipeline = gst_pipeline_new ("asr_pipeline");


	//initializing elements
	udpsrc = gst_element_factory_make ("tcpserversrc", "udpsrc");
	audio_resample = gst_element_factory_make ("audioresample", "audio_resample");
	vader = gst_element_factory_make ("vader", "vader");
	asr = gst_element_factory_make ("pocketsphinx", "asr");
	conv0 = gst_element_factory_make ("audioconvert", "audioconvert0");
	fakesink = gst_element_factory_make ("fakesink", "fakesink");

	if(!udpsrc)
		return send_error(ASR_ERROR_INIT_ALSA_FAILED, "Unable create udpsrc!");
	if(!audio_resample)
		return send_error(ASR_ERROR_INIT_RESAMPLER_FAILED, "Unable create resampler.");
	if(!vader)
		return send_error(ASR_ERROR_INIT_VADER_FAILED, "Unable create vader.");
	if(!asr)
		return send_error(ASR_ERROR_INIT_POCKETSPHINX_FAILED, "Unable create pocketsphinx element. Is the gstpocketsphinx installed?");
	if(!fakesink)
		return send_error(ASR_ERROR_INIT_SINK_FAILED, "Unable create fakesink... strange.");
	if(!conv0)
		return send_error(ASR_ERROR_INIT_CONVERTER_FAILED, "Unable create converter... strange.");



#ifndef USE_RAW_AUDIO

	rtpmp4adepay = gst_element_factory_make ("rtpmp4adepay", "rtpmp4adepay");
	queue = gst_element_factory_make ("queue", "queue");
	decodebin =gst_element_factory_make ("decodebin", "decodebin");

	if(!rtpmp4adepay)
		return send_error(ASR_ERROR_INIT_IIRFILTER_FAILED, "Unable create rtpmp4adepay");
	if(!decodebin)
		return send_error(ASR_ERROR_INIT_AMPLIFIER_FAILED, "Unable create decodebin... strange.");
	if (!queue)
		return send_error(42, "Unable to create queue");
	
	//listening to pad added-signal
	g_signal_connect (decodebin, "new-decoded-pad", G_CALLBACK (on_pad_added), NULL);
	


	g_object_set(G_OBJECT(udpsrc), "caps", gst_caps_new_simple("application/x-rtp",
		"payload", G_TYPE_INT, 96,
		"encoding-name", G_TYPE_STRING, encoding_name,
		"clock-rate", G_TYPE_INT, 8000,	
		"config", G_TYPE_STRING, udp_config,
		"media", G_TYPE_STRING, "audio",
		NULL), NULL);
#endif
/*
	g_object_set(G_OBJECT(udpsrc), "caps", gst_caps_new_simple("audio/x-raw-int",
		"rate", G_TYPE_INT, 8000,
		"depth", G_TYPE_INT, 16,	
		"width", G_TYPE_INT, 16,
		"channels", G_TYPE_INT, 1,
		"endianess", G_TYPE_INT, 1234,
		"endianness", G_TYPE_INT, 1234,
		"signed", G_TYPE_BOOLEAN, true,
		NULL), NULL);
*/

	g_object_set(G_OBJECT(udpsrc), "port", m_port, NULL);
	g_object_set(G_OBJECT(udpsrc), "host", m_hostIp, NULL);

	//set up the vader to auto-threshold:
	g_object_set(G_OBJECT(vader), "auto_threshold", true, NULL);

	//set the directory containing acoustic model parameters
	g_object_set(G_OBJECT(asr), "hmm",  hmm_file , NULL);
	//set the language model of the asr
	g_object_set(G_OBJECT(asr), "lm",  lm_file , NULL);
	//set the dictionary of the asr
	g_object_set(G_OBJECT(asr), "dict",  dict_file , NULL);
	//set the asr to be configured before receiving data
	g_object_set(G_OBJECT(asr), "configured",  true , NULL);

	//add the bus message methods to the asr (complete & partial)
	g_signal_connect (asr, "partial_result", G_CALLBACK (asr_partial_result), NULL);
	g_signal_connect (asr, "result", G_CALLBACK (asr_result), NULL);

	// create the bus for the pipeline:
	bus = gst_pipeline_get_bus(GST_PIPELINE(pipeline));
	//add the bus handler method:
	gst_bus_add_watch(bus, bus_call, loop);
	//g_signal_connect (G_OBJECT (bus), "message", G_CALLBACK (bus_call), loop);
	gst_object_unref(bus);

	// Add the elements to the bin
	gst_bin_add_many (GST_BIN (pipeline), 
		udpsrc,
		conv0,
//		audio_resample,  
		vader,
		asr,
		fakesink,
#ifndef USE_RAW_AUDIO
		rtpmp4adepay,
		decodebin, 
		queue,
#endif		
	NULL);



#ifndef USE_RAW_AUDIO
	return linkWithBin();
#else
	return linkRaw();
#endif
}

int asr_start ()
{
	if (is_running) {
		send_error (0, "Unable to start ASR - already running!");
		return 1;  
	}
	
	text_received ("THIS MAMMA");
	//try to create the elements and initialize them
	int init_elements_result = init_elements(lm_file_name , dic_file_name, hmm_file_name);
	if (init_elements_result  != 0) {		
		return 1;
	}
	
	//travers the pipe to "play state"
	gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_PLAYING);
	is_running = true;
	//start the main loop
	g_main_loop_run(loop);
	is_running = false;
	
	return 0;
}

void asr_set_text_received_callback(const char*(*text_received_callback)(const char *message ))
{
	text_received = text_received_callback;
}
void asr_set_report_error_callback (const char*(*report_error_callback)(int type, const char *message))
{
	report_error = report_error_callback;
}

int asr_init (const char*(*text_received_callback)(const char *message ),
		  const char*(*report_error_callback)(int type, const char *message),
		  const char *lm_file, const char *dict_file, const char *hmm_file, int port, const char *hostIp)
{

	asr_set_text_received_callback(text_received_callback);
	asr_set_report_error_callback(report_error_callback);

	m_port = port;
	m_hostIp = strdupa (hostIp);

	
	//check for file existance!%!
	FILE *fh;

	int fail = 0;
	fh = fopen(lm_file, "rb");
//printf("\n%s\n", lm_file);
//printf("\n%s\n", dict_file);
//printf("\n%s\n", hmm_file);

    if (fh == NULL) {
		printf("\nFailed to open lm_file:%s\n", lm_file);
		fail = 1;
	} else
	fclose(fh);

	fh = fopen(dict_file, "rb");
    if (fh == NULL) {
		printf("\nFailed to open dict_file:%s\n", dict_file);
		fail = 1;
	} else
	fclose(fh);	
	
	fh = fopen(hmm_file, "rb");
    if (fh == NULL) {
		printf("\nFailed to open hmm_file:%s\n", hmm_file);
		fail = 1;
	} else
	fclose(fh);

	if (fail == 1)
	{
		g_critical("ASR init failed!");
	}




	lm_file_name = strdupa(lm_file); //copy and use stack! (gnu only)
	dic_file_name = strdupa(dict_file);
	hmm_file_name = strdupa(hmm_file);



	//initialize gst(!)
	gst_init (NULL, NULL);
/*
	GstRegistry *registry;
	registry = gst_registry_get_default();

	gst_registry_add_path(registry, "/usr/lib");
	gst_registry_add_path(registry, "/usr/lib/gstreamer-0.10");
*/

	return fail;

}

void set_is_active (bool report_mode)
{
	printf (report_mode ? " ASR is active\n" : " ASR is inactive\n");
	is_active = report_mode;
	GstState currentState = GST_STATE(GST_ELEMENT(pipeline));
	/*
	if (GST_STATE_CHANGE_SUCCESS == gst_element_get_state(GST_ELEMENT(pipeline), currentState, NULL, GST_CLOCK_TIME_NONE) ) {
		report_error (95, "UNABLE TO GET/CHANGE STATE");
	} else {
	}*/
	
	if (is_active && currentState == GST_STATE_PAUSED) {
		gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_PLAYING);	
	} else if (!is_active && currentState== GST_STATE_PLAYING) {
		gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_PAUSED);
	}
	
}

bool get_is_active () 
{
	return is_active;
}



bool get_is_running () 
{
	return is_running;
}

int main (int argc, char *argv[])
{
	//set_udp_config("MP4A-LATM", "40002b10");
	//m_hostIp = "192.168.0.19";

	if (asr_init (0,0,
			"/home/olga/Temp/language.lm", //arpa
			"/home/olga/Temp/language.dic", //dict
			NATIVE_MODELDIR "/hmm/en_US/hub4wsj_sc_8k",
			5005, "192.168.0.19" ) == 0) {
		asr_start ();

		while (is_running) {
			printf ("x");
			}
		asr_start();
	
	} else {
		printf("Init failed...");
		return -1;
	}

	return 0;
}
