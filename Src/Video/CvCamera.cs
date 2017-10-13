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
	public class CvCamera : DeviceBase, IFrameSource
	{
		private const string dllPath = "libr2opencv.so";

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern System.IntPtr _ext_capture_camera(int deviceId);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern bool _ext_start_capture(int deviceId, CvSize size, int skipFrames);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern CvSize _ext_get_video_size(int deviceId);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_stop_capture(int deviceId);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern bool _ext_is_running_capture (int deviceId);

		/// <summary>
		/// It seems like v4l2 uses a buffer set and thus returning the last image in this buffer. (see cap_v4l.cpp, DEFAULT_V4L_BUFFERS). 
		/// </summary>
		public const int SkipFrames = 5;

		private int m_cameraId;
		private CvSize m_size;
		private int m_skipFrames;

		/// <summary>
		/// Creates a camera source with lazy resource loading. the skipFrames parameter is used to drop (grab) frames from the v4l buffer in order to get the latest frame. Set to zero if it's not working (or if you like delays).  
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="cameraId">Camera identifier.</param>
		/// <param name="skipFrames">Skip frames.</param>
		public CvCamera (string id, int width, int height, int cameraId, int skipFrames = SkipFrames) : base (id) {

			m_cameraId = cameraId;
			m_size = new CvSize () { Width = width, Height = height };
			m_skipFrames = skipFrames;

		}

		public CvSize Size{ get { return _ext_get_video_size (m_cameraId); } }

		/// <summary>
		/// Captures a frame using the web cam
		/// </summary>
		/// <value>The current frame.</value>
		public IplImage CurrentFrame { get { return new IplImage (_ext_capture_camera (m_cameraId)); } }

		public override void Stop () {
		
			_ext_stop_capture (m_cameraId);

		}

		public override bool Ready { get { return _ext_is_running_capture (m_cameraId); } }

		public override void Start () {

			if (!_ext_start_capture (m_cameraId, m_size, m_skipFrames)) {
			
				throw new ApplicationException ($"Unable to start camera for camera id: '{m_cameraId}'.");

			}

		}

	}

}

