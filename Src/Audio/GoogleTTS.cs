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
using R2Core.Network;
using R2Core.Device;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace R2Core.Audio
{

	/// <summary>
	/// Using the Google translate API to create mpga-encoded audio which is played using the libr2mp3. 
	/// </summary>
	public class GoogleTTS: DeviceBase, ITTS {
		
		private const string dllPath = "libr2mp3.so";

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_init_mp3_memory();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_play_memory_mp3(byte[] data, int size);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_stop_mp3();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_pause_mp3();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_is_playing_mp3();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_stop_playback_mp3();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_is_initialized_mp3();

		private IMessageClient m_client;
		private Task m_sendTask;
		private string m_text;
		private string m_language = "en";

		public GoogleTTS(string id, WebFactory webFactory) : base(id) {
			
			m_client = webFactory.CreateHttpClient("http_client");

			if (_ext_init_mp3_memory() != 1) {
			
				throw new ApplicationException("Unable to initialize TTS");

			}

		}

		public void Say(string text) {

			m_text = text;

			HttpMessage message = new HttpMessage() {
				Method = "GET",
				Destination = $"http://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&q={System.Uri.EscapeDataString(text)}&tl={m_language}"
			};

			Log.t($"Sending: {message.Destination}");

			m_sendTask = m_client.SendAsync(message, (response) => {
			
				if (response.Payload is byte[]) {

					byte[] mp3Data = (byte[]) response.Payload;
					Log.t($"Got reply with length: {mp3Data.Length}!");
					_ext_play_memory_mp3 (mp3Data, mp3Data.Length);

				} else {

					Log.e($"Unable to play request: {message.Destination}. Response code: {response.Code}. Response payload: {response.Payload}.");

				}

			});

		}

		public bool IsQuiet {

			get { return _ext_is_playing_mp3 () == 1; }
			set { _ext_pause_mp3(); }

		}

		public string CurrentText { get { return m_text; } }

		public override void Stop() {
		
			_ext_stop_mp3();
		
		}

		public string Language { 
		
			get { return m_language; }
		
			set { m_language = value; } 
		
		}
	}
}

