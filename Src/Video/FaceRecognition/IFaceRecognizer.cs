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

using System;
using Core.Device;
using Core.Memory;

namespace Video
{
	public interface IFaceRecognizer : IDevice
	{
		/// <summary>
		/// Fetches face images associated with the nameMemory and trains 
		/// it using the pre-defined training conditions and the algorithm defined as: 
		/// #define FACE_REC_EIGEN	0
		/// #define FACE_REC_LBPH	1
		/// #define FACE_REC_FISHER	2
		/// 
		/// skip is the number of images in a set to ignore before pre-processing
		/// </summary>
		/// <param name='nameMemory'>
		/// Name memory.
		/// </param>
		/// <param name='algorithm'>
		/// Algorithm.
		/// </param>
		IMemory Train (IMemory nameMemory, int algorithm, int skip = 0);
		
		/// <summary>
		/// Tries to predict weither a given IplImage belongs to the model set
		/// defined by modelMemory (and thus, selecting the algorithm to use accordingly)
		/// using threashold (lower value => more restrictive prediction)
		/// Returns a prediction confidence value (lower means its more likely that the image
		/// was a part of the model) or -1.0 if the image did not belong.
		/// </summary>
		/// <param name='image'>
		/// If set to <c>true</c> image.
		/// </param>
		/// <param name='modelMemory'>
		/// If set to <c>true</c> model memory.
		/// </param>
		/// <param name='threshold'>
		/// If set to <c>true</c> threshold.
		/// </param>
		double Predict (IplImage image, IMemory modelMemory, double threshold = 70.0);
		void Test (IplImage image, int id);
		
		/// <summary>
		/// Sets the size of the face used by the pre-processor.
		/// </summary>
		/// <param name='size'>
		/// Size.
		/// </param>
		void SetFaceSize (uint size);
		
		/// <summary>
		/// If true, the bilateralFilter will be used during pre-processing
		/// </summary>
		/// <param name='useFilter'>
		/// Use filter.
		/// </param>
		void SetUseFilter (bool useFilter);
		
		/// <summary>
		/// If true, the preprocessor will try to determine a face elipse
		/// and if successfull, mask it out
		/// </summary>
		/// <param name='useElipse'>
		/// Use elipse.
		/// </param>
		void SetUseElipse (bool useElipse);
		
		/// <summary>
		/// Sets the equalization method used by the pre-processor. 
		/// #define FACE_REC_EQUALIZE_LEFT_RIGHT 2
		/// #define FACE_REC_EQUALIZE_NORMAL 	 1
		/// #define FACE_REC_EQUALIZE_NONE 		 0
		/// </summary>
		/// <param name='equalizationMethod'>
		/// Equalization method.
		/// </param>
		void SetEqualization(int equalizationMethod);
		
		/// <summary>
		/// Sets the minimum number of features that is required to be
		/// found in an image in order for qualifying it for a model. The
		/// availabel features are "left eye", "right eye" and "mouth". 
		/// The value n, must thus be -1 < n < 4.
		/// </summary>
		/// <param name='numberOfRequiredFeatures'>
		/// Number of required features.
		/// </param>
		void SetFeaturesRequirement (int numberOfRequiredFeatures);
		
		IplImage Equalize (IplImage image);
		
		/// <summary>
		/// Caches the all the models and stores them into memory. You have to run this method
		/// before predicting and once any model have changed.
		/// </summary>
		/// <param name='memorySource'>
		/// Memory source.
		/// </param>
		void LoadModels (IMemorySource memorySource);
		
	}
}


