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

#include "OpenCvFaceRecognize.hpp"

const double FACE_ELLIPSE_CY = 0.40;
const double FACE_ELLIPSE_W = 0.50; // Should be atleast 0.5
const double FACE_ELLIPSE_H = 0.80; // Controls how tall the face mask is.

const double FACE_FILTER_SIGMA_COLOR = 20.0;
const double FACE_FILTER_SIGMA_SPACE = 2.0;

// Histogram Equalize seperately for the left and right sides of the face.
// taken from: https://github.com/MasteringOpenCV/code/blob/master/Chapter8_FaceRecognition/preprocessFace.cpp
void FaceRec::equalizeLeftAndRightHalves(Mat &faceImg)
{
    // It is common that there is stronger light from one half of the face than the other. In that case,
    // if you simply did histogram equalization on the whole face then it would make one half dark and
    // one half bright. So we will do histogram equalization separately on each face half, so they will
    // both look similar on average. But this would cause a sharp edge in the middle of the face, because
    // the left half and right half would be suddenly different. So we also histogram equalize the whole
    // image, and in the middle part we blend the 3 images together for a smooth brightness transition.

    int w = faceImg.cols;
    int h = faceImg.rows;

    // 1) First, equalize the whole face.
    Mat wholeFace;
    equalizeHist(faceImg, wholeFace);

    // 2) Equalize the left half and the right half of the face separately.
    int midX = w/2;
    Mat leftSide = faceImg(Rect(0,0, midX,h));
    Mat rightSide = faceImg(Rect(midX,0, w-midX,h));
    equalizeHist(leftSide, leftSide);
    equalizeHist(rightSide, rightSide);

    // 3) Combine the left half and right half and whole face together, so that it has a smooth transition.
    for (int y=0; y<h; y++) {
        for (int x=0; x<w; x++) {
            int v;
            if (x < w/4) { // Left 25%: just use the left face.
                v = leftSide.at<uchar>(y,x);
            }
            else if (x < w*2/4) { // Mid-left 25%: blend the left face & whole face.
                int lv = leftSide.at<uchar>(y,x);
                int wv = wholeFace.at<uchar>(y,x);
                // Blend more of the whole face as it moves further right along the face.
                float f = (x - w*1/4) / (float)(w*0.25f);
                v = cvRound((1.0f - f) * lv + (f) * wv);
            }
            else if (x < w*3/4) { // Mid-right 25%: blend the right face & whole face.
                int rv = rightSide.at<uchar>(y,x-midX);
                int wv = wholeFace.at<uchar>(y,x);
                // Blend more of the right-side face as it moves further right along the face.
                float f = (x - w*2/4) / (float)(w*0.25f);
                v = cvRound((1.0f - f) * wv + (f) * rv);
            }
            else { // Right 25%: just use the right face.
                v = rightSide.at<uchar>(y,x-midX);
            }
            faceImg.at<uchar>(y,x) = v;
        }// end x loop
    }//end y loop
}

Mat FaceRec::createFiltered (Mat scaledFaceImage) {
	Mat filtered;
	
	filtered = Mat(scaledFaceImage.size(), CV_8U);
    bilateralFilter(scaledFaceImage, 
					filtered, 0, 
					FACE_FILTER_SIGMA_COLOR, 
					FACE_FILTER_SIGMA_SPACE);

	return filtered;
	 
}

Mat FaceRec::createElipse (Mat scaledFaceImage, CvRect leftEye, CvRect rightEye, CvRect mouth, float scaleModifier) {

	Point p1 = isEmpty(leftEye) ? Point(0,0) : Point(leftEye.x + leftEye.width/2, leftEye.y + leftEye.height/2);
	Point p2 = isEmpty(rightEye) ? Point(scaledFaceImage.size().width,0) : Point(rightEye.x + rightEye.width/2, rightEye.y + leftEye.height/2);
	Point p3 = isEmpty(mouth) ? Point(scaledFaceImage.size().width / 2, scaledFaceImage.size().height) : Point(mouth.x + mouth.width / 2, mouth.y + mouth.height/2);

	int ry = (int) ((float)(p1.y < p2.y ? p1.y : p2.y)) / scaleModifier;
	int rx = (int) ((float)(p1.x)) / scaleModifier;
	int rh = (int) ((float)p3.y - ry) / scaleModifier;
	int rw = (int) ((float)p2.x - rx) / scaleModifier;

	printf ("IMAGE: %d,%d : p1 %d,%d p2 %d,%d, p3 %d,%d  - rx: %d ry: %d, rw: %d, rh: %d \n", 
			scaledFaceImage.size().width, scaledFaceImage.size().height, 
			p1.x, p1.y, 
			p2.x, p2.y, 
			p3.x, p3.y, 
			rx, ry, rw, rh);
 
	Mat mask = Mat(scaledFaceImage.size(), CV_8U, Scalar(0)); // Start with an empty mask.
	Point faceCenter = Point( rx + rw / 2, ry + rh / 2);
	Size size = Size(rw,rh);

	//faceCenter = Point (41,41);
	//size = Size (scaledFaceImage.size().width, scaledFaceImage.size().height);
	ellipse(mask, faceCenter, size, 0, 0, 360, Scalar(255), CV_FILLED);

	// Use the mask, to remove outside pixels.
	Mat dstImg = Mat(scaledFaceImage.size(), CV_8U, Scalar(128)); // Clear the output image to a default gray.

	// Apply the elliptical mask on the face.
	scaledFaceImage.copyTo(dstImg, mask); // Copies non-masked pixels from filtered to dstImg.

	return dstImg;
}


int FaceRec::detectFeatures (Mat mat,
		CaptureObjectsContainer *leftEye,
		CaptureObjectsContainer *rightEye,
		CaptureObjectsContainer *mouth ) {

	if (m_mouthCascade.classifier == 0 || m_rightEyeCascade.classifier == 0 || m_leftEyeCascade.classifier == 0) {
		throw FaceRecException (" Yo have to set the classifiers to use detectFeatures! "); 
	}

	IplImage ipl = mat;
	IplImage *image = &ipl; 
	CvRect leftEyeRoi = cvRect (0,0,mat.size().width / 2, mat.size().height / 2);
	CvRect rightEyeRoi = cvRect (mat.size().width / 2, 0,mat.size().width / 2, mat.size().height / 2);
	CvRect mouthRoi = cvRect (0,mat.size().height / 2, mat.size().width, mat.size().height / 2);
	
	int result = 0;
	result += try_detect_object (image, mouth, 	m_mouthCascade, 	false, mouthRoi) ? 1 : 0;
	result += try_detect_object (image, rightEye, m_rightEyeCascade, 	false, rightEyeRoi) ? 1 : 0;
	result += try_detect_object (image, leftEye, 	m_leftEyeCascade, 	false, leftEyeRoi) ? 1 : 0;

	return result;	
	
}
