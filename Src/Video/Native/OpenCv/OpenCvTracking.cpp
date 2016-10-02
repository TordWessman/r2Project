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

#include "OpenCvTracking.hpp"
#include "OpenCvBase.hpp"

PointsTracker::PointsTracker() {
	m_termcrit = TermCriteria(CV_TERMCRIT_ITER|CV_TERMCRIT_EPS,20,0.03);
	m_subPixWinSize = Size(10,10);
	m_winSize = Size(31,31);
}

CvPoint PointsTracker::GetPoint(Tracker* tracker, int number) {

	return cvPoint((int)tracker->points[number].x, (int)tracker->points[number].y);
}

Tracker* PointsTracker::CreateTracker(IplImage *frame, CvRect roi) {


	//_ext_create_dump ("frame1.jpg", frame);
	Tracker* tracker =  (Tracker* ) malloc (sizeof(Tracker));

	if (roi.x + roi.width > frame->width || roi.y + roi.height > frame->height) {
		char xxx[255];
		sprintf(xxx,"Roi outside image %i, %i - %i, %i (frame: %i, %i) " , roi.x, roi.y, roi.width, roi.height, frame->width, frame->height);
		throw TrackingException (string(xxx));
	}

	

	Mat imageROI = Mat(cvSize(frame->width, frame->height),CV_8UC1);
	Mat image, gray;

	int xSize = imageROI.cols;

	//printf ("\nROI: %i, %i, %i, %i -%i- \n\n", roi.x, roi.y, roi.width, roi.height, xSize);



	for(int x=0; x<frame->width; x++){
	   for(int y=0; y<frame->height; y++) {
			size_t pos =  xSize*y + x;
			if (x >= roi.x && x <= roi.width + roi.x && y >= roi.y && y <= roi.height + roi.y) {
				imageROI.data[pos] = 1;
			} else {
				imageROI.data[pos] = 0;
			}
		      
		}
	}
	
	//
	image = Mat(frame);
	cvtColor(image, gray, CV_BGR2GRAY);

	
	vector<Point2f> points;
	
	goodFeaturesToTrack(gray, points, TRACKER_MAX_COUNT, TRACKER_QUALITY_LEVEL, TRACKER_MIN_DISTANCE, imageROI, 3, 0, 0.04);
	
	if (points.size() > 0) {
		cornerSubPix(gray, points, m_subPixWinSize, Size(-1,-1), m_termcrit);
	} else {
		tracker->detected_points_count = 0;
		fprintf(stderr, " ** WARNING No points tracked (need to resize ROI?)\n");
		return tracker;
	}
	tracker->previous_frame = cvCloneImage(new IplImage(gray));

	tracker->points = (Point2f *) malloc (sizeof(Point2f) * points.size());

	for (size_t i = 0; i < points.size(); i++) {
		tracker->points[i] = points[i];
		//printf ("POINT: %i,%i...\n", (int)points[i].x, (int)points[i].y);
	}

	tracker->detected_points_count = points.size();

	return tracker;

	
}

Tracker* PointsTracker::CreateTracker(IplImage *frame) {

	return CreateTracker(frame, cvRect(0,0, frame->width, frame->height));
}

int PointsTracker::Update (IplImage *frame, Tracker* tracker) {

	if (!frame) {
		fprintf(stderr, " ** WARNING frame was null in Update\n");
		return 0;
	}

	if (!tracker) {
		fprintf(stderr, " ** WARNING tracker was null Update\n");
		return 0;
	}

	if (tracker->detected_points_count == 0) {
		return 0;
	}

	Mat gray;
	
	Mat image = Mat (frame);
		
	vector<uchar> status;
	vector<float> err;
	   
	cvtColor(image, gray, CV_BGR2GRAY);

	vector<Point2f> points_in, points_out;

	for (int i = 0; i < tracker->detected_points_count;i++) {
		points_in.push_back(tracker->points[i]);
	}

	free(tracker->points);

	calcOpticalFlowPyrLK(Mat(tracker->previous_frame), gray, points_in, points_out, status, err, m_winSize,
                                 3, m_termcrit, 0, 0.001);
	vector<Point2f> ok_points;

	for (size_t i = 0; i < points_out.size(); i++) {
		if (status[i]) {
		
			ok_points.push_back(points_out[i]);
		}
	}

	tracker->detected_points_count = ok_points.size();

	tracker->points = (Point2f *) malloc (sizeof(Point2f) * ok_points.size());

	for (size_t i = 0; i < ok_points.size(); i++) {
		tracker->points[i] = ok_points[i];
	}

	if (tracker->previous_frame) {
		cvReleaseImage(&tracker->previous_frame);	
	}
	
	tracker->previous_frame = cvCloneImage(new IplImage(gray));

	return ok_points.size();
}


void PointsTracker::ReleaseTracker (Tracker* tracker) {
	if (!tracker) {
		fprintf(stderr, " ** WARNING unable to release tracker (was null)");
		return;
	}

	if (tracker->previous_frame) {
		cvReleaseImage(&tracker->previous_frame);	
	}

	if (tracker->points) {
		free(tracker->points);
	}

	free (tracker);
}

Tracker *_ext_create_tracker_roi (IplImage *frame, CvRect roi) {
	return _tracker.CreateTracker(frame, roi);
}

Tracker *_ext_create_tracker (IplImage *frame) {
	return _tracker.CreateTracker(frame);
}

CvPoint _ext_get_point (Tracker *tracker, int number) {
	return _tracker.GetPoint(tracker,number);
}

void _ext_release_tracker (Tracker *tracker) {
	_tracker.ReleaseTracker(tracker);
}

int _ext_update_tracker (IplImage *frame, Tracker *tracker) {
	return _tracker.Update(frame, tracker);
}

int _ext_count_points(Tracker* tracker) {
	return tracker->detected_points_count;
}



int main( int argc, char** argv )
{
    VideoCapture cap;
	 cap.open(0);

	Tracker *tracker = 0;
    if( !cap.isOpened() )
    {
        cout << "Could not initialize capturing...\n";
        return 0;
    }

    namedWindow( "LK Demo", 1 );
  	while(true) {
        Mat frame;
        cap >> frame;
        if( frame.empty() )
            break;

		if (!tracker) {
			tracker = _ext_create_tracker(new IplImage(frame) );

		} else {
			_ext_update_tracker(new IplImage(frame), tracker);
		}
		
		printf ("prickar: %i\n", tracker->detected_points_count);
		imshow("LK Demo", frame);

        char c = (char)waitKey(10);
        if( c == 27 )
            break;
  
	}	

}
