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

namespace R2Core.Video
{


	
	[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit), Serializable]
	public struct CaptureObject {
		[System.Runtime.InteropServices.FieldOffset(0)]
		public CvRect capture_bounds;				//the latest capture for this object
		[System.Runtime.InteropServices.FieldOffset(16)]
		public bool did_capture;					//did the latest try to capture result in a capture?
		[System.Runtime.InteropServices.FieldOffset(16 + 4) ]
		public bool initialized;					//is this CaptureObject loaded?
		[System.Runtime.InteropServices.FieldOffset(16 + 8) ]
		public System.IntPtr captured_image;				//capturedImage (if any) stored in RGBA format

	}
}

