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
// feel free to do what you like with code.

#include <gst/gst.h>
#include <stdbool.h>
#include <stdio.h>
#include <stdint.h>

#define MP3_ERROR_PIPELINE_OBJECT_MISSING -1
#define MP3_ERROR_UNABLE_TO_LINK_PIPELINE -2

// Initializes the player. Returns an id to be used on subsequent calls if successfull.
int _ext_init_mp3();
// Stop the player and dealocate resources.
void _ext_stop_mp3(int id);
// Pause playback
void _ext_pause_mp3(int id);

// Returns 1 if the player is playing.
int _ext_is_playing_mp3 (int id);

// Intialize as a file player.
int _ext_init_mp3_file ();

// Initialize as a memory player.
int _ext_init_mp3_memory ();

// Play from file with location ´mp3_file´. Requires an initialization by ´_ext_init_mp3_file´.
void _ext_play_file_mp3(int id, const char* mp3_file);

// Play resource from memory. Require the ´_ext_init_mp3_memory´ to be called.
void _ext_play_memory_mp3(int id, uint8_t* mp3_pointer, int size);

void _ext_stop_playback_mp3(int id);
