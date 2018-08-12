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
	
	[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit, Size=8), Serializable]
	public struct CvSize
	{
		
		public CvSize (int width, int height) {

			if (width < 0 || height < 0) { throw new ArgumentException ($"width and height must be > 0 width: {width} height: {height}"); }
			
			this.Width = width;
			this.Height = height;
		}
		
		[System.Runtime.InteropServices.FieldOffset(0)]
		public int Width;
		[System.Runtime.InteropServices.FieldOffset(4)]
		public int Height;

	}

}