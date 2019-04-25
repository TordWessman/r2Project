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
using R2Core;
using AIMLbot;
using R2Core.Scripting;
using System.Threading.Tasks;

namespace R2Core.Audio.ASR
{
	public class ASRInterpreter<T>: DeviceBase, IASRInterpreter where T: IScript
	{
		private string m_TTSId;
		private ISpeechInterpreter m_speechInterpreter;
		private IASR m_asr;
		private IDeviceManager m_deviceManager;
		private CommandInterpreter<T>m_commandInterpreter;
		
		private IConversationLock m_conversationLock;
		private Bot m_bot;
 		private User m_user;
		
		private bool m_ttsMode;
		
		public ASRInterpreter(string id, ISpeechInterpreter speechInterpreter,
							CommandInterpreter<T>commandInterpreter,
		                       IASR asr, IDeviceManager deviceManager, ITaskMonitor taskMonitor,
		                      string aimlPath,
		                   	  string settingsPath = "/../aimlConfig",
			string botPathToConfigFiles = "/Settings.xml") : base(id) {

			m_speechInterpreter = speechInterpreter;
			m_asr = asr;
			m_deviceManager = deviceManager;
			m_commandInterpreter = commandInterpreter;
			
			m_bot = new Bot();
			
			m_bot.PathToAIML = aimlPath;
			m_bot.PathToConfigFiles = aimlPath + settingsPath;
			Task loadTask = new Task(() => {
				Log.d("Loading chat bot settings.");
				DateTime d = DateTime.Now;
				m_bot.loadSettings(m_bot.PathToConfigFiles + botPathToConfigFiles);
				m_bot.isAcceptingUserInput = false;
				//m_bot.loadFromBinaryFile("test.bin");
				m_bot.loadAIMLFromFiles();
				m_bot.isAcceptingUserInput = true;
				m_ttsMode = true;
				m_user = new User ("axle", m_bot);
			
				//m_bot.saveToBinaryFile("test.bin");
				//Log.d("Done. Saving to file.");
				Log.d("Done: " + (DateTime.Now - d).TotalMilliseconds);
			}
			);
			
			taskMonitor.AddTask("load bot files", loadTask);
			
			loadTask.Start();
		}
		
		~ASRInterpreter () {
			//m_bot.saveToBinaryFile("test.bin");
		}
		
		private string GetBotResponse(string text) {
			if (m_bot.isAcceptingUserInput) {
				Request r = new Request(text, m_user, m_bot);
				Result res = m_bot.Chat(r);
				return res.Output;
			}
			
			Log.w("Bot is not accepting response yet. Waiting for language to be loaded");
			
			return "";
		}
		
		public void SetTTSId(string tts_Id) {
			m_TTSId = tts_Id;
		}
		
		public bool TTSIsActive {
			get {
				ITTS tts = m_deviceManager.Has(m_TTSId) ? 
						m_deviceManager.Get<ITTS> (m_TTSId) : null;

				if (tts == null) {
					Log.w("ASRInterpreter Cannot answer: No TTS set.");
				} else if (!tts.Ready) {
					Log.w("ASRInterpreter Cannot answer: TTS not ready");
				} else {
					return m_ttsMode;
				}
			
			return false;
		}
		}
		
		public void SetReplyMode(bool isActive) 
		{
			m_ttsMode = isActive;
		}
		
		private void AddLock(IConversationLock conversationLock) {
			if (m_conversationLock != null) {
				Log.w("Unable to lock conversation. The lock is held!");
			} else {
				m_conversationLock = conversationLock;
			}
			
		}
		
		public void ScriptLock(string scriptHandle, string methodHandle) {
			if (m_deviceManager.Has(scriptHandle)) {
				IScript script = m_deviceManager.Get<IScript> (scriptHandle);
				AddLock(new ScriptConversationLock(script, methodHandle));
				
			} else {
				Log.e("Script: " + scriptHandle + " could not be locked(" + 
					methodHandle + "), since it's not found by DeviceManager");
			}
			
		}
		
		
		#region IASRObserver implementation
		public void TextReceived(string text) {
			if (m_conversationLock != null) {
				if (m_conversationLock.TryRelease(text)) {
					Log.t("Releasing lock!");
					m_conversationLock = null;
				} else {
					Log.t("Keeping lock!");
				}
				return;
			}
			
			if (m_commandInterpreter.Execute(text)) {
				return;
			}
			
			string response = "";
				
			if (m_speechInterpreter.KnowReply(text)) {
				response = m_speechInterpreter.GetReply(text);
			} else {
				response = GetBotResponse(text);
			}
			
			Log.t("Robot response: " + response);
			
			if (TTSIsActive) {
				
				ITTS tts = m_deviceManager.Has(m_TTSId) ? 
						m_deviceManager.Get<ITTS> (m_TTSId) : null;
				

				
				m_asr.Active = false;
				tts.Say(response);
				m_asr.Active = true;
				
			}

		}
		#endregion

		#region ILanguageUpdated implementation
		public void Reload() {
			m_commandInterpreter.Reload();
			m_speechInterpreter.Reload();
		}
		#endregion
	}
}

