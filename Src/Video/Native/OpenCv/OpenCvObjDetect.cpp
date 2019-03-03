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

#include "OpenCvObjDetect.hpp"

CvRect zero_rect_make();
CaptureObject create_capture_object();

#define SCALE_DELIMITER 16384

//bool try_detect_object (Mat image, CaptureObjectsContainer *capture_objects, HaarCascade cascade, bool saveImage, CvRect roi) {
//	IplImage ipl = image;
//	IplImage *iplImage = &ipl); 
//
//	return try_detect_object (iplImage, capture_objects, cascade, saveImage,roi);
//}

bool try_detect_object (IplImage* input_image, CaptureObjectsContainer *capture_objects, HaarCascade cascade, bool saveImage, CvRect roi) {

	//IplImage* input_image = image;
	
	//printf (" -1 ");
	if (!input_image || !input_image->width || !input_image->height) {
		fprintf(stderr, "no frame detected when capturing for object");
		return false;
	} /*else if (input_image->width > 640 || input_image->height > 480) {
		fprintf(stderr, "Bad input image width (%i) or height (%i). These values are currently hardcoded and required to be less than 640x480.", input_image->width, input_image->height);
		return false;
	}*/
	
	IplImage* image =  cvCreateImage(cvGetSize(input_image), IPL_DEPTH_8U, 1);
	
	cvCvtColor(input_image, image, CV_RGB2GRAY);

	//TODO: test input image size before scaling
	int scale = image->width * image->height > SCALE_DELIMITER ? 2 : 1;

	IplImage *small_image = cvCreateImage(cvSize(image->width/scale,image->height/scale), image->depth, image->nChannels);

	//printf (" scale: %d ", scale);
	//Scale down the searchable image to half the size:
	if (scale > 1) {
		cvPyrDown(image, small_image, CV_GAUSSIAN_5x5);
	} else {
		
		 cvCopy (image, small_image);
	}
	
	if (roi.x >= 0 && roi.y >= 0 && roi.width > 0 && roi.height > 0) {
		roi.x = roi.x / scale;
		roi.y = roi.y / scale;
		roi.width = roi.width / scale;
		roi.height = roi.height / scale;

		/*
		if (roi.width + roi.x > small_image->width) {
			roi.width = small_image->width - roi.x;
		}

		if (roi.height + roi.y > small_image->height) {
			roi.height = small_image->height - roi.y;
		}
		*/
		
		if (!(roi.width >= 0 && roi.height >= 0 && roi.x < small_image->width && roi.y < small_image->height && roi.x + roi.width >= (int)(roi.width > 0) && roi.y + roi.height >= (int)(roi.height > 0))) {
				printf (" ------ ------- \n");	
				printf (" ------ THIS WILL BE MY 2000 DEATH! ------- \n");
				printf (" roi: %i,%i - %i,%i \n", roi.x , roi.y , roi.width , roi.height );
				printf (" ROI-roi:    %i,%i - %i,%i \n", roi.x , roi.y , roi.width + roi.x, roi.height + roi.y);
				printf (" Small Image:    %i,%i \n",  small_image->width , small_image->height);
				printf (" ------ ------- \n");	
				printf (" ------ ------- \n");
			}
		cvSetImageROI(small_image, roi);
		//printf (" ROI: %i,%i - %i,%i ", roi.x , roi.y , roi.width, roi.height);
		
	} 
	//

	CvMemStorage* storage = cvCreateMemStorage(0);
	//printf (" 1 \n");
	CvSeq* haar_objects = cvHaarDetectObjects (
		small_image, 
		cascade.classifier, 
		storage,
		1.2f, 
		2, 
		cascade.options, 
		cvSize( small_image->width / 8, 
				small_image->width / 8),
		cvSize( small_image->width, 
				small_image->width)
	);

	//_ext_create_dump ("tmp2.jpg", small_image);
	cvReleaseImage(&small_image);
	
	
	capture_objects->size = haar_objects->total;
	int size = capture_objects->size * sizeof( CaptureObject);
	//printf (" 3  - size: %i ", size);
	capture_objects->objects = (CaptureObject*) malloc(size); //(CaptureObject *)

	//printf (" 4 %p %i", cascade.classifier, cascade.tag);

	//currently only checking the first element
	for (int i = 0; i < haar_objects->total; i++) {

		CvRect rect = *(CvRect*)cvGetSeqElem(haar_objects, i);
		rect.x = scale * (rect.x + roi.x);
		rect.y = scale * (rect.y + roi.y);
		rect.width *= scale;
		rect.height *= scale;
		//printf (" 5 ");
		CaptureObject obj = create_capture_object();

		obj.capture_bounds = rect;
		obj.did_capture = true;
		
		capture_objects->objects[i] = obj;
		//malloc (sizeof(CaptureObject));
		
		//memcpy(&capture_objects->objects[i], &obj, sizeof(CaptureObject));
		
		//if (obj.captured_image) {
		//	cvReleaseImage(&(obj.captured_image));
		//	obj.captured_image = 0;
		//}
		
		//printf ("6: %i %p %i", rect.width, cascade.classifier, cascade.tag );
		if (saveImage) {
			
			//IplImage *tmp =  cvCreateImage(cvSize(rect.width, rect.height), image->depth, image->nChannels);
			if (!(rect.width >= 0 && rect.height >= 0 && rect.x < image->width && rect.y < image->height && rect.x + rect.width >= (int)(rect.width > 0) && rect.y + rect.height >= (int)(rect.height > 0))) {
				printf (" ------ ------- \n");	
				printf (" ------ THIS WILL BE MY DEATH! ------- \n");
				printf (" rect: %i,%i - %i,%i \n", rect.x , rect.y , rect.width , rect.height );
				printf (" ROI-rect:    %i,%i - %i,%i \n", roi.x , roi.y , roi.width + roi.x, roi.height + roi.y);
				printf (" Image:    %i,%i \n",  image->width , image->height);
				printf (" ------ ------- \n");	
				printf (" ------ ------- \n");
			}
			capture_objects->objects[i].captured_image = cvCreateImage(cvSize(rect.width, rect.height), image->depth, image->nChannels);
			//printf (" rect: %i,%i - %i,%i \n", rect.x , rect.y , rect.width + rect.x, rect.height + rect.y);
			//printf (" ROI-rect:    %i,%i - %i,%i ", roi.x , roi.y , roi.width + roi.x, roi.height + roi.y);
			
			//printf (" 7 dumpi:");
			cvSetImageROI(image, cvRect(rect.x, rect.y, rect.width, rect.height));
			//printf (" 7.1 ");
			//cvCvtColor(image, tmp, CV_RGB2RGBA);
			cvCopy (image, capture_objects->objects[i].captured_image);
			cvResetImageROI(image);
			//printf (" 7.2 ");
			//capture_objects->objects[i].captured_image = tmp;
			//_ext_create_dump ("tmp2.jpg", capture_objects->objects[i].captured_image);

		} else {
			capture_objects->objects[i].captured_image = 0;
		}
		
		
	}
	
	cvReleaseMemStorage(&storage);
	cvReleaseImage(&image);
	//printf (" 8 ");
	return capture_objects->size > 0;
}



CvRect zero_rect_make() {
	return cvRect(0,0,0,0);
}





HaarCascade create_haar_cascade(const char* filename, int tag) {
	FILE *fh;
	fh = fopen(filename, "rb");
	HaarCascade cascade;
	
	if (fh == NULL) {
		fprintf(stderr,"Could not open '%s' haar file\n", filename);

	} else {

		fclose(fh);

		cascade.classifier = (CvHaarClassifierCascade*)cvLoad(filename, NULL, NULL, NULL);
		cascade.tag = tag;
		cascade.options = CV_HAAR_DO_CANNY_PRUNING 
			//| CV_HAAR_DO_ROUGH_SEARCH 
			| CV_HAAR_FIND_BIGGEST_OBJECT;
	
	}

	return cascade;
}

CaptureObject create_capture_object() {
	CaptureObject obj;
	obj.initialized = true;
	obj.did_capture = false;
	obj.capture_bounds = zero_rect_make();
	obj.captured_image = 0;

	return obj;

}

void release_capture_object (CaptureObject* obj) {
	if (obj->captured_image && 
		obj->captured_image->imageSize == obj->captured_image->height * obj->captured_image->widthStep //Sanity check
		) {
		cvReleaseImage(&(obj->captured_image));
	}

	obj->initialized = false;
	obj->capture_bounds = zero_rect_make();
	obj->did_capture = false;
}

void _ext_release_haar_cascade (HaarCascade* cascade) {
	if (cascade->classifier) {
		cvReleaseHaarClassifierCascade(&(cascade->classifier));
	}
	cascade->tag = 0;
}



void _ext_release_capture_object_array(CaptureObjectsContainer* objectArray) {
	
	for (int i= 0; i < objectArray->size; i++) {
		release_capture_object( &(objectArray->objects[i]));
	}


}

bool _ext_haar_capture (IplImage* image, CaptureObjectsContainer* objectArray) {
	
	if (objectArray->size != 0 && objectArray->saveImage) {
		for (int i = 0; i < objectArray->size; i++) {
			release_capture_object( &(objectArray->objects[i]));
		}
	}

	objectArray->size = 0;

	return try_detect_object(image, objectArray, objectArray->cascade, objectArray->saveImage, objectArray->roi);

}

CaptureObject _ext_get_capture_object(CaptureObjectsContainer* array, int index) {
	//CaptureObject obj = array.objects[index];
	//printf (" get capture pointer to ");
	//return obj;
	CaptureObject captureObject = array->objects[index];

	//printf ("img: %p\n", captureObject.captured_image);

	return captureObject;
}

HaarCascade _ext_create_haar_cascade(const char* file_name, int tag) {
	//HaarCascade cascade = create_haar_cascade(file_name, tag);
	//HaarCascade *cascadePtr = malloc (sizeof(HaarCascade));
	//memcpy(cascadePtr, &cascade, sizeof(HaarCascade));

	//return cascadePtr;
	tag = 42;
	return create_haar_cascade(file_name, tag);
}

void _ext_hej(CaptureObjectsContainer* array) {
	printf ("SIZE IN HEJ: %i\n", array->size);
	array->size = 42;
}

//void _ext_img_ptr (CaptureObject* obj) {
//		printf ("POINTER: %p\n", obj->captured_image);
//}

IplImage* _ext_img_ptr (CaptureObject* obj) {
		return obj->captured_image;
		//printf ("POINTER: %p\n", obj->captured_image);
}

void _ext_testish2 () {
	printf ("OKEY \n");
}

void _ext_testish (HaarCascade cascade) {
	printf ("HEJ TAG: %i och hund: %s", cascade.tag, (cascade.classifier ? "JA HER FANS" : "nej"));
}

