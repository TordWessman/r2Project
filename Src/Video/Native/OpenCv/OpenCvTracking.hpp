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
#include <opencv2/core/core.hpp>
#include <opencv2/contrib/contrib.hpp>
#include <opencv2/features2d/features2d.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2/imgproc/imgproc_c.h>
#include <opencv2/legacy/compat.hpp>
#include <opencv2/video/tracking.hpp>

#include<stdarg.h>
#include <iostream>
#include <ctype.h>

using namespace cv;
using namespace std;


const int TRACKER_MAX_COUNT = 50;
const double TRACKER_QUALITY_LEVEL = 0.01;
const double TRACKER_MIN_DISTANCE = 10;

struct TrackingException : public exception
{
	string m_message;
	TrackingException(string message) : m_message(message) {}
	TrackingException(string message, int value) {
		stringstream ss;
		ss << message << "VALUE:" << value;
		m_message = ss.str();
	}
	~TrackingException() throw () {}
	const char* what() const throw() { return m_message.c_str(); }
};

typedef struct Tracker {

	int detected_points_count;
	CvRect roi;

	IplImage* previous_frame;
	//vector<Point2f> points[2];
	Point2f* points;

} Tracker;

extern "C" {

	Tracker *_ext_create_tracker_roi (IplImage *frame, CvRect roi);
	Tracker *_ext_create_tracker (IplImage *frame);
	CvPoint _ext_get_point (Tracker *tracker, int number);
	void _ext_release_tracker (Tracker *tracker);
	int _ext_update_tracker (IplImage *frame, Tracker *tracker);
	int _ext_count_points(Tracker *tracker);
}



class PointsTracker {

private:

	TermCriteria m_termcrit;
	Size m_subPixWinSize;
	Size m_winSize;

public:
	PointsTracker();
	CvPoint GetPoint(Tracker* tracker, int number);
	Tracker* CreateTracker(IplImage *frame, CvRect roi);
	Tracker* CreateTracker(IplImage *frame);
	void ReleaseTracker (Tracker *tracker);
	int Update (IplImage *frame, Tracker *tracker);

};

static PointsTracker _tracker;
