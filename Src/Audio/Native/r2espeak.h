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

//initializes the player. returns 1 if successfull
int _ext_init_espeak();
//speaks the text (!)
void _ext_speak_espeak(const char* text);
//set the pause between words (-100 to 100)
void _ext_set_rate_espeak(int _rate);
//set the voice pitch (-100 to 100)
void _ext_set_pitch_espeak(int _pitch);
// returns the current pitch
int _ext_get_pitch_espeak();
// returns the current rate
int _ext_get_rate_espeak();
//stop the player and dealocate resources
void _ext_stop_espeak();
//just stops the player
void _ext_pause_espeak();
//returns 1 if the player is playing

int _ext_is_playing_espeak ();

bool _ext_is_initialized_espeak();
