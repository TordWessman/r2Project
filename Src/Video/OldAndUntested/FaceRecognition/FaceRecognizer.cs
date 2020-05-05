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
using R2Core.Device;
using System.Runtime.InteropServices;
using R2Core.Common;
using System.Linq;
using System.Collections.Generic;
using R2Core;

namespace R2Core.Video
{
	public class FaceRecognizer : DeviceBase, IFaceRecognizer
	{
		private const string dllPath = "OpenCvModule.so";
		
		public const int ALGORITHM_EIGEN = 0;
		public const int ALGORITHM_LBPH = 1;
		public const int ALGORITHM_FISHER = 2;
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern void _ext_init( HaarCascade left_eye, 
		                                      HaarCascade right_eye, 
		                                      HaarCascade mouth);
			
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern bool _ext_test(System.IntPtr image, int id);
		                                         
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern void _ext_train(string outputModelFileName, 
		                                      string [] fileNames, 
		                                      int []labels, 
		                                      int count,
		                                      int algorithm);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern double _ext_predict(System.IntPtr image, int modelId, double threshold);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern System.IntPtr _ext_prepare_face(System.IntPtr face_image, int equalizationType, int useElipse, int minimumFeaturesRequirement);
			
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern void _ext_load_model(int modelId, int algorithm, string input_file);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern System.IntPtr _ext_equalize(System.IntPtr image);
		                                         
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern void _ext_set_size(uint size);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern void _ext_set_equalization(int eq_type);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern void _ext_set_elipse(bool use_elipse);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern void _ext_set_use_filter(bool use_filter);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern void _ext_set_feature_requirement(int number_of_features);
		
		private IImageStorage m_storage;
		private static readonly object m_lock = new object();
		
		public FaceRecognizer(string id, 
		                       IImageStorage storage,
		                      HaarCascade leftEyeCascade,
		                      HaarCascade rightEyeCascade,
		                      HaarCascade mouthCascade) : base(id) {
			_ext_init(leftEyeCascade, rightEyeCascade, mouthCascade);
			m_storage = storage;
			//_ext_test2 (new string[] {"1305819942.jpg"}, new int[] {42}, 1);
		}
		
		public void Test(IplImage image, int id) {
			_ext_test(image.Ptr, id);
		}
		
		public IplImage Equalize(IplImage image) {
			return new IplImage(_ext_equalize(image.Ptr));
		}
		
		public IplImage PrepareFace(IplImage image, int equalizationType, int useElipse, int minimumFeaturesRequirement) {
			return new IplImage(_ext_prepare_face(image.Ptr, equalizationType, useElipse, minimumFeaturesRequirement));
			
		}
		
		public void LoadModels(IMemorySource memorySource) {
			ICollection<IMemory> models = memorySource.All("model");
			
			foreach (IMemory modelMemory in models) {
				string modelFileName = m_storage.GetModelFileName(modelMemory);
			
				if (!System.IO.File.Exists(modelFileName)) {
					Log.w("Prediction: MODEL FILE DOES NOT EXIST: " + modelFileName);
				} else {
					_ext_load_model(modelMemory.Id, int.Parse(modelMemory.Value), modelFileName);
				}
			}
		}
		
		public double Predict(IplImage image, IMemory modelMemory, double threshold = 70.0) {
			
			string modelFileName = m_storage.GetModelFileName(modelMemory);
			
			if (!System.IO.File.Exists(modelFileName)) {
				return -1.0;
			}
			
			lock(m_lock) {
				
				if (modelMemory.Type.ToLower () !=
					FaceRecognitionMemoryTypes.Model.ToString().ToLower ()) {
					throw new ArgumentException("Memory provided was of type: " + 
					                             modelMemory.Type + 
					                             " should be " +
						FaceRecognitionMemoryTypes.Model.ToString().ToLower ()
					);
				}
	
				return _ext_predict(image.Ptr, 
				                     modelMemory.Id, 
				                     threshold);
			}
		}
		
		public IMemory Train(IMemory nameMemory, int algorithm, int skip = 0) {
			//FIXME: naming conventions should be strings. Stored in settings!
			string nameType = "name";

			lock(m_lock) {
				if (nameMemory.Type != "name") {
					throw new ArgumentException("Memory provided was of type: " + nameMemory.Type + " should be " + nameType);

				}
				    
				ICollection<IMemory> associations = nameMemory.Associations;
				
				IMemory modelMemory = (from m in associations where 
					m.Type == FaceRecognitionMemoryTypes.Model.ToString().ToLower ()
				                       select m).FirstOrDefault();
				
				if (modelMemory != null && 
					modelMemory.Value != algorithm.ToString()) {
					
					modelMemory = m_storage.UpdateFaceRecognitionMemory(ref modelMemory, algorithm);
					modelMemory.Associate(nameMemory);
					
				} else if (modelMemory == null) {
					
					modelMemory = m_storage.CreateFaceRecognitionMemory(algorithm);
					
					modelMemory.Associate(nameMemory);
				}
	
				IList<int> ids = new List<int>();
				IList<string> fileNames = new List<string>();
				
				int count = 0;
				
				foreach (IMemory mem in (from m in associations where 
					m.Type == ImageTypes.Face.ToString().ToLower ()
				                       select m)) {
					
					
					string fileName = m_storage.GetImageFileName(mem);
					
					if (!System.IO.File.Exists(fileName)) {
						Log.w("Facetrain: FILE DOES NOT EXIST: " + fileName);
					} else {
						
						
						count++;
						if (count > skip) {
							//Log.t("HAHAH: " + skip + " " + count);
							fileNames.Add(fileName);
							ids.Add(mem.Id);
						} else {
							Log.d("Skipping image: " + fileName);
						}
						
						

					}
				}
				
				string modelFileName = m_storage.GetModelFileName(modelMemory);
				
				if (System.IO.File.Exists(modelFileName)) {
					System.IO.File.Delete(modelFileName);
				}
				
				if (ids.Count > 0) {
					_ext_train(modelFileName, 
				            fileNames.ToArray(), 
				            ids.ToArray(), 
				            fileNames.Count,
				            algorithm);
				} else {
					Log.w("Unable to train memory named: " + nameMemory.Value + " since it has no face images");
				}
				
				
				return modelMemory;
			}
		}

		
		public void SetFaceSize(uint size) {
			_ext_set_size(size);
		}

		public void SetUseFilter(bool useFilter) {
			_ext_set_use_filter (useFilter);
		}

		public void SetUseElipse(bool useElipse) {
			_ext_set_elipse(useElipse);
		}

		public void SetEqualization(int equalizationMethod) {
			_ext_set_equalization(equalizationMethod);
		}
		
		public void SetFeaturesRequirement(int numberOfRequiredFeatures) {
			_ext_set_feature_requirement(numberOfRequiredFeatures);
		}
		
	}
}

