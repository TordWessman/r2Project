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
using R2Core.Device;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using R2Core;
using System.IO;

namespace R2Core.Audio.ASR
{
	public class GoogleASR : DeviceBase, IASR {
		
		private const string FLAC_FILE = "test.flac.tmp";
		
		protected delegate void FileRecordedCallBack(string fileName);
		protected delegate void ErrorCallBack(int errorType, string message);
		
		private const string dllPath = "GstVader.so";
	
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
			string host,
			int port,
		   	FileRecordedCallBack msgFunc,
			ErrorCallBack errorFunc
		);
		
		private const int MINIMUM_RAW_FILE_SIZE = 0;
		protected bool m_isRunning;
		protected Task m_asrTask;
		
		private static ICollection<IASRObserver> m_observers;

		private FileRecordedCallBack m_fileRecordeddCallBack;
		private ErrorCallBack m_errorReceivedCallBack;
		private FlacConverter m_flacConverter;
		private ITaskMonitor m_taskMonitor;
		private GoogleSpeechFacade m_facade;
		
		public GoogleASR(string id, string host, int port, ITaskMonitor taskMonitor) : base(id) {
			
			m_observers = new List<IASRObserver>();
			
			m_fileRecordeddCallBack = new FileRecordedCallBack(this.DefaultFileRecorded);
			m_errorReceivedCallBack = new ErrorCallBack(this.DefaultErrorReceived);
			m_flacConverter = new FlacConverter();
			m_taskMonitor = taskMonitor;
			m_facade = new GoogleSpeechFacade();
				
			m_asrTask = new Task(() => {
				
				while (m_isRunning) {
					if (asr_start() != 0) {
						throw new ExternalException("Unable to start ASR!");
					}
				}
			}
			);

			if (asr_init(
				host,
				port,
				m_fileRecordeddCallBack, 
			    m_errorReceivedCallBack

			) != 0)
			
				throw new ExternalException("Unable to initialize ASR engine");
		}
		
		public bool Active 
		{
			get { return get_is_active(); }
			set { set_is_active(value); }

		}
		
		public void AddObserver(IASRObserver observer) {
			
			if (observer == null) {
				
				throw new ArgumentNullException("IASRObserver can not be null!");
			
			}

			m_observers.Add(observer);
		
		}

		public override void Start() {

			if (m_isRunning)
				throw new DeviceException("ASR is already running...");
			
			m_isRunning = true;
			m_asrTask.Start();
		}
		
		public override void Stop() {
			Log.d("Stopping Google ASR...");
			m_isRunning = false;
			asr_turn_off();
			Log.t("..asr stopped");
		}
		
		
		protected void DefaultFileRecorded(string fileName) {
			if (new FileInfo(fileName).Length > MINIMUM_RAW_FILE_SIZE) {
				Task transcriptionTask = new Task(() => {
				
					Log.d("ASR: Starting transcription...");
				
					m_flacConverter.Convert(fileName, FLAC_FILE);
					string response = m_facade.GetReply(FLAC_FILE);
					
					if (response != null) {
						Log.t("GOT RESPONSE: " + response);
						foreach (IASRObserver observer in m_observers) {
							observer.TextReceived(response);
						}
					}
				
				}
				);
			
				m_taskMonitor.AddTask("GOOGLE ASR TRANSCRIPTION", transcriptionTask);
				transcriptionTask.Start();
			}
			
		}
		
		protected void DefaultErrorReceived(int errorType, string errorMessage) {
			Log.e("ASR ERROR: " + errorMessage + " type: " + errorType.ToString());
			
		}
		
		#region ITaskMonitored implementation
		public IDictionary<string,Task> GetTasksToObserve() {
			return new Dictionary<string, Task>() {{"GOOGLE ASR", m_asrTask}};
		}
		#endregion

		#region IDeviceBase implementation

		public override bool Ready {
			get {
				return m_isRunning && get_is_running();
			}
		}
		#endregion

		#region ILanguageUpdated implementation
		public void Reload() {
			
		}
		#endregion


	}
}

