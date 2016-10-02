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
using System.Net;
using Core;
using System.Threading.Tasks;
using System.Collections.Generic;
using Core.Device;


namespace ASR
{
	public class SphinxASRServer : DeviceBase, IASR
	{
		
		protected delegate void TextInterpretedCallBack(string message);
		protected delegate void SphinxErrorCallBack(int errorType, string message);
		
		private const string dllPath = "sphinx_stream.so";
	
		[DllImport(dllPath, CharSet = CharSet.Auto)]
   	 	public static extern int asr_start();

		[DllImport( dllPath)]
   	 	protected static extern int asr_turn_off();
		
		[DllImport(dllPath)]
   	 	protected static extern void set_is_active(bool reportMode);
		
		[DllImport(dllPath)]
   	 	protected static extern bool get_is_active();

		[DllImport(dllPath)]
   	 	protected static extern bool get_is_running();
		
		[DllImport(dllPath)]
   	 	protected static extern int asr_init(
		   	TextInterpretedCallBack msgFunc,
			SphinxErrorCallBack errorFunc,
		    string lmFile,
		    string dictFile,
		    string hmmFile,
			int port,
			string hostIp);
		
		
		protected bool m_isRunning;
		protected Task m_asrTask;

		//observers are called from a gstreamer thread...
		private static ICollection<IASRObserver> m_observers;
		
		public const int AUDIO_SERVER_PORT = 5005;
				
		private TextInterpretedCallBack m_textReceivedCallBack;
		private SphinxErrorCallBack m_errorReceivedCallBack;
		
		public SphinxASRServer (string id, string lm, string dic, string hmmDir, string hostIp, int port = AUDIO_SERVER_PORT)
			: base (id)
		{
			

			m_observers = new List<IASRObserver> ();
			
			m_textReceivedCallBack = new TextInterpretedCallBack (this.DefaultTextReceived);
			m_errorReceivedCallBack = new SphinxErrorCallBack (this.DefaultErrorReceived);
			
			m_asrTask = new Task (() => {
				
				while (m_isRunning) {
					if (asr_start () != 0) {
						throw new ExternalException ("Unable to start ASR!");
					}
				}
			}
			);
			
			
			
			if (asr_init (
				m_textReceivedCallBack, 
			    m_errorReceivedCallBack,
				lm, dic, hmmDir,
				port,
				hostIp
			) != 0)
			
				throw new ExternalException ("Unable to initialize ASR engine");
		}
		
		public bool IsActive 
		{
			get 
			{
				return get_is_active();
			}
			set
			{
				set_is_active(value);
			}
		}
		
		public override void Start ()
		{

			if (m_isRunning)
				throw new DeviceException ("ASR is already running...");
			
			m_isRunning = true;
			m_asrTask.Start ();
		}
		
		public override void Stop ()
		{
			m_isRunning = false;
			asr_turn_off ();
		}
		
		
		protected void DefaultTextReceived (string text)
		{
			if (text == null)
				Log.t ("TEXT WAS NULL");
			else
			Console.WriteLine ("ASR: " + text);
			/*
			foreach (IASRObserver observer in m_observers) {
				IASRObserver clone = observer;
				Task.Factory.StartNew (() => {
					try {
						observer.TextReceived (text);
					} catch (Exception ex) {
						Log.x (ex);
					}
						
				}
				);
				
			}*/
			
			Parallel.ForEach (m_observers, observer => {
				try {
					observer.TextReceived (text);
				} catch (Exception ex) {
					Log.x (ex);
				}
			});
		}
		
		protected void DefaultErrorReceived (int errorType, string errorMessage)
		{
			Log.e("ASR ERROR: " + errorMessage + " type: " + errorType.ToString());
			
		}
		
		#region ITaskMonitored implementation
		public IDictionary<string,Task> GetTasksToObserve ()
		{
			return new Dictionary<string, Task>() {{"SPHINX ASR",m_asrTask}};
		}
		#endregion

		#region IDeviceBase implementation

		public override bool Ready {
			get {
				return m_isRunning && get_is_running ();
			}
		}
		#endregion



		#region IASR implementation
		private void ReloadLanguageModels ()
		{
			Stop ();
			while (get_is_running()) {
				Console.Write ("x");
			}
			
			Start ();
			
		}
		#endregion

		#region ILanguageUpdated implementation
		
		public void Reload ()
		{
			ReloadLanguageModels ();
		}
		
		public void AddObverver (IASRObserver observer)
		{
			if (observer == null) {
				throw new ArgumentNullException("IASRObserver can not be null!");
			}
			m_observers.Add (observer);
		}
		
		#endregion


	}
}

