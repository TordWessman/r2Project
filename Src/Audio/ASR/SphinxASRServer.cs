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
using Core.Network;

namespace Audio.ASR
{
	public class SphinxASRServer : DeviceBase, IASR, IEndpoint
	{
		
		protected delegate void TextInterpretedCallBack(string message);
		protected delegate void SphinxErrorCallBack(int errorType, string message);

		private const string dllPath = "libr2sphinx.so";
	
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		public static extern int _ext_asr_start();

		[DllImport( dllPath)]
		protected static extern int _ext_asr_turn_off();
		
		[DllImport(dllPath)]
		protected static extern void _ext_asr_set_is_active(bool reportMode);
		
		[DllImport(dllPath)]
		protected static extern bool _ext_asr_get_is_active();

		[DllImport(dllPath)]
		protected static extern bool _ext_asr_get_is_running();
		
		[DllImport(dllPath)]
   	 	protected static extern int _ext_asr_init(
		   	TextInterpretedCallBack msgFunc,
			SphinxErrorCallBack errorFunc,
		    string lmFile,
		    string dictFile,
		    string hmmFile,
			int port,
			string hostIp,
			bool as_server);
		

		protected bool m_isRunning;
		protected Task m_asrTask;

		//observers are called from a gstreamer thread...
		private static ICollection<IASRObserver> m_observers;

		private TextInterpretedCallBack m_textReceivedCallBack;
		private SphinxErrorCallBack m_errorReceivedCallBack;
		private int m_port;
		private string m_ip;

		public int Port { get { return m_port; } }
		public string Ip { get { return m_ip; } }

		/// <summary>
		/// Initailize and listen to local microphpone
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="lm">Lm.</param>
		/// <param name="dic">Dic.</param>
		/// <param name="hmmDir">Hmm dir.</param>
		public SphinxASRServer (string id, string lm = null, string dic = null, string hmmDir = null)
			: base (id) {

			initialize (null, 0, lm, dic, hmmDir, false);

		}

		/// <summary>
		/// Initialize and use as tcp server. Expecting audio to be received to the specified tcp address/port
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="hostIp">Host ip.</param>
		/// <param name="port">Port.</param>
		/// <param name="lm">Lm.</param>
		/// <param name="dic">Dic.</param>
		/// <param name="hmmDir">Hmm dir.</param>
		public SphinxASRServer (string id, int port, string hostIp, string lm = null, string dic = null, string hmmDir = null)
			: base (id)
		{
			initialize (hostIp, port, lm, dic, hmmDir, true);
		}


		public void initialize (string hostIp, int port, string lm, string dic, string hmmDir, bool as_tcp_server)
		{

			m_ip = hostIp;
			m_port = port;

			m_observers = new List<IASRObserver> ();
			
			m_textReceivedCallBack = new TextInterpretedCallBack (this.DefaultTextReceived);
			m_errorReceivedCallBack = new SphinxErrorCallBack (this.DefaultErrorReceived);
			
			m_asrTask = new Task (() => {
				
				while (m_isRunning) {
					
					if (_ext_asr_start () != 0) {
						
						throw new ExternalException ("Unable to start ASR!");
					
					}
				
				}

			});

			if (_ext_asr_init (
				    m_textReceivedCallBack, 
				    m_errorReceivedCallBack,
				    lm, dic, hmmDir,
				    port,
				    hostIp,
					as_tcp_server

			    ) != 0) {

				throw new ExternalException ("Unable to initialize ASR engine");

			}

		}

		public void SetActive(bool active) {
		
			Active = active;

		}
		
		public bool Active  { 
			
			get { return _ext_asr_get_is_active(); }
			set { _ext_asr_set_is_active(value); }
		
		}
		
		public override void Start ()
		{

			if (m_isRunning) {
				
				throw new DeviceException ("ASR is already running...");
			}
			
			m_isRunning = true;
			m_asrTask.Start ();
		}
		
		public override void Stop ()
		{
			m_isRunning = false;
			_ext_asr_turn_off ();
		}
		
		
		protected void DefaultTextReceived (string text)
		{
			Console.WriteLine ("ASR: " + text);

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
				return m_isRunning && _ext_asr_get_is_running ();
			}
		}
		#endregion



		#region IASR implementation
		private void ReloadLanguageModels ()
		{
			Stop ();
			while (_ext_asr_get_is_running()) {
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

