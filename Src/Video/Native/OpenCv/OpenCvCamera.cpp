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

#include "OpenCvCamera.hpp"
//#include <opencv2/highgui/highgui.hpp>
//#include <opencv2/core.hpp>
//#include <opencv2/imgproc.hpp>
//#include <opencv2/videoio.hpp>
#include <opencv2/opencv.hpp>

#define MAX_CAMERAS 4

typedef struct CaptureDevices {

	CvCapture* capture; // the capture session.
	int id; // device id for hardware.
	int skipFrames; // number of frames to skip before returning (thus omitting buffer).
	bool started; // true if started

} CaptureDevice;

CaptureDevice capture_devices[MAX_CAMERAS];

CaptureDevice* get_capture_device(int id);
IplImage* clone_frame_from_camera (CaptureDevice *device);

IplImage* clone_frame_from_camera (CaptureDevice *device) {

	int i = 0;
	while (i++ < device->skipFrames && cvGrabFrame(device->capture)) { }
	
	IplImage* frame = cvRetrieveFrame(device->capture);

        if ( !frame ) {
		
		printf (" ** WARNING - Unable to capture frame.\n");
		return NULL;
	
	}

	return cvCloneImage(frame);

}

CaptureDevice* get_capture_device(int deviceId) {

	for (int i = 0; i < MAX_CAMERAS; i++) {

		if (capture_devices[i].id == deviceId && capture_devices[i].started) {

			return &capture_devices[i];

		}

	}

	return NULL;
	
}

CvSize _ext_get_video_size(int deviceId) {

	CvSize size;

	CaptureDevice* device = get_capture_device(deviceId);

	if (device) {

		size.width = cvGetCaptureProperty(device->capture, CV_CAP_PROP_FRAME_WIDTH);
		size.height = cvGetCaptureProperty(device->capture, CV_CAP_PROP_FRAME_HEIGHT);
	}

	return size;

}



void _ext_stop_capture(int deviceId) {

	CaptureDevice* device = get_capture_device(deviceId);

	if (device) {

		cvReleaseCapture(&device->capture);
		device->capture = NULL;
		device->started = false;

	}

}

bool _ext_is_running_capture(int deviceId) {

	CaptureDevice* device = get_capture_device(deviceId);

	if (device && device->capture != NULL) {

		return true;

	}

	return false;

}

bool _ext_start_capture(int deviceId, CvSize size, int skipFrames) {

	if (get_capture_device(deviceId)) {

		printf(" ** WARNING Device id '%d' is in use. \n", deviceId);
		return false;

	}

	int capture_device_index = -1;

	for (int i = 0; i < MAX_CAMERAS; i++) {

		if (!capture_devices[i].started) {

			capture_device_index = i;
			capture_devices[i].started = true;
			capture_devices[i].id = deviceId;
			capture_devices[i].capture = cvCreateCameraCapture(CV_CAP_ANY);
			capture_devices[i].skipFrames = skipFrames;
			capture_device_index = i;
			break;
	
		}

	}

	if (capture_device_index == -1) {

		printf(" ** WARNING Unable to initiate capture. All devices in use. \n");
		return false;

	} else if ( !capture_devices[capture_device_index].capture ) { 

		printf (" ** WARNING - Unable to capture image. capture was null. \n");
		return false;

	}

	if (size.width > 0 && size.height > 0) {

		cvSetCaptureProperty(capture_devices[capture_device_index].capture , CV_CAP_PROP_FRAME_WIDTH, size.width);
		cvSetCaptureProperty(capture_devices[capture_device_index].capture , CV_CAP_PROP_FRAME_HEIGHT, size.height);

	}


	return true;

}

// Lazy initialization
IplImage* _ext_capture_camera(int deviceId) {

	CaptureDevice* device = get_capture_device(deviceId);

	if (device) {
		
		return clone_frame_from_camera (device);

	}

	printf (" ** WARNING - Unable to capture image. device not started?. \n");

	return NULL;
	
}
