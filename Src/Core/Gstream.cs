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
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;

namespace Core
{
	/// <summary>
	/// Represent a gstreamer pipeline object. Use this object to create a simple gstreamer pipline. 
	/// </summary>
	public class Gstream : RemotlyAccessableDeviceBase, IGstream, ITaskMonitored, IJSONAccessible
	{
		private static readonly int START_LOCK_TIMEOUT_MS = 5000;

		private const string dllPath = "libr2gstparseline.so";

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern System.IntPtr _ext_create_gstream(string pipeline);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_init_gstream(System.IntPtr ptr);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_destroy_gstream(System.IntPtr pipeline);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_play_gstream(System.IntPtr ptr);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_stop_gstream(System.IntPtr ptr);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_is_playing_gstream(System.IntPtr ptr);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_is_initialized_gstream (System.IntPtr ptr);

		private string m_pipeLine;
		private Task m_task;
		private bool m_isRunning;

		private System.IntPtr m_ptr;

		readonly object m_startLock = new object();  

		public Gstream (string id, string pipeline) : base (id)
		{

			m_pipeLine = pipeline;

			m_ptr = _ext_create_gstream (pipeline);


			if (m_ptr == System.IntPtr.Zero) {

				throw new OutOfMemoryException ("Unable to create pipeline: " + pipeline);

			}


			if (_ext_init_gstream (m_ptr) != 1) {

				Log.e ("Gstream '" + Identifier + "' Unable to parse pipeline: " + pipeline);
			
			}
		
		}

		~Gstream() {

			_ext_destroy_gstream (m_ptr);

		}

		public override void Start ()
		{

			if (_ext_is_initialized_gstream (m_ptr) == 0) {
			
				throw new InvalidOperationException ("Unable to Start Gstream '" + Identifier + "'. Initialization failed.");

			}

			if (Monitor.IsEntered (m_startLock)) {

				throw new InvalidOperationException ("Unable to Start Gstream '" + Identifier + "' since it's about to start.");

			}

			Monitor.Enter (m_startLock);

			m_task = Task.Factory.StartNew (() => {

				m_isRunning = true;

				Monitor.Exit(m_startLock);

				if (_ext_play_gstream(m_ptr) != 1) {

					m_isRunning = false;

					Log.e ("Gstream '" + Identifier + "' Unable to play pipeline: " + m_pipeLine);

				}

			});
				
			Monitor.Wait (m_startLock, START_LOCK_TIMEOUT_MS);

		}

		public override void Stop ()
		{
			m_isRunning = false;

			if (Monitor.IsEntered (m_startLock)) {
			
				Monitor.Exit(m_startLock);

			}

			if (!Ready && _ext_stop_gstream(m_ptr) != 1) {

				Log.e ("Gstream '" + Identifier + "' Unable to stop pipeline: " + m_pipeLine);

			}

		}

		public bool IsRunning {

			get {

				return m_isRunning;

			}

		}

		/// <summary>
		/// Returns true if the the player has been initialied without errors and if it hasn't been started.
		/// </summary>
		/// <value><c>true</c> if ready; otherwise, <c>false</c>.</value>
		public override bool Ready {
		
			get {

				return _ext_is_playing_gstream(m_ptr) != 1;
			
			}
		
		}

		#region ITaskMonitored implementation

		public System.Collections.Generic.IDictionary<string, Task> GetTasksToObserve ()
		{

			return new System.Collections.Generic.Dictionary<string,Task>() {{ "Gst" + Identifier, m_task}};
		
		}

		#endregion

		#region IRemotlyAccessable implementation

		public override byte[] RemoteRequest (string methodName, byte[] rawData, IRPCManager<System.Net.IPEndPoint> mgr)
		{
			if (IsBaseMethod (methodName)) {

				return ExecuteStandardDeviceMethod (methodName, rawData, mgr);

			} else if (methodName == "is_running") {
			
				return mgr.RPCReply<bool> (Guid, methodName, IsRunning);
			}

			throw new NotImplementedException ("Gstream '" + Identifier + "' : Unable to interpret IRemotlyAccessable.RemoteRequest: " + methodName);
		}

		public override RemoteDevices GetTypeId ()
		{

			return RemoteDevices.Gstream;
		
		}

		#endregion

		#region IJSONAccessible implementation

		public string Interpret (string functionName, string parameters = null)
		{

			if (functionName == "start") {

				Start ();

				return IsRunning.ToString();
		
			} else 	if (functionName == "stop") {
			
				Stop ();
				return Ready.ToString();
			
			} else if (functionName == "ready") {
			
				return Ready.ToString();
			
			}

			throw new NotImplementedException ("Gstream '" + Identifier + "': Unable to interpret IJSONAccessible.Interpret: " + functionName);
		
		}

		#endregion
	}

}