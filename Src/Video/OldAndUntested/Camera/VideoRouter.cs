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
using R2Core.Video;

namespace R2Core.Video
{
	/// <summary>
	/// Represents a remote video source using the VideoServer native components(using a gstreamer pipe)
	/// </summary>
	public class VideoRouter : DeviceBase, IFrameSource {
		
		//private const string dllPath = "Test.so";
		
		private const string dllPath = "VideoServer.so";
		
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
		protected static extern void _ext_set_input_vars(string remote_address, int remote_port, string local_ip,
		     string width, string height);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_set_output_vars(string remote_address, string remote_port,
		    string width, string height);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_dealloc();
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_resume_from_eos();
		
		protected bool _isInitialized;
		
		protected Thread _serverThread;
		
		private ErrorCallBack m_errorCb;
		private EOSCallBack m_eosCb;
		
		private ICameraController m_camera;
		private static readonly object m_lock = new object();
		
		public VideoRouter(string id, string remoteIp, int remotePort, string localIp ) 
			: base(id) {
			_ext_set_input_vars(remoteIp, remotePort, localIp, "640", "480");
			_ext_set_output_vars(localIp, "6000", "320", "240");
			m_errorCb = new ErrorCallBack(this.DefaultErrorReceived);
			m_eosCb = new EOSCallBack(this.EOSReceived);
			
			
			if (_ext_init() == 1) {
				_isInitialized = true;
				
				_ext_set_callbacks(m_errorCb, m_eosCb);
			} else {
				throw new DeviceException("Unable to initialize video server module.");
			}

		}
		
		public void SetCamera(ICameraController camera) {
			m_camera = camera;
		}
		
		protected void EOSReceived() {
			new Thread(() => {
				while (m_camera == null || !m_camera.Ready) {
					Thread.Sleep(1000);
					Log.t("Waiting for camera...");
				}
				Thread.Sleep(2000);
				if (_ext_init() == 1) {
					Log.d("Re-initializing video router.");
					Thread.Sleep(2000);
					Start();
				} else {
					Log.e("Unable to reinitialize router.");
				}
				
				//_ext_resume_from_eos();	
			}
			).Start();
					
			//
		}

		protected void DefaultErrorReceived(int errorType, string errorMessage) {
			
			Log.e("VideoServer Error: " + errorMessage + " type: " + errorType.ToString());
			//Thread.Sleep(5000);
			//Start();
		}

		#region IDeviceBase implementation
		public override void Start() {
			
			if (_ext_is_running() == 1) {
				throw new DeviceException("Unable to start video server. Device is already running.");
			}
			
			_serverThread = new Thread(() => {
				
				_ext_start();
				Log.t("VideoRoterThread is no longer running");
			});
			_serverThread.Start();

		}

		public override void Stop() {
			
			if (_ext_is_running() == 0) {
				Log.e("Unable to stop video server. Device is not running.");
			} else {
				_ext_stop();
				_ext_dealloc();
			}
			
		}

		public override bool Ready {
			get {
				return _ext_is_running() == 1;
			}
		}
		#endregion

		public IplImage CurrentFrame
		{
			get {
				lock(m_lock) {
					return new IplImage( _ext_get_frame());
				}
			}
		}

		public CvSize Size { get {
				return new CvSize(_ext_get_video_width(), _ext_get_video_height());
			}
		}
	}
}

