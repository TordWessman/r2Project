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
//
using System;
using Core.Device;
using System.Runtime.InteropServices;

namespace Video
{
	public class CvCamera : DeviceBase
	{
		private const string dllPath = "libr2opencv.so";

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern System.IntPtr _ext_capture_camera(int device);

		int m_cameraId;

		public CvCamera (string id, int cameraId = 0) : base (id) {

			m_cameraId = cameraId;

		}

		/// <summary>
		/// Captures a frame using the web cam
		/// </summary>
		/// <value>The current frame.</value>
		public IplImage CurrentFrame { get { return new IplImage (_ext_capture_camera (m_cameraId)); } }

	}

}

