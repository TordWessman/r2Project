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
#include <sstream>

using namespace std;

FaceRec::FaceRec () {
	m_equalizationType = FACE_REC_EQUALIZE_NONE;
	m_useElipse = false;
	m_size = FACE_REC_DEFAULT_IMAGE_SIZE;
	m_useFilter = false; 
	m_leftEyeCascade.classifier = 0;
	m_rightEyeCascade.classifier = 0;
	m_mouthCascade.classifier = 0;
	m_minimumFeaturesRequirement = FACE_REC_DEFAULT_FEATURE_REQUIREMENT;
}

void FaceRec::setFeatureRequirements (int numberOfFeatures) {
	m_minimumFeaturesRequirement = numberOfFeatures;
}

void FaceRec::setHaarCascades (HaarCascade leftEye, HaarCascade rightEye, HaarCascade mouth) {
	m_leftEyeCascade = leftEye;
	m_rightEyeCascade = rightEye;
	m_mouthCascade = mouth;
}

void FaceRec::setUseElipse (bool useElipse) {
	m_useElipse = useElipse;
}

void FaceRec::setUseFilter (bool useFilter) {
	m_useFilter = useFilter;
}

void FaceRec::setEqualization (int equalizationType) {

	if (equalizationType == FACE_REC_EQUALIZE_LEFT_RIGHT ||
		equalizationType == FACE_REC_EQUALIZE_NORMAL ||
		equalizationType == FACE_REC_EQUALIZE_NONE) {

		m_equalizationType = equalizationType;

	} else {
		throw FaceRecException ("Bad equalization type: " , equalizationType);

	}
}


Ptr<FaceRecognizer> FaceRec::createModel(int algorithm) {

	if (algorithm == FACE_REC_LBPH) {
		return createLBPHFaceRecognizer();
	} else if (algorithm == FACE_REC_FISHER){
		return createFisherFaceRecognizer();
	} else if (algorithm ==  FACE_REC_EIGEN) {
	 	return createEigenFaceRecognizer(); 
	} else {
		throw FaceRecException ("Bad face recognition algorithm type: " , algorithm);

	}
	
}

Mat FaceRec::resizeAndCvtColor (Mat image) {
	Mat resized;

	resize(image, resized, Size(m_size, m_size), 0, 0, INTER_CUBIC);

   	if (resized.channels() == 3 || resized.channels() == 4) {
		cvtColor(resized, resized, CV_BGR2GRAY);
	}

	return resized;

}

const char *getFileName (const char* name, int id) {
	stringstream ss;
	ss << "tmp/" << id << "_" << name << ".jpg";
	const std::string tmp = ss.str();
	return tmp.c_str();
}

Mat FaceRec::equalize (Mat resized, int type) {
	Mat mat = resized.clone();
	if (type ==  FACE_REC_EQUALIZE_LEFT_RIGHT) {
		equalizeLeftAndRightHalves(mat);	
	} else if (type ==  FACE_REC_EQUALIZE_NORMAL) {
		equalizeHist(resized, mat);	
	}

	return mat;
}

void FaceRec::test (IplImage *image, int id) {
	Mat original = Mat (image, true);
	Mat resized, elipse_eq, eq, elipse, eq_elipse;

	setImageSize (82);
	resized = resizeAndCvtColor (original);
	eq = equalize (resized, FACE_REC_EQUALIZE_LEFT_RIGHT);
	
	//eq_elipse = createElipse (eq);
	CvRect lx, rx, mx;
	CaptureObjectsContainer leftEye, rightEye, mouth;
	leftEye.size = rightEye.size = mouth.size = 0;

	if (detectFeatures (resized, &leftEye, &rightEye, &mouth) > 0) {
		printf ("!");
	} else {
		printf ("?");
	}
	
	lx = (leftEye.size > 0) ? leftEye.objects[0].capture_bounds : cvRect(0,0,0,0);
	rx = rightEye.size > 0 ? rightEye.objects[0].capture_bounds : cvRect(0,0,0,0);
	mx = mouth.size > 0 ? mouth.objects[0].capture_bounds : cvRect(0,0,0,0);
	elipse = createElipse (resized, lx, rx, mx, (float) resized.size().width / (float)m_size);

	if (detectFeatures (eq, &leftEye, &rightEye, &mouth) > 0) {
		printf ("!");
	} else {
		printf ("?");
	}

	lx = (leftEye.size > 0) ? leftEye.objects[0].capture_bounds : cvRect(0,0,0,0);
	rx = rightEye.size > 0 ? rightEye.objects[0].capture_bounds : cvRect(0,0,0,0);
	mx = mouth.size > 0 ? mouth.objects[0].capture_bounds : cvRect(0,0,0,0);
	eq_elipse = createElipse (eq, lx, rx, mx, (float) resized.size().width / (float)m_size);

	if (detectFeatures (resized, &leftEye, &rightEye, &mouth) > 0) {
		printf ("!");
	} else {
		printf ("?");
	}
	
	lx = (leftEye.size > 0) ? leftEye.objects[0].capture_bounds : cvRect(0,0,0,0);
	rx = rightEye.size > 0 ? rightEye.objects[0].capture_bounds : cvRect(0,0,0,0);
	mx = mouth.size > 0 ? mouth.objects[0].capture_bounds : cvRect(0,0,0,0);

	elipse_eq = equalize ( createElipse (resized, lx, rx, mx, ((float) resized.size().width /  (float) m_size)), FACE_REC_EQUALIZE_LEFT_RIGHT);
	
	save_tmp (getFileName("eq", id), eq);
	save_tmp (getFileName("elipse_eq", id), elipse_eq);
	save_tmp (getFileName("eq_elipse", id), eq_elipse);
	save_tmp (getFileName("elipse", id), elipse);
	//save_tmp (getFileName("eq_elipse_filtered", id), eq_elipse);
	resized.release();
	elipse_eq.release();
	eq.release();
	original.release();
	elipse.release();
	eq_elipse.release();
/*
	Mat clone = mat.clone();
	save_tmp (getFileName("filter", id), createFiltered(clone));
	clone.release();
	clone = mat.clone();
	save_tmp (getFileName("elipse", id), createElipse(clone));
	clone.release();
	clone = mat.clone();
	equalizeLeftAndRightHalves(clone);
	save_tmp (getFileName("eq_l_r_elipse_filter", id),	createElipse(createFiltered(clone)));
	clone.release();
	clone = mat.clone();
	equalizeHist(clone,clone);
	save_tmp (getFileName("eq", id),	clone);
	clone.release();
	clone = mat.clone();
	equalizeLeftAndRightHalves(clone);
	save_tmp (getFileName("eq_l_r", id),	clone);
	clone.release();
	mat.release();
*/
	
	
}

void FaceRec::setImageSize (size_t size) {
	m_size = size;
}


void _ext_set_size (size_t size) {
	_engine.setImageSize (size);
}

void _ext_set_equalization (int eq_type) {
	_engine.setEqualization (eq_type);
}

void _ext_set_elipse (bool use_elipse) {
	_engine.setUseElipse (use_elipse);
}
void _ext_set_use_filter (bool use_filter) {
	_engine.setUseFilter(use_filter);
}

void _ext_set_feature_requirement (int number_of_features) {
	_engine.setFeatureRequirements (number_of_features);
}

void _ext_train (const char* output_model_filename, const char* file_names[], int ids[], int count, int algorithm) {
	
	_engine.train (output_model_filename, file_names, ids, count, algorithm);
}

double _ext_predict (IplImage* image, int modelId, double threshold)  {
	return _engine.predict (image, modelId, threshold);
}


void _ext_load_model(int model_id, int algorithm, const char*input_file) {
	_engine.loadModel(model_id, algorithm, input_file);
}

IplImage* _ext_prepare_face (IplImage* face_image, int equalizationType, int useElipse, int minimumFeaturesRequirement) {
	return _engine.prepareFace(face_image, equalizationType, useElipse, minimumFeaturesRequirement);
}
void _ext_init ( HaarCascade left_eye, HaarCascade right_eye, HaarCascade mouth) {
	//cout << output_dir << endl;
	//_engine.setOutputDir (output_dir);
	_engine.setHaarCascades (left_eye, right_eye, mouth);
}

void _ext_test (IplImage *image, int id) {
	_engine.test (image, id);
}




