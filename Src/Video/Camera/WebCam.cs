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
using Core.Device;
using Video;
using System.Threading;
using Core.Shared;
using System.Runtime.InteropServices;
using Core;
using System.Threading.Tasks;

namespace Video.Camera
{
	public class WebCam: DeviceBase, IFrameSource
	{
		//private const string dllPath = "Test.so";

		private const string dllPath = "WebCam.so";

		protected delegate void ErrorCallBack(int errorType, string message);
		protected delegate void EOSCallBack();

		//[DllImport(dllPath, CharSet = CharSet.Auto)]
		//protected static extern int test();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_start();	

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_init();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_stop();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_get_video_width();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_get_video_height();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_is_running();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_set_callbacks(ErrorCallBack errorCb, EOSCallBack EOScb);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern System.IntPtr _ext_get_frame();	

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_set_input_vars (string width, string height);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_pause_frame_fetching();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_resume_frame_fetching();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_dealloc();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_resume_from_eos();

		protected bool _isInitialized;

		protected Task _serverTask;

		private ErrorCallBack m_errorCb;
		private EOSCallBack m_eosCb;

		private ICameraController m_camera;
		private static readonly object m_lock = new object();

		public WebCam (string id, int width, int height) 
			: base (id)
		{
			_ext_set_input_vars (width.ToString(), height.ToString());
			m_errorCb = new ErrorCallBack (this.DefaultErrorReceived);
			m_eosCb = new EOSCallBack (this.EOSReceived);


			if (_ext_init () == 1) {
				_isInitialized = true;

				_ext_set_callbacks (m_errorCb, m_eosCb);
			} else {
				throw new DeviceException("Unable to initialize video server module.");
			}

		}

		public int Width {
			get {
				return _ext_get_video_width ();
			}
		}

		public int Height {
			get {
				return _ext_get_video_height ();
			}
		}

		public void SetCamera (ICameraController camera)
		{
			m_camera = camera;
		}

		protected void EOSReceived ()
		{
			Task.Factory.StartNew (() => {
				while (m_camera == null || !m_camera.Ready) {
					Thread.Sleep (1000);
					Log.t ("Waiting for camera...");
				}
				Thread.Sleep (2000);
				if (_ext_init () == 1) {
					Log.d ("Re-initializing video router.");
					Thread.Sleep (2000);
					Start ();
				} else {
					Log.e ("Unable to reinitialize router.");
				}

				//_ext_resume_from_eos ();	
			}
			).Start();

			//
		}

		protected void DefaultErrorReceived (int errorType, string errorMessage)
		{
			Log.e ("VideoServer Error: " + errorMessage + " type: " + errorType.ToString ());

		}

		#region IDeviceBase implementation
		public override void Start ()
		{
			if (_ext_is_running () == 1) {
				throw new DeviceException ("Unable to start video server. Device is already running.");
			}

			_serverTask = Task.Factory.StartNew (() => {

				_ext_start ();
				Log.t ("...video thread is no longer running.");
			});
		
		}

		public override void Stop ()
		{
			if (_ext_is_running () == 0) {
				Log.e ("Unable to stop video server. Device is not running.");
			} else {
				Log.d ("Terminating video thread...");
				_ext_stop ();
				_ext_dealloc ();

			}

		}

		public override bool Ready {
			get {
				return _ext_is_running () == 1;
			}
		}
		#endregion

		public IplImage CurrentFrame
		{
			get {
				lock (m_lock) {
					return new IplImage( _ext_get_frame ());
				}
			}
		}

		public void PauseFrameFetching ()
		{
			_ext_pause_frame_fetching ();
		}

		public void ResumeFrameFetching ()
		{
			_ext_resume_frame_fetching ();
		}

		public CvSize Size{ get {
				return new CvSize (Width, Height);
			}
		}
	}
}

