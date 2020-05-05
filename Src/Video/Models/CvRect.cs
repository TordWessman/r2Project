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

	[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit, Size=16), Serializable]
	public struct CvRect {
		
		[System.Runtime.InteropServices.FieldOffset(0)]
		public int X;
		[System.Runtime.InteropServices.FieldOffset(4)]
		public int Y;
		[System.Runtime.InteropServices.FieldOffset(8)]
		public int Width;
		[System.Runtime.InteropServices.FieldOffset(12)]
		public int Height;
		
		public CvPoint Center { get { return new CvPoint(X + Width / 2, Y + Height / 2); }}
		
		public CvRect(string[]vals) {
			
			this = new CvRect() {X = int.Parse(vals[1]), Y = int.Parse(vals[2]), Width = int.Parse(vals[3]), Height =  int.Parse(vals[4])};

		}

		public CvRect(int x, int y, int width, int height) {
			
			if (x < 0 || y < 0) { throw new ArgumentException($"x and y must be > 0 x: {x} y: {y}"); }
			
			if (width < 0 || height < 0) { throw new ArgumentException($"width and height must be > 0 width: {width} height: {height}"); }

			this.X = x;
			this.Y = y;
			this.Width = width;
			this.Height = height;

		}
		
		public CvSize Size { get { return new CvSize(Width, Height); } }
		
		public CvPoint Position { get { return new CvPoint(X, Y); } }
		
		public bool IsEmpty { get { return !(X == 0 && Y == 0 && Width == 0 && Height == 0); } }

		public void BindTo(int maxWidth, int maxHeight) {

			X = X < 0 ? X : X > maxWidth ? X = maxWidth : X;
			Y = Y < 0 ? Y : Y > maxHeight ? Y = maxHeight : Y;
			Width = X + Width > maxWidth ? maxWidth + X : Width;
			Height = Y + Height > maxHeight ? maxHeight - Y : Height;

		}

	}

}