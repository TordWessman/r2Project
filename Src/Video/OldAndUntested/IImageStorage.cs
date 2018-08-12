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
using R2Core.Memory;
using System.Drawing;

namespace R2Core.Video
{
	public interface IImageStorage : IDevice
	{
		IMemory UpdateFaceRecognitionMemory (ref IMemory modelMemory, int algorithmType);
		IMemory CreateFaceRecognitionMemory (int algorithmType);
		IMemory Save (IplImage captureObject, string type, IMemory parent);
		//IMemory Save (CaptureObject captureObject, ImageTypes type);
		void SaveDump (IplImage image, string fileName);
		IMemory Save (IFrameSource source);
		void Delete (IMemory memory);
		void Delete (params int[] memoryIds);
		
		string GetImageFileName (IMemory imageMemory);
		string GetModelFileName (IMemory modelMemory);
		
		IplImage LoadImage (string fileName);
		IplImage LoadImage (IMemory memory);
		
		void ReleaseImage (IplImage image);
		IplImage Create32BitImage(IplImage image);
		Bitmap CreateBitmap (IplImage image);
		byte[] ImageToByte(Image img, string format_string = "png");
		//byte
		
	}
}

