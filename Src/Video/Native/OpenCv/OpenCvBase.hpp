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

#include <opencv/cv.h>
#include <stdio.h>
#include <opencv/highgui.h>
#include <opencv/cxcore.h>

extern "C" {

	//creates a dump image using OpenCv
	void _ext_create_dump(const char*filename, IplImage* image);

	char* _ext_get_bitmap (IplImage* image);
	void* _ext_load_image (const char* filename);
	void _ext_release_ipl_image(IplImage* image);
	short _ext_sharpnes (IplImage *in);
	IplImage* _ext_equalize (IplImage *in);
	int _ext_get_image_width (IplImage *image);
	int _ext_get_image_height (IplImage *image);
	IplImage* _ext_create_32_bit_image (IplImage* image);

	// Captures a frame from webcam
	IplImage* _ext_capture_camera(int device);
}

short GetSharpness(IplImage* in);

bool isEmpty(CvRect rect);
