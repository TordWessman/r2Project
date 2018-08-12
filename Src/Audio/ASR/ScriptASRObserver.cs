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
using R2Core.Device;
using R2Core.Audio.ASR;
using R2Core.Scripting;
using R2Core.Audio.TTS;

namespace R2Core.Audio.ASR
{
	public class ScriptASRObserver: DeviceBase, IASRObserver, ITTSObserver
	{

		public const string INTERPRET_FUNCTION_NAME = "interpretx";
		public const string SPEACH_START_FUNCTION_NAME = "speach_start";
		public const string SPEACH_END_FUNCTION_NAME = "speach_end";

		private IScript m_script;

		public ScriptASRObserver (string id, IScript script): base (id)
		{
			m_script = script;
		}

		public void TextReceived(string text) {
		
			m_script.Invoke (INTERPRET_FUNCTION_NAME, text);

		}


		public void TalkStarted(ITTS tts) {
		
			m_script.Invoke (SPEACH_START_FUNCTION_NAME, tts);
		}

		public void TalkEnded(ITTS tts) {

			m_script.Invoke (SPEACH_END_FUNCTION_NAME, tts);
		
		}

	}
}

