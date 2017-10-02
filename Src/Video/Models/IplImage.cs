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

﻿using System;
using System.Runtime.InteropServices;

namespace Video
{
	/// <summary>
	/// Object representation of the raw opencv Ipl Image 
	/// </summary>
	public class IplImage
	{

		private const string dllPath = "libr2opencv.so";

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_create_dump(string filename, System.IntPtr image);


		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_release_ipl_image (System.IntPtr image);

		private System.IntPtr m_ptr;

		public System.IntPtr Ptr {get { return m_ptr; }}

		public IplImage (System.IntPtr ptr) {
			
			m_ptr = ptr;

		}

		public void Save(string filename) {
		
			_ext_create_dump (filename, m_ptr);

		}

		public void Destroy() {
		
			_ext_release_ipl_image (m_ptr);
			m_ptr = default(System.IntPtr);

		}

		~IplImage() {

			Destroy ();
		}

	}

}