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

#include "OpenCvBase.hpp"
#include <stdbool.h>
//ERU3190N
typedef struct HaarCascade {
	
	int tag;
	int options;
	CvHaarClassifierCascade* classifier;		//the haar cascade

} HaarCascade;

typedef struct CaptureObject {
	
	CvRect capture_bounds;				//the latest capture for this object
	bool did_capture;					//did the latest try to capture result in a capture?
	bool initialized;					//is this CaptureObject loaded?
	IplImage *captured_image;				//capturedImage (if any) stored in RGBA format

} CaptureObject;

typedef struct CaptureObjectsContainer {
	int size;
	bool saveImage; 
	CvRect roi;
	HaarCascade cascade;

	CaptureObject* objects;
} CaptureObjectsContainer;

//used internally by the FaceRec
bool try_detect_object (IplImage* image, CaptureObjectsContainer *capture_objects, HaarCascade cascade, bool saveImage, CvRect roi);

extern "C" {
HaarCascade _ext_create_haar_cascade(const char* filename, int tag);
bool _ext_haar_capture (IplImage* image, CaptureObjectsContainer* array);
void _ext_release_haar_cascade (HaarCascade* cascade);
void _ext_release_capture_object_array(CaptureObjectsContainer* array);
CaptureObject _ext_get_capture_object(CaptureObjectsContainer* array, int index);
IplImage * _ext_img_ptr (CaptureObject* obj);
//void _ext_init(int input_width, int input_height);

void _ext_hej(CaptureObjectsContainer* array);
void _ext_testish (HaarCascade cascade);
extern void _ext_testish2 ();

}
