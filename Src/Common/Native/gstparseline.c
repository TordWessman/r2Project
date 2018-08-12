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

#include <stdio.h>
#include "gstparseline.h"
#include <string.h>
#include <stdlib.h>

int stopGstPipeLine (struct PLC* plc);

static gboolean bus_callGstPipeLine(GstBus *busObj, GstMessage *msg, void *user_data)
{
	
	struct PLC* plc = (struct PLC*)user_data;

	switch (GST_MESSAGE_TYPE(msg)) {
	case GST_MESSAGE_EOS: {
		//g_message("End-of-stream");
		//report the end of stream
		
		g_main_loop_quit(plc->loop);
		plc->is_stopped = true;
		break;
	}
	case GST_MESSAGE_ERROR: {
		GError *err;
		gst_message_parse_error(msg, &err, NULL);
		//report error
		printf ("ERROR: %s", err->message);
		g_error_free(err);
		
		g_main_loop_quit(plc->loop);
		plc->is_stopped = true;
		
		break;
	} 
	case GST_MESSAGE_ELEMENT: {
		const GstStructure *str;
		str = gst_message_get_structure (msg);
		
		if (gst_structure_has_name(str,"espeak-mark") || 
			gst_structure_has_name(str,"espeak-word")) {
			//espeak messages
		}
		break;
	}
	//onther element specific message

	case GST_MESSAGE_APPLICATION: {

		const GstStructure *str;
		str = gst_message_get_structure (msg);

		 if (gst_structure_has_name(str,"turn_off"))
			{
				g_main_loop_quit(plc->loop);
				plc->is_stopped = true;
			}

		break;
	}
	default:
	
		break;
	}

		//printf("info: %i %s\n", (int)(msg->timestamp), GST_MESSAGE_TYPE_NAME (msg));

	return true;
}


struct PLC* createPipeLine (const char* pipelineString) {

	struct PLC *plc = malloc(sizeof(struct PLC));

	if (plc == NULL) {

		return NULL;

	}

	plc->pipelineString = strdup(pipelineString);
	plc->is_playing = false;
	plc->is_initialized = false;
	plc->is_stopped = false;

	return plc;

}

int destroyPipeLine(struct PLC *plc) {

	if (plc != NULL) {

		if (!plc->is_stopped && plc->is_initialized) {
		
			stopGstPipeLine(plc);

		}

		free(plc);

		return true;

	}

	return false;

}

int initGstPipeLine(struct PLC* plc) {

	if (plc == NULL) {
		
		g_critical ("Unable to init Gtreamer pipeline. Pipeline deinitialized.");
		return false;

	}

	printf ("Starting Gstreamer pipeline: %s \n", plc->pipelineString);

	GError *error = NULL;

	plc->is_stopped = false;

	if (plc->is_playing) {

		g_critical ("Unable to initialize Gtreamer pipeline. It's playng!");
		return false; 		
		
	} else if (plc->is_initialized) {

		gst_object_unref(GST_OBJECT(plc->pipeline));		
		
	}

	gst_init (NULL, NULL);
	plc->loop = g_main_loop_new(NULL, FALSE);

	plc->pipeline = gst_parse_launch (plc->pipelineString, &error);

	if (!plc->pipeline) {
	
		
    		g_critical ("Parse error: %s\n", error->message);
		g_error_free(error);
		return false;
    		
	}

	plc->bus = gst_pipeline_get_bus(GST_PIPELINE(plc->pipeline));

	gst_bus_add_watch(plc->bus, bus_callGstPipeLine, plc);

	gst_object_unref(plc->bus);

	plc->is_initialized = true;

	return true;

}


int playGstPipeLine(struct PLC *plc) {

	if (plc == NULL) {
		
		g_critical ("Unable to Play Gtreamer pipeline. Pipeline deinitialized.");
		return false;

	}

	if (plc->is_stopped) {

		if (!initGstPipeLine(plc)) {

			g_critical ("Unable to start Gtreamer pipeline: cannot re-initialize the pipeline.");
			return false;

		}
	
	}

	if (!plc->is_initialized) {

		g_critical ("Unable to start Gtreamer pipeline. You have to initialize first!");
		return false;
	
	}

	plc->is_playing = true;

	gst_element_set_state(GST_ELEMENT(plc->pipeline), GST_STATE_PLAYING);
	g_main_loop_run(plc->loop);
	gst_element_set_state(GST_ELEMENT(plc->pipeline), GST_STATE_NULL);
	//gst_object_unref(GST_OBJECT(plc->pipeline));
	//g_main_loop_unref (plc->loop);

	plc->is_playing = false;
	
	return true;

}



int stopGstPipeLine (struct PLC* plc) {

	if (plc == NULL) {
		
		g_critical ("Unable to stop Gtreamer pipeline. Pipeline deinitialized.");
		return false;

	}

	printf ("Stopping pipeline: %s \n", plc->pipelineString);

	if (!plc->is_playing) {

		g_critical ("Unable to stop Gtreamer pipeline. Pipeline must be playing");
		return false; 

	}

	if (plc->pipeline != NULL) {

		gst_element_set_state(GST_ELEMENT(plc->pipeline), GST_STATE_NULL);
	
	}  else {

		g_critical ("Unable to stop Gtreamer pipeline. pipeline was null.");
	
	}

	if (plc->loop != NULL) {
		
		g_main_loop_quit(plc->loop);
	
	} else {

		g_critical ("Unable to stop Gtreamer pipeline. Loop was null.");
	
	}
	
	plc->is_stopped = true;

	printf ("Did terminate pipeline: %s \n", plc->pipelineString);

	return true;

}


int _ext_stop_gstream(struct PLC* plc) {

	return stopGstPipeLine(plc);

}

int _ext_play_gstream(struct PLC* plc) {

	return playGstPipeLine(plc);
}

void* _ext_create_gstream(const char* pipelineString) {

	return (void*) createPipeLine (pipelineString);	

}

int _ext_destroy_gstream(struct PLC* plc) {

	return destroyPipeLine(plc);
}

int _ext_init_gstream(struct PLC* plc) {

	return initGstPipeLine (plc);	

}

int _ext_is_playing_gstream (struct PLC* plc) {

	return plc->is_playing;

}

int _ext_is_initialized_gstream (struct PLC* plc) {

	return plc->is_initialized;

}


int main (int argc, char *argv[])
{
	
	struct PLC *plc = createPipeLine(argv[1]);

	if (!initGstPipeLine(plc)) {

		return 1;

	}

	if (playGstPipeLine(plc)) {
		
		
		stopGstPipeLine(plc);
		destroyPipeLine(plc);
	
		return 0;	
	
	}

	destroyPipeLine(plc);

 	return 1;

}

