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
using R2Core.DataManagement.Memory;
using R2Core;
using MemoryType = System.String;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using R2Core.Device;
using System.IO;
using System.Drawing;
using System.Threading;

namespace R2Core.Video
{
	public class ImageStorage : DeviceBase, IImagePointerManager, IImageStorage
	{

		private const string dllPath = "OpenCvModule.so";
		public const MemoryType IMAGE_MEMORY_TYPE = "image";
		
		private const string IMAGE_EXTENSION = ".jpg";
		
		private const string MODEL_EXTENSION = "faceModel.yaml";
		
		private string m_basePath;
		
		private IMemorySource m_memory;
	
		private ICollection<IplImage> m_loadedImages;
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_create_dump(string filename,  System.IntPtr image);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern System.IntPtr _ext_load_image(string filename);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_release_ipl_image(System.IntPtr image);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_get_image_width (System.IntPtr image);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_get_image_height(System.IntPtr image);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern System.IntPtr _ext_create_32_bit_image(System.IntPtr image);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern System.IntPtr _ext_get_bitmap(System.IntPtr image);
		
		public override bool Ready { get { return m_memory != null && m_memory.Ready;}}
		
		public ImageStorage(string id, IMemorySource memory, string basePath) : base(id) {
			m_basePath = basePath;
			m_memory = memory;
			m_loadedImages = new List<IplImage>();
		}
		
		~ImageStorage() {
			foreach (IplImage image in m_loadedImages) {
				_ext_release_ipl_image(image.Ptr);
			}
		}
		
		public void Delete(IMemory memory) {
			string fileName = null;
			if (memory.Type == 
				FaceRecognitionMemoryTypes.Model.ToString().ToLower ()) {
				fileName = GetModelFileName(memory);
			} else if (memory.Type == ImageTypes.Face.ToString().ToLower ()) {
				fileName = GetImageFileName(memory);
			} else {
				throw new ArgumentException("Invalid memory for image deletion. Type: " + memory.Type.ToString() +
					"id: " + memory.Id
				);
				                         
			}
			
			Log.t("TAR BORT: " + fileName + " type: "  + memory.Type );
			System.IO.File.Delete(fileName);
			m_memory.Delete(memory.Id);
		}
		
		
		public void Delete(params int[] memoryIds) {
			
			foreach (int memoryId in memoryIds) {

				IMemory memory = m_memory.Get(memoryId);
			
				if (memory == null) {
					Log.w("Memory id provided did not exist: " + memoryId);
				} else {
					Delete(memory);
				}
			
				
			}
			
		}
		
		public IMemory Save(IplImage image, string type, IMemory parent) {

			IMemory memory = m_memory.Create(type.ToLower (), parent.Value);
			
			_ext_create_dump(GetImageFileName(memory), image.Ptr);
			
			return memory;
		}
//		public IMemory Save(CaptureObject captureObject, ImageTypes type)
//		{
//
//			IMemory memory = m_memory.Create(type.ToString().ToLower (), IMAGE_MEMORY_TYPE);
//			
//			_ext_create_dump(GetImageFileName(memory), captureObject.captured_image);
//			
//			return memory;
//		}
		

		public IMemory Save(IFrameSource source) {
			IMemory memory = m_memory.Create(ImageTypes.Frame.ToString().ToLower (), IMAGE_MEMORY_TYPE);	
			
			_ext_create_dump(GetImageFileName(memory), source.CurrentFrame.Ptr);

			return memory;
		}
		
		public void SaveDump(IplImage image, string fileName) {
			_ext_create_dump(fileName, image.Ptr);
		}
		

		#region IImagePointerManager implementation
		public IplImage LoadImage(IMemory memory) {
			
			IplImage image = LoadImage(GetImageFileName(memory));
			
			return image;
		}
		
		public void ReleaseImage(IplImage image) {
			_ext_release_ipl_image(image.Ptr);
		}
		
		#endregion
		public IplImage LoadImage(string fileName) {
			if (!System.IO.File.Exists(fileName)) {
				throw new System.IO.FileNotFoundException("Unable to load file: " + fileName + ". Does it exist?");
			}
			
			IplImage image = new IplImage( _ext_load_image(fileName));
			m_loadedImages.Add(image);
			
			return image;
		}
		
		public IMemory UpdateFaceRecognitionMemory(ref IMemory modelMemory, int algorithmType) {
			m_memory.Delete(modelMemory.Id);

			modelMemory = null;

			return CreateFaceRecognitionMemory(algorithmType);
		}
		
		public IMemory CreateFaceRecognitionMemory(int algorithmType) {
			return m_memory.Create(
					FaceRecognitionMemoryTypes.Model.ToString().ToLower (),
					algorithmType.ToString());
		}
		
		
		private string GetBasePath (IMemory memory) {
			string path = m_basePath + Path.DirectorySeparatorChar +
				memory.Type;
			
			if (!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}
			
			return path += Path.DirectorySeparatorChar;
		}

		
		public string GetImageFileName(IMemory memory) {
			string path = GetBasePath (memory) + memory.Value;
			
			if (!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}

			return path + Path.DirectorySeparatorChar +
				memory.Id + IMAGE_EXTENSION;
		}
		
		public string GetModelFileName(IMemory memory) {
			if (memory.Type != "model") {
				throw new ArgumentException("Memory provided is not a model");
			}
			
			IMemory nameMemory = memory.GetAssociation("name");
			
			return GetBasePath (memory) + nameMemory.Value + MODEL_EXTENSION;
			

		}
		
		public Bitmap CreateBitmap(IplImage image) {
			int width = _ext_get_image_width (image.Ptr);
			int height = _ext_get_image_height(image.Ptr);
			
			return new Bitmap(
					
				    width,
				                       height,
				                      4 * width,
				                       System.Drawing.Imaging.PixelFormat.Format24bppRgb,
				_ext_get_bitmap(image.Ptr));
		}

		public IplImage Create32BitImage(IplImage image) {
			return new IplImage(_ext_create_32_bit_image(image.Ptr));
		}
		
		
		public byte[] ImageToByte(Image img, string format_string = "png") {
			
			//ImageConverter converter = new ImageConverter();
			//return(byte[])converter.ConvertTo(img, typeof(byte[]));
			
			System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;
			
			if (format_string == "jpeg") {
				format = System.Drawing.Imaging.ImageFormat.Jpeg;
			} else if (format_string == "bmp") {
				format = System.Drawing.Imaging.ImageFormat.Bmp;
			} else if (format_string == "gif") {
				format = System.Drawing.Imaging.ImageFormat.Gif;
			} else if (format_string == "mbmp") {
				format = System.Drawing.Imaging.ImageFormat.MemoryBmp;
			}
			
			//img.Save("hej." + format_string, format);
			
			byte[] byteArray = new byte[0];
			using(MemoryStream stream = new MemoryStream()) {
				img.Save(stream, format);
				stream.Close();

				byteArray = stream.ToArray();
			}
			//Log.d("Length: " + byteArray.Length);
			return byteArray;
		}
	}
}

