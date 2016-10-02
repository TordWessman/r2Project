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
using Core.Shared;

namespace Video
{
	
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto), Serializable]
	//[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit), Serializable]
	public struct CaptureObjectsContainer {
		//[System.Runtime.InteropServices.FieldOffset(0)]
		public int size;
		//[System.Runtime.InteropServices.FieldOffset(4)]
		public bool saveImage; 
		//[System.Runtime.InteropServices.FieldOffset(8)]
		public CvRect roi;
		//[System.Runtime.InteropServices.FieldOffset(8 + 16)]
		public HaarCascade cascade;
		//[MarshalAs(UnmanagedType.SafeArray)] 
		//public CaptureObject[] objects;
		
		//unsafe public CaptureObject* data;
		public IntPtr objects;

	}
}

