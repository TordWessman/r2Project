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
using R2Core.Device;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace R2Core.Common
{
	/// <summary>
	/// Represent a gstreamer pipeline object. Use this object to create a simple gstreamer pipline. 
	/// </summary>
	public class Gstream : DeviceBase, IGstream, ITaskMonitored {

		private const string dllPath = "libr2gstparseline.so";

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern IntPtr _ext_create_gstream(string pipeline);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_init_gstream(IntPtr ptr);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_destroy_gstream(IntPtr pipeline);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_play_gstream(IntPtr ptr);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_stop_gstream(IntPtr ptr);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_is_playing_gstream(IntPtr ptr);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_is_initialized_gstream(IntPtr ptr);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_get_error_code(System.IntPtr ptr);

		private string m_pipeLine;
		private Task m_task;

        private IntPtr m_ptr;

		readonly object m_startLock = new object();

        public bool IsRunning { get; private set; }

        public Gstream(string id, string pipeline) : base(id) {

			m_pipeLine = pipeline;

		}

		~Gstream() {
		
			if (m_ptr != IntPtr.Zero) {
			
				_ext_destroy_gstream(m_ptr);

			}

		}

		private void CreatePipeline() {
		
			m_ptr = _ext_create_gstream(m_pipeLine);

			if (m_ptr == IntPtr.Zero) {

				throw new ApplicationException("Unable to create pipeline: " + m_pipeLine);

			}


			if (_ext_init_gstream(m_ptr) != 1) {

				Log.e($"Gstream '{Identifier}' init failed with error code: {_ext_get_error_code(m_ptr)}. Unable to parse pipeline '{m_pipeLine}'.");

			}

		}

		public void SetPipeline(string pipeline) {

			if (!Ready) { 
			
				Stop();

			}

			if (m_ptr != IntPtr.Zero) {

				_ext_destroy_gstream(m_ptr);

			}

			m_pipeLine = pipeline;
			m_ptr = IntPtr.Zero;

		}


		public override void Start() {

			if (m_ptr == IntPtr.Zero) {
			
				CreatePipeline();

			}

			if (_ext_is_initialized_gstream(m_ptr) == 0) {
			
				throw new InvalidOperationException($"Unable to Start Gstream '{Identifier}'. Not initialized.");

			}

			m_task = Task.Factory.StartNew(() => {

				IsRunning = true;

				if (_ext_play_gstream(m_ptr) != 1) {

					IsRunning = false;

					Log.e($"Gstream '{Identifier}' start playback failed with error code: {_ext_get_error_code(m_ptr)}. Unable to play pipeline '{m_pipeLine}'.");

				}

				if (_ext_get_error_code(m_ptr) != 0) {

					Log.e($"Gstream '{Identifier}' playback failed with error code: {_ext_get_error_code(m_ptr)}. Unable to play pipeline '{m_pipeLine}'.");

				}

			});

		}

		public override void Stop() {

			IsRunning = false;

			if (Ready && _ext_stop_gstream(m_ptr) != 1) {

				Log.e("Gstream '" + Identifier + "' Unable to stop pipeline: " + m_pipeLine);

			}

		}

        /// <summary>
        /// Returns true if the the player has been initialied without errors and if it hasn't been started.
        /// </summary>
        /// <value><c>true</c> if ready; otherwise, <c>false</c>.</value>
        public override bool Ready {
		
			get {

				return m_ptr != IntPtr.Zero && _ext_is_playing_gstream(m_ptr) == 1;
			
			}
		
		}

		public bool Initialized {
			
			get {
			
				return _ext_is_initialized_gstream(m_ptr) == 1;

			}

		}

		#region ITaskMonitored implementation

		public System.Collections.Generic.IDictionary<string, Task> GetTasksToObserve() {

			return new System.Collections.Generic.Dictionary<string,Task> {{ "Gst" + Identifier, m_task}};
		
		}

		#endregion

	}

}