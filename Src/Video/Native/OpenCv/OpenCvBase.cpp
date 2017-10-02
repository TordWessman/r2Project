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

void create_dump_file (const char* filename);

void _ext_create_dump(const char*filename, IplImage* image) {

	if (!image) {
		printf (" ** Warning - unable to create dump, since image was null (filename: %s)!\n",filename);
		return;	
	}	
		int p[3];
		
		p[0] = CV_IMWRITE_JPEG_QUALITY;
    		p[1] = 90;
    		p[2] = 0;
		
		
		printf("Saving image %s pointer: %p\n", filename, image);
		cvSaveImage(filename, image, p);
		printf("- saved image: %p\n", image);
}

IplImage* _ext_create_32_bit_image (IplImage* image) {
	
	if (image)
	{

		IplImage *tmp_frame = cvCreateImage(cvSize(image->width, image->height), IPL_DEPTH_8U, 4);
		cvCvtColor(image, tmp_frame, CV_RGB2RGBA);

		return tmp_frame;
	}

	return 0;
}

char* _ext_get_bitmap (IplImage* image) {

	if (image)
	{
		if (image->nChannels != 4) {
			printf (" ** ERROR - image was not 32 bit (use _ext_create_32_bit_image ) **");
			return 0;
		}

		
		return image->imageData;
	}

	return 0;
}

IplImage* _ext_equalize (IplImage *image) {

	if (!image) {
		return NULL;	
	}	
	IplImage *clone = image; 

	//_ext_create_dump("pre_equalize_bogdan.jpg", image);
	IplImage *tmp = cvCreateImage(cvSize(clone->width, clone->height), IPL_DEPTH_8U, 1);

	if (clone->depth != CV_8UC1) {
		cvCvtColor (clone,tmp,CV_RGB2GRAY);
		cvEqualizeHist(tmp, tmp);
	} else {
		cvEqualizeHist(clone, tmp);	
	}

	return tmp;	
	
}

void* _ext_load_image (const char* filename) {
	return cvLoadImage(filename, CV_LOAD_IMAGE_COLOR);
}

void _ext_release_ipl_image(IplImage* image) {
	if (image) {
		cvReleaseImage(&image);	
	}
	
}

short _ext_sharpnes (IplImage *image) {
	if (image) {
		return GetSharpness(image);
	}
	return 0;
}

bool isEmpty(CvRect rect) {
	return rect.x == 0 && rect.y == 0 && rect.width == 0 && rect.height == 0;
}


short GetSharpness(IplImage* in)
{
    // assumes that your image is already in planner yuv or 8 bit greyscale
    //IplImage* in = cvCreateImage(cvSize(width,height),IPL_DEPTH_8U,1);
    
	
	IplImage* out = cvCreateImage(cvSize(in->width, in->height),IPL_DEPTH_16S,1);
	IplImage* modif = cvCreateImage(cvSize(in->width, in->height),IPL_DEPTH_8U,1);
	
	
	if (in->nChannels == 3 || in->nChannels == 4) {
		//printf ("CHANGING COLORS\n");
		cvCvtColor(in, modif, CV_BGR2GRAY);
	} else {
		
		modif = cvCloneImage (in);
	}
    // aperture size of 1 corresponds to the correct matrix
	cvLaplace(modif, out, 1);

    short maxLap = -32767;
    short* imgData = (short*)out->imageData;
    for(int i =0;i<(out->imageSize/2);i++)
    {
        if(imgData[i] > maxLap) maxLap = imgData[i];
    }

    cvReleaseImage(&out);
    cvReleaseImage(&modif);
    return maxLap;
}

int _ext_get_image_width (IplImage *image) {

	if (image) { return image->width; }
	
	printf (" ** WARNING - IMAGE WAS NULL (_ext_get_image_width)\n");
	return 0;
	
}

int _ext_get_image_height (IplImage *image) {

	if (image) { return image->height; }
	
	printf (" ** WARNING - IMAGE WAS NULL (_ext_get_image_height)\n");
	return 0;
	
}

int _ext_get_image_bpp (IplImage *image) {

	if (image) { return image->widthStep; }
	
	printf (" ** WARNING - IMAGE WAS NULL (_ext_get_image_bpp)\n");
	return 0;

}

