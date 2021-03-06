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
using System.Collections.Generic;
using R2Core;
using R2Core.Device;
using System.Threading.Tasks;

namespace R2Core.Audio.TTS
{
	public class EspeakTTS : DeviceBase, ITTS {
		
		private const string dllPath = "libr2espeak.so";
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
   	 	protected static extern int _ext_init_espeak();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_speak_espeak(string text);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_set_rate_espeak(int rate);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_set_pitch_espeak(int pitch);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_stop_espeak();
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_pause_espeak();
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_is_playing_espeak();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern bool _ext_is_initialized_espeak();
		
		private string m_currentText;
		private bool m_isQuiet;
		private bool m_isStarted;
		
		private IList<ITTSObserver> m_observers;
		
		private static readonly object m_lock = new object();					
		
		public EspeakTTS(string identifier) : base(identifier) {

			m_observers = new List<ITTSObserver>();
			_ext_init_espeak();
		
		}
		

		public void Say(string text) {

			if (text == null && text.Trim().Length == 0) { return; }

			if (!m_isStarted) {
				throw new DeviceException("Unable to speak: not started!");
			}

			m_currentText = text;

			Task.Factory.StartNew( () => {
			
				foreach (ITTSObserver observer in m_observers) {

					observer.TalkStarted(this);

				}

				_ext_speak_espeak(text);

				foreach (ITTSObserver observer in m_observers) {

					observer.TalkEnded(this);

				}

			});

		}

		public bool IsQuiet {

			get {

				return m_isQuiet;
			
			}
			set {
			
				_ext_pause_espeak();

				m_isQuiet = value;
			
			}
		
		}

		public string CurrentText {
		
			get {
			
				return m_currentText;
			
			}
		
		}
		
		
		public override void Start() {
			if (m_isStarted) {
			
				return;
			
			}
			
			if (_ext_init_espeak() != 1) {
			
				throw new DeviceException("Unable to initialize espeak.");
			}

			m_isStarted = true;

		}

		public override void Stop() {

			_ext_stop_espeak();
			m_isStarted = false;
		
		}

		public override bool Ready {

			get {

				return(_ext_is_playing_espeak() == 0) && 
						m_isStarted && 
					_ext_is_initialized_espeak();

			}

		}
		
		public void AddObserver(ITTSObserver observer) 
		{

			if (m_observers.Contains(observer)) {

				throw new DeviceException("Observer already added for me: " + m_id);
			}

			m_observers.Add(observer);

		}

	}

}

