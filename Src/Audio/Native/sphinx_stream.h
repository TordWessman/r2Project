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

/**
*
*
*
*/

#include <gst/gst.h>
#include <stdbool.h>
#include <stdio.h>
#include <string.h>
#include <math.h>

#define ASR_ERROR_INIT_ALSA_FAILED 1024
#define ASR_ERROR_INIT_RESAMPLER_FAILED 2048
#define ASR_ERROR_INIT_VADER_FAILED 4096
#define ASR_ERROR_INIT_POCKETSPHINX_FAILED 8192
#define ASR_ERROR_LINK_FAILED 16384
#define ASR_ERROR_GST 32768
#define ASR_WARNING 65536
#define ASR_EOS 131072
#define ASR_ERROR_INIT_SINK_FAILED 262144
#define ASR_ERROR_INIT_CONVERTER_FAILED 524288
#define ASR_ERROR_INIT_IIRFILTER_FAILED 1048576
#define ASR_ERROR_INIT_AMPLIFIER_FAILED 2097152

/*if true = callback methods will be called upon receiving data */
void set_is_active (bool report_mode);
bool get_is_active ();

/** sends a message to the pipeline in order to turn it of **/
int asr_turn_off();

/** 	the asr_start fires up the engine and makes it ready to receive audio from the microphone
	and to fire delegate methods.
**/
int asr_start ();

/**
	asr_init is the main initialization method.
	params: 
	*) textReceivedCallback: is the delegate method to which an interpreted message will be sent.
	*) reportErrorCallback: is the delegate method to which errors will be reported
	*) lmFile: is the path to the language model file
	*) dictFile: is the path to the dictionary file
	*) hmmFile: is the path to the file containing the audio vocabulary file
**/
int asr_init (const char*(*textReceivedCallback)(const char *message ),
		  const char*(*reportErrorCallback)(int type, const char *message),
		  const char *lmFile, const char *dictFile, const char *hmmFile, int port, const char* hostIp);
		  
void asr_set_text_received_callback(const char*(*text_received_callback)(const char *message ));
void asr_set_report_error_callback (const char*(*report_error_callback)(int type, const char *message));
bool get_is_running ();

