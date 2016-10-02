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

//#include <opencv2/opencv.hpp>
#include <opencv2/core/core.hpp>
#include <opencv2/contrib/contrib.hpp>
#include <opencv2/highgui/highgui.hpp>
#include "OpenCvObjDetect.hpp"

using namespace std;
using namespace cv;

#define FACE_REC_EQUALIZE_LEFT_RIGHT 2
#define FACE_REC_EQUALIZE_NORMAL 	 1
#define FACE_REC_EQUALIZE_NONE 		 0
#define FACE_REC_EIGEN	0
#define FACE_REC_LBPH	1
#define FACE_REC_FISHER	2
#define FACE_REC_DEFAULT_IMAGE_SIZE 112	//the size of the scaled (down) image
#define FACE_REC_DEFAULT_FEATURE_REQUIREMENT 1

#define FACE_REC_NOT_IN_MODEL -1.0

extern "C" {
	void _ext_init (HaarCascade left_eye, HaarCascade right_eye, HaarCascade mouth);
	void _ext_train (const char* output_model_file_name, const char* file_names[], int ids[], int count, int algorithm);
	double _ext_predict (IplImage *image, int modelId, double threshold);
	void _ext_set_size (size_t size);
	void _ext_set_equalization (int eq_type);
	void _ext_set_elipse (bool use_elipse);
	void _ext_set_use_filter (bool use_filter);
	void _ext_test (IplImage *image, int id);
	void _ext_set_feature_requirement (int number_of_features);
	void _ext_load_model(int model_id, int algorithm, const char*input_file);
	IplImage* _ext_prepare_face (IplImage* face_image, int equalizationType, int useElipse, int minimumFeaturesRequirement);
}


struct FaceRecException : public exception
{
	string m_message;
	FaceRecException(string message) : m_message(message) {}
	FaceRecException(string message, int value) {
		stringstream ss;
		ss << message << value;
		m_message = ss.str();
	}
	~FaceRecException() throw () {}
	const char* what() const throw() { return m_message.c_str(); }
};

class FaceRec
{
private:
	string m_outputDir;
	
	//Values used for preprocessing face images before model rendering
	int m_equalizationType;
	bool m_useElipse;
	bool m_useFilter;
	int m_minimumFeaturesRequirement;

	HaarCascade m_leftEyeCascade;
	HaarCascade m_rightEyeCascade;
	HaarCascade m_mouthCascade;

	map<int,Ptr<FaceRecognizer> > m_models;

	//The size of the pre-processed image
	size_t m_size;
	
	Mat prepareImage (Mat &image,  CvRect leftEye, CvRect rightEye, CvRect mouth, int equalizationType, int useElipse);

	void save_tmp (const char* name, Mat mat);
	void equalizeLeftAndRightHalves(Mat &faceImg);

	//returns the number of features detected
	int detectFeatures (Mat mat,
		CaptureObjectsContainer *leftEye,
		CaptureObjectsContainer *rightEye,
		CaptureObjectsContainer *mouth);

	
	Ptr<FaceRecognizer> createModel(int algorithm);
	Mat createElipse (Mat scaledFaceImage, CvRect leftEye, CvRect rightEye, CvRect mouth, float scaleModifier);
	Mat createFiltered (Mat scaledFaceImage);
	Mat resizeAndCvtColor (Mat sourceImage);
	Mat equalize (Mat sourceImage, int equalizationType);

public:
	FaceRec ();

	//caches and prepares a model. the saved model file (inputFile) must have been created using the same algorithm.
	void loadModel(int modelId, int algorithm, const char*inputFile);

	//Sets the size of the image for the pre-processing method
	void setImageSize (size_t size);

	//use bilateralFilter
	void setUseFilter (bool useFilter);

	//try to mask out a face elipse using eye and mouth haars?
	void setUseElipse (bool useElipse);

	//use equalization
	void setEqualization (int equalizationType);

	//sets the number of features (2 eyes + mouth) required in order for accepting an image in a model set during pre-processing 
	void setFeatureRequirements (int numberOfFeatures);

	//set the haar cascades for eyes and mouth
	void setHaarCascades (HaarCascade leftEye, HaarCascade rightEye, HaarCascade mouth);

	//creates a model (outputFile) using fileNames[] images with ids[] and the specified algorithm
	void train (const char*outputFile, const char* fileNames[], int ids[], int count, int algorithm);

	//returns a confidence if image was predicted in the model  using threshold and algorithm and FACE_REC_NOT_IN_MODEL if not in model
	double predict (IplImage* image, int modelId, double threshold);

	//prepares an image of a face using the specified methods
	IplImage* prepareFace (IplImage* image, int equalizationType, int useElipse, int minimumFeaturesRequirement);

	void test (IplImage *image, int id);
	void setOutputDir (const char* outputDir);	
};


static FaceRec _engine;
