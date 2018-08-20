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
using R2Core.Audio.TTS;
using R2Core.Device;
using R2Core.Audio.ASR;
using R2Core.Scripting;

namespace R2Core.Audio
{
	public class AudioFactory : DeviceBase, IDevice
	{
		private string m_basePath;

		public AudioFactory (string id, string basePath) : base(id)
		{
			m_basePath = basePath;
		}

		public ITTS CreateEspeak(string id) {
			return new EspeakTTS (id);
		}

		public ITTS CreateGoogle(string id, R2Core.Network.WebFactory webFactory) {
		
			return new GoogleTTS (id, webFactory);

		}

		public IAudioPlayer CreateMp3Player(string id) {
			return new Mp3Player (id, m_basePath);
		}

		/// <summary>
		/// Creates an ASR server. The ASR server will send audio data received through the TCP server to the ASR engine.
		/// </summary>
		/// <returns>The sphinx server.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="port">Port.</param>
		/// <param name="hostIp">Host ip.</param>
		/// <param name="lm">Lm.</param>
		/// <param name="dic">Dic.</param>
		/// <param name="hmmDir">Hmm dir.</param>
		public IASR CreateSphinxServer(string id, int port, string hostIp, string lm = null, string dic = null, string hmmDir = null) {
		
			return new SphinxASRServer(id, port, hostIp, lm, dic, hmmDir);

		}

		/// <summary>
		/// Creates a ASR instance that uses local audio as input.
		/// </summary>
		/// <returns>The sphinx.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="lm">Lm.</param>
		/// <param name="dic">Dic.</param>
		/// <param name="hmmDir">Hmm dir.</param>
		public IASR CreateSphinx(string id, string lm = null, string dic = null, string hmmDir = null) {

			return new SphinxASRServer(id, lm, dic, hmmDir);

		}

		/// <summary>
		/// Will create a server that only deliver input to speakers.
		/// </summary>
		/// <returns>The mic receiver.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="port">Port.</param>
		/// <param name="hostIp">Host ip.</param>
		public IASR CreateMicReceiver(string id, int port, string hostIp) {

			return new SphinxASRServer(id, port, hostIp, null, null, null, true);

		}

		public IASRObserver CreateScriptObserver(string id, IScript script) {
		
			return new ScriptASRObserver (id, script);

		}

	}
}

