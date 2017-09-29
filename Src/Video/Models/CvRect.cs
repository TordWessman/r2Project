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

namespace Video
{

	[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit, Size=16), Serializable]
	public struct CvRect {
		
		[System.Runtime.InteropServices.FieldOffset(0)]
		public int x;
		[System.Runtime.InteropServices.FieldOffset(4)]
		public int y;
		[System.Runtime.InteropServices.FieldOffset(8)]
		public int width;
		[System.Runtime.InteropServices.FieldOffset(12)]
		public int height;
		
		public CvPoint Center { get { return new CvPoint(x + width / 2, y + height / 2); }}
		
		public CvRect (string[]vals) {
			
			this = new CvRect () {x = int.Parse (vals [1]), y = int.Parse (vals [2]), width = int.Parse (vals [3]), height =  int.Parse (vals [4])};

		}

		public CvRect (int x, int y, int width, int height) {
			
			if (x < 0 || y < 0) { throw new ArgumentException ($"x and y must be > 0 x: {x} y: {y}"); }
			
			if (width < 0 || height < 0) { throw new ArgumentException ($"width and height must be > 0 width: {width} height: {height}"); }

			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;

		}
		
		public CvSize Size { get { return new CvSize (width, height); } }
		
		public CvPoint Position { get { return new CvPoint (x,y); } }
		
		public bool IsEmpty { get { return !(x == 0 && y == 0 && width == 0 && height == 0); } }

		public void BindTo (int maxWidth, int maxHeight) {

			x = x < 0 ? x : x > maxWidth ? x = maxWidth : x;
			y = y < 0 ? y : y > maxHeight ? y = maxHeight : y;
			width = x + width > maxWidth ? maxWidth + x : width;
			height = y + height > maxHeight ? maxHeight - y : height;

		}

	}

}