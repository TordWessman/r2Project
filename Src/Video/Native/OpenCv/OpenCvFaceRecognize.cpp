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
#include <iostream>
#include <fstream>
#include <sstream>

using namespace cv;
using namespace std;


struct timespec time_1, time_2;

void saveMat(Mat mat, int id) {

	string fn;
	ostringstream convert;  
	convert << "tmp/" << id << ".jpg";

	imwrite( convert.str(),mat);
}

int64_t timespecDiff(struct timespec *timeA_p, struct timespec *timeB_p)
{
  return ((timeA_p->tv_sec * 1000000000) + timeA_p->tv_nsec) -
           ((timeB_p->tv_sec * 1000000000) + timeB_p->tv_nsec);
}

void FaceRec::setOutputDir (const char* outputDir) {
		 m_outputDir = string(outputDir);
}

void FaceRec::loadModel(int modelId, int algorithm, const char*inputFile) {

	printf ("Loading model %i from file %s using algorithm %i\n", modelId, inputFile, algorithm);
	Ptr<FaceRecognizer> model = createModel(algorithm);
	model->load (string (inputFile));
	m_models[modelId] = model;
}

IplImage* FaceRec::prepareFace (IplImage* image, int equalizationType, int useElipse, int minimumFeaturesRequirement) {

	IplImage *tmpi = cvCloneImage (image);	
	Mat resized = resizeAndCvtColor(Mat (tmpi));
	if (tmpi) {
		cvReleaseImage(&tmpi);	
	}
	

	if (minimumFeaturesRequirement > 0) {

		CaptureObjectsContainer leftEye, rightEye, mouth;
		leftEye.size = rightEye.size = mouth.size = 0;
		if (detectFeatures (resized, 
							&leftEye, &rightEye, &mouth) >= minimumFeaturesRequirement) {

			CvRect lx = (leftEye.size > 0) ? leftEye.objects[0].capture_bounds : cvRect(0,0,0,0);
			CvRect rx = rightEye.size > 0 ? rightEye.objects[0].capture_bounds : cvRect(0,0,0,0);
			CvRect mx = mouth.size > 0 ? mouth.objects[0].capture_bounds : cvRect(0,0,0,0);
			IplImage *prepared = cvCloneImage(new IplImage(prepareImage (resized, lx,rx,mx, equalizationType, useElipse)));
			//_ext_create_dump ("prepared.jpg", prepared);
			return prepared;
			//return &prepared;
		} else {
			fprintf(stderr," ** WARNING ** number of required features not met (%i)\n", minimumFeaturesRequirement);

		}
	} else {

		IplImage *prepared = cvCloneImage(new IplImage(prepareImage (resized,  cvRect(0,0,0,0), cvRect(0,0,0,0), cvRect(0,0,0,0), equalizationType, useElipse)));
		//_ext_create_dump ("prepared.jpg", prepared);
		return prepared;
	}
	return NULL;
}

double FaceRec::predict (IplImage* image, int modelId, double threshold) {

	if (!image) {
		fprintf(stderr," ** WARNING ** Image was null in FaceRec::predict. \n");
		return false;
	}
	
	Mat prepared = Mat(image);
	//clock_gettime(CLOCK_MONOTONIC, &time_1);
	double confidence = 0.0;
	int predictedLabel = -1;

	if (m_models.count(modelId) == 0) {
		fprintf(stderr," ** WARNING ** Model with id: %i have not been loaded!\n", modelId);
		return false;
	}

//	m_models[modelId]->set("threshold", threshold);

	//clock_gettime(CLOCK_MONOTONIC, &time_2); printf("Model load time:  %lld\n", timespecDiff(&time_2, &time_1)); clock_gettime(CLOCK_MONOTONIC, &time_1);
	
	//IplImage *tmpi = cvCloneImage (image);	
	//Mat resized = resizeAndCvtColor(Mat (tmpi));
	//clock_gettime(CLOCK_MONOTONIC, &time_2); printf("Image preparation time:  %lld\n", timespecDiff(&time_2, &time_1)); clock_gettime(CLOCK_MONOTONIC, &time_1);

	//predictImage = prepareImage (predictImage);
	//saveMat(prepared,4242);
//    m_models[modelId]->predict(prepared, predictedLabel, confidence);
	//clock_gettime(CLOCK_MONOTONIC, &time_2); printf("Prediction time:  %lld\n", timespecDiff(&time_2, &time_1)); clock_gettime(CLOCK_MONOTONIC, &time_1);

	//resized.release();

	if (predictedLabel != -1) {
		//cout << "Confidence: " << confidence << endl;
		return confidence;	
	}

	return FACE_REC_NOT_IN_MODEL;
}

Mat FaceRec::prepareImage (Mat &image,  CvRect leftEye, CvRect rightEye, CvRect mouth, int equalizationType, int useElipse) {

	float scaleModifier = ((float)image.size().width) / ((float)m_size);
	Mat resized = resizeAndCvtColor(image);

	
	if (equalizationType ==  FACE_REC_EQUALIZE_LEFT_RIGHT) {
		equalizeLeftAndRightHalves(resized);	
	} else if (equalizationType ==  FACE_REC_EQUALIZE_NORMAL) {
		equalizeHist(resized, resized);	
	}

	Mat dst;

	if (useElipse) {
		
		dst = createElipse (resized, leftEye, rightEye, mouth, scaleModifier);
		resized.release();
		
	} else {
		dst = resized;
	}

	image.release();

	return dst;
}

void FaceRec::train (const char*outputFile, const char* fileNames[], int ids[], int count, int algorithm) {
	
	vector<Mat> images;
	vector<int> labels;
	
	int unQualifiedCount = 0;
	cout << "TRAING IMAGE USING equalization type: " << m_equalizationType
		<< " ELIPSE: " << m_useElipse
		<< " FILTER: " << m_useFilter
		<< " NO FEATURE REQUIREMENTS: " << m_minimumFeaturesRequirement
		<< " SIZE: " << m_size
		<< " ALGORITHM: " << algorithm
		<< endl;

	for (int i = 0; i < count; i++) {
		
		images.push_back(imread(fileNames[i], CV_LOAD_IMAGE_GRAYSCALE));
		CaptureObjectsContainer leftEye, rightEye, mouth;
		leftEye.size = rightEye.size = mouth.size = 0;

		if (detectFeatures (images[i - unQualifiedCount], 
							&leftEye, &rightEye, &mouth) >= m_minimumFeaturesRequirement) {

			CvRect lx = (leftEye.size > 0) ? leftEye.objects[0].capture_bounds : cvRect(0,0,0,0);
			CvRect rx = rightEye.size > 0 ? rightEye.objects[0].capture_bounds : cvRect(0,0,0,0);
			CvRect mx = mouth.size > 0 ? mouth.objects[0].capture_bounds : cvRect(0,0,0,0);

			labels.push_back(ids[i]);
			//Mat *tmp = &images[i - unQualifiedCount];
			images[i - unQualifiedCount] = prepareImage (images[i - unQualifiedCount], lx,rx,mx, m_equalizationType, m_useElipse);
			saveMat(images[i - unQualifiedCount],ids[i]);
			//tmp->release();
		} else {
			images[i - unQualifiedCount].release();
			images.pop_back();
			unQualifiedCount++;
		}
	}

	Ptr<FaceRecognizer> model = createModel(algorithm);

	model->train(images, labels);
	
	cout << "Saving output file: " << outputFile << endl;
	model->save (string (outputFile));


}

void FaceRec::save_tmp (const char* name, Mat mat) {
	std::vector<int> qualityType;
	qualityType.push_back(CV_IMWRITE_JPEG_QUALITY);
	qualityType.push_back(90);
	imwrite (name, mat, qualityType);

}



