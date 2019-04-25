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
using System.Net;
using System.IO;
using Newtonsoft;
using R2Core;
using Newtonsoft.Json.Linq;


namespace R2Core.Audio.ASR
{
	public class GoogleSpeechFacade
	{
		private const string URL = "https://www.google.com/speech-api/v1/recognize?";
		private const string USER_AGENT = "chromium";
		private const int DEFAULT_SAMPLE_RATE = 8000;
		private const string DEFAULT_LANGUAGE = "en-US";
		private const int MAX_RESULT = 1;
		
		private int m_sampleRate;
		private string m_language;
		
		public GoogleSpeechFacade(string language = DEFAULT_LANGUAGE,
		                           int sampleRate = DEFAULT_SAMPLE_RATE) {
			m_sampleRate = sampleRate;
			m_language = language;
		}
		
		private string GoogleSpeechRequest(string flacFileName) {
	
			if (!System.IO.File.Exists(flacFileName)) {
				throw new IOException("Flac file does not exist: " + flacFileName);
			}
			
			HttpWebRequest request =
                    (HttpWebRequest)HttpWebRequest.Create(
                      URL +
				"xjerr=1" +
				"&client=" + USER_AGENT +
				"&lang=" + m_language +
				"&maxresults=" + MAX_RESULT +
				"&pfilter=0"
			);

			request.Proxy = null;

			ServicePointManager.ServerCertificateValidationCallback +=
                            delegate {
				return true;
			};

			request.Timeout = 6000;
			request.Method = "POST";
			request.KeepAlive = true;
			request.ContentType = "audio/x-flac; rate=" + m_sampleRate;
			request.UserAgent = USER_AGENT;

			byte[] data;

			using(FileStream fStream = new FileStream(
                                                flacFileName,
                                                FileMode.Open,
                                                FileAccess.Read)) {
				data = new byte[fStream.Length];
				fStream.Read(data, 0, (int)fStream.Length);
				fStream.Close();
			}
		
			using(Stream wrStream = request.GetRequestStream()) {
				wrStream.Write(data, 0, data.Length);
			}

			string returnString = null;
			Stream resp = null;
			
			try {
				resp = ((HttpWebResponse)request.GetResponse()).GetResponseStream();
				
				if (resp != null) {
					using(StreamReader sr = new StreamReader(resp)) {
						returnString = sr.ReadToEnd();
					}

					resp.Close();
					resp.Dispose();	
				} else {
					Log.w("SpeechFacade. Got NULL");
				}
			} catch (Exception ex) {
				
				Log.w("SpeechFacade.WARNING: " + ex.Message);
				
				
			}
			

			

			return returnString;
		}
		
		public string GetReply(string flacFileName) {
			string jsonResult = GoogleSpeechRequest(flacFileName);
			//Log.t("HEJ:" + jsonResult);
			if (jsonResult != null) {
				string [] jsonResults = jsonResult.Split('\n');
				jsonResult = null;
				foreach (string result in jsonResults) {
					if (result.Contains("\"status\":0")) {
						jsonResult = result;
					}
				}
				if (jsonResult != null) {
				
					//Log.d("PAPPA:" + jsonResult);
					JObject jsonObject = JObject.Parse(jsonResult);
					JToken hyp;
					if (jsonObject.TryGetValue("hypotheses", out hyp)) {
						if (hyp.First != null) {
							return(string)(hyp.First ["utterance"]);
						} 
						
					} else {
						Log.w("GoogleASR - strange reply: " + jsonResult);
					}
				}
				
			}
			
			Log.w("GoogleASR - Got no reply.");
			
			return null;
		}
		
	
	}
}

