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
#include <stdio.h>

extern "C" {

	void _ext_release_image(IplImage *image);

	// Captures a frame from webcam. skipFrames determines how many frames to discard from the video buffer (v4l issues..)
	IplImage* _ext_capture_camera(int deviceId);
	CvSize _ext_get_video_size(int deviceId);
	void _ext_stop_capture(int deviceId);
	bool _ext_start_capture(int deviceId, CvSize size, int skipFrames);
	bool _ext_is_running_capture(int deviceId);

}
