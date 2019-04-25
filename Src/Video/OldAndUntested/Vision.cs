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
using R2Core.Device;
using R2Core;
using System.Threading;
using IplImage = System.IntPtr;

namespace R2Core.Video
{

	public class Vision
	{
		//public const int kCvRectSize = 16;
		
		private const string dllPath = "OpenCvModule.so";
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		//unsafe protected static extern void _ext_testish (void* cascade_pointer);
		protected static extern void _ext_testish (HaarCascade cascade);
		
		
		public Vision() {

		}

	}
}

