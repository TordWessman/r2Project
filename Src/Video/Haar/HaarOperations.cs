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
using System.Runtime.InteropServices;
using R2Core;
using System.Collections.Generic;
using R2Core.Device;

namespace R2Core.Video
{
	public class HaarOperations : DeviceBase
	{
		private const string dllPath = "libr2opencv.so";

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_get_image_width (System.IntPtr image);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_get_image_height (System.IntPtr image);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern HaarCascade _ext_create_haar_cascade(string filename, int tag);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern bool _ext_haar_capture (
			System.IntPtr image, 
			ref CaptureObjectsContainer array);

		//		protected static extern CaptureObjectArray _ext_haar_capture (
		//			System.IntPtr image, 
		//			HaarCascade cascade,
		//			bool saveImage, 
		//			CvRect roi);


		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_release_haar_cascade (ref HaarCascade cascade);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_release_capture_object_array(ref CaptureObjectsContainer array);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_release_ipl_image(System.IntPtr image);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern CaptureObject _ext_get_capture_object(ref CaptureObjectsContainer array, int index);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_hej(ref CaptureObjectsContainer array);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern System.IntPtr _ext_img_ptr (ref CaptureObject obj);

		private static readonly object m_lock = new object();
		
		private IDictionary<int,HaarCascade> m_haarCascades;
		private ICollection<IplImage> m_capturedImages;
		private string m_haarPath;
	
		/// <summary>
		/// Initializes a new instance of the <see cref="Video.HaarOperations"/> class. 'basePath' is the default path where the haar cascade files are located. 
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="basePath">Base path.</param>
		public HaarOperations (string id, string basePath = null) : base (id)
		{
			
			m_haarPath = basePath;	
			m_haarCascades = new Dictionary<int, HaarCascade> ();
			m_capturedImages = new List<IplImage> ();
		
		}
		
		~HaarOperations () {
		
			Log.w ("HAAR OPERATION WILL NOW REMOVE ALL IMAGES!");
			
			foreach (HaarCascade cascade in m_haarCascades.Values) {
				HaarCascade clone = cascade; 
				//_ext_release_haar_cascade (ref clone);
			}
			
			foreach (IplImage image in m_capturedImages) {
				//_ext_release_ipl_image (image);
				//CaptureObjectArray clone = array;
				//ReleaseCaptureObeject (clone);
			}

		}

		public HaarCascade CreateHaarCascade (string fileName) {
			
			fileName = m_haarPath == null ? fileName : System.IO.File.Exists (fileName) ? fileName : m_haarPath + System.IO.Path.DirectorySeparatorChar + fileName;
			
			if (!System.IO.File.Exists (fileName)) { throw new System.IO.FileNotFoundException ($"Haar file not found: '{fileName}'."); }
			
			int tag = new Random ().Next (0, Int32.MaxValue);
			HaarCascade cascade = _ext_create_haar_cascade (fileName, tag);
			m_haarCascades.Add (tag, cascade);
			return cascade;

		}
		
		public IplImage GetImage (CaptureObject obj) {
			
			return new IplImage(_ext_img_ptr (ref obj));
		
		}
		
		public void ReleaseCaptureObeject (CaptureObjectsContainer objectArray) {
			
			_ext_release_capture_object_array (ref objectArray);

		}

		public CaptureObjectsContainer CreateCapture (HaarCascade haar, bool saveImage = true) {
			
			return CreateCapture (haar, saveImage, new CvRect{X = 0, Y = 0, Width = 0, Height = 0});
		
		}
		
		public CaptureObjectsContainer CreateCapture (HaarCascade haar, bool saveImage, CvRect roi) {

			return new CaptureObjectsContainer () {size = 0, saveImage = saveImage, roi = roi, cascade = haar};
		
		}
		
		public CaptureObjectsContainer HaarCapture (IplImage image, ref CaptureObjectsContainer array)
		{
			
			lock (m_lock) {
				
				int width = _ext_get_image_width (image.Ptr);
				int height = _ext_get_image_height (image.Ptr);
				
				if (array.roi.Width + array.roi.X > width) {
					
					Log.e ("Bad ROI: roi.width:" + array.roi.Width + " roi.x: " + array.roi.X + " image width: " + width);
					
					array.size = 0;
					return array;
					
				} else if (array.roi.Height + array.roi.Y > height) {
					
					Log.e ("Bad ROI: roi.heigtn:" + array.roi.Height + " roi.y: " + array.roi.Y + " image height: " + height);
					
					array.size = 0;
					return array;

				}
				
				_ext_haar_capture (image.Ptr, ref array);

				//for (int i = 0; i < array.size; i++) {
				//	CaptureObject obj = GetCaptureObject (array, i);
//					Console.WriteLine ("Image Pointer: {0:X}", obj.captured_image);
				//}

				return array;

				//return array;
//				if (saveImage) {
//					for (int i = 0; i < array.size; i++) {
				//						IplImage capturedImage = new IplImage (_ext_get_capture_object (array, i).captured_image);
//						Log.t ("Saved image pointer: " + capturedImage.ToInt64);
//						m_capturedImages.Add (capturedImage);
//					}
//					
//			
//				return array;
			}
		}
		
		public CaptureObjectsContainer FrameCapture (IFrameSource source, ref CaptureObjectsContainer array) {
			//Not tested...

			HaarCapture (source.CurrentFrame, ref array);

			return array;
			
		}
		
		public CaptureObject GetCaptureObject (CaptureObjectsContainer array, int index)
		{
			CaptureObject obj = _ext_get_capture_object (ref array, index);
			
			return obj;
		}
		
		
		
	}
}

