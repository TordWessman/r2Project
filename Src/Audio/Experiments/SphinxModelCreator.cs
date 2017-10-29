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
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Core;
using System.Threading;


namespace Audio.ASR
{
	public class SphinxModelCreator : IModelCreator
	{
		private class WebClientWithGreatTimeout : WebClient
		{
		    protected override WebRequest GetWebRequest (Uri uri)
			{
				WebRequest w = base.GetWebRequest (uri);
				w.Timeout = DEFAULT_TIMEOUT;
		        return w;
		    }
		}
		
		private const int DEFAULT_TIMEOUT = 60 * 15;
		private bool m_isRunning;
		private const string POST_URL = "http://www.speech.cs.cmu.edu/cgi-bin/tools/lmtool/run";
		private const string BASE_FILE_URL = "http://www.speech.cs.cmu.edu/tools/product/";
		public 	const string LM_EXTENSION = ".lm";
		public 	const string DIC_EXTENSION = ".dic";
		public const string FILE_NAME_PREFIX = "language";

		private ICollection<ILanguageUpdated> m_observers;
		
		private string m_outputPath;
		private Task m_updateTask;

		
		public SphinxModelCreator (string outputPath)
		{
			if (outputPath.EndsWith (Path.DirectorySeparatorChar.ToString ())) {
				outputPath = outputPath.Substring (0, outputPath.Length - 1);
			}
			
			m_outputPath = outputPath;
			m_observers = new List<ILanguageUpdated> ();
		}
		
		public bool Ready {
			get {
				return !m_isRunning;
			}
		}
		
		//well... > http://stackoverflow.com/questions/1688855/httpwebrequest-c-sharp-uploading-a-file
		private string PostAndGetRawOutputHtml (string url, string fileName)
		{
			//Identificate separator
			string boundary = "---------------------------" + DateTime.Now.Ticks.ToString ("x");
//Encoding
			byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes ("\r\n--" + boundary + "\r\n");
			string contentType = "multipart/form-data; boundary=" + boundary;
//Creation and specification of the request
			HttpWebRequest wr = (HttpWebRequest)WebRequest.Create (url); //sVal is id for the webService
			
			wr.Timeout = DEFAULT_TIMEOUT;
			wr.ContentType = contentType;
			wr.Method = "POST";
			wr.KeepAlive = true;
			wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

			Stream rs = wr.GetRequestStream ();


			string fieldTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}"; //For the POST's format

//Writting of the file
			rs.Write (boundarybytes, 0, boundarybytes.Length);
			//rs.Write(string.Format(fieldTemplate,"formtype"));
			byte[] formTypeSimpleBytes = System.Text.Encoding.UTF8.GetBytes (
				string.Format (fieldTemplate, "formtype", "simple"));	
			rs.Write (formTypeSimpleBytes, 0, formTypeSimpleBytes.Length);
			
			//string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}"; //For the POST's format

//Writting of the file
			rs.Write (boundarybytes, 0, boundarybytes.Length);
			byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes (fileName);
			rs.Write (formitembytes, 0, formitembytes.Length);

			rs.Write (boundarybytes, 0, boundarybytes.Length);

			string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
			string header = string.Format (headerTemplate, "corpus", Path.GetFileName (fileName), contentType);
			byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes (header);
			rs.Write (headerbytes, 0, headerbytes.Length);

			FileStream fileStream = new FileStream (fileName, FileMode.Open, FileAccess.Read);
			byte[] buffer = new byte[4096];
			int bytesRead = 0;
			while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0) {
				rs.Write (buffer, 0, bytesRead);
			}
			fileStream.Close ();

			byte[] trailer = System.Text.Encoding.ASCII.GetBytes ("\r\n--" + boundary + "--\r\n");
			rs.Write (trailer, 0, trailer.Length);
			rs.Close ();
			rs = null;

			WebResponse wresp = null;
			try {
				//Get the response
				//while (!wr.HaveResponse) {
					Thread.Sleep (2000);
					Log.d ("waiting...");
				//}
				wresp = wr.GetResponse ();
				
				Stream stream2 = wresp.GetResponseStream ();
				StreamReader reader2 = new StreamReader (stream2);
				string responseData = reader2.ReadToEnd ();
				
				return responseData;
			} catch (Exception ex) {
				Core.Log.x (ex);
			} 
			
			return null;
		}
		
		public string BasePath { get {
				return m_outputPath + Path.DirectorySeparatorChar;
			}}
		
		public void AddObserver (ILanguageUpdated observer)
		{
			m_observers.Add (observer);
		}
		
		public void CreateLanguageFiles (IEnumerable<string> output, string corpusFileName, string wordsFileName)
		{
			Log.d ("Creating corpus and words files.");
			
			if (File.Exists (corpusFileName)) {
				File.Delete (corpusFileName);
			}
			
			if (File.Exists (wordsFileName)) {
				File.Delete (wordsFileName);
			}
			
			
			IList<string> words = new List<string> ();
			
			using (StreamWriter corpuslWriter = File.AppendText(corpusFileName)) {
				using (StreamWriter wordsWriter = File.AppendText(wordsFileName)) {
					foreach (string line in output) {
						foreach (string word in new Regex(@"[\s]+").Split(line)) {
							if (!words.Contains (word)) {
								words.Add (word);
								wordsWriter.WriteLine (word);
							}
						}
						corpuslWriter.WriteLine ("<s> " + line + " </s>");
					}
				
					wordsWriter.Close ();
				}
				
				corpuslWriter.Close ();
			}
		}
		
		public Task UpdateAsync (string corpusFileName)
		{
			if (m_isRunning) {
				throw new InvalidOperationException ("Unable to update: update is in progress");
			}
			
			m_isRunning = true;
			
			m_updateTask = new Task (() => {
				
				
				Log.d ("Sending output to server");
				string rawHtml = PostAndGetRawOutputHtml (POST_URL, corpusFileName);
				Log.d ("Creating lm & dic files from output");
				if (rawHtml != null) {
					
					string corpusPrefix = new Regex (@"\<b\>[\d]{4}\<\/b\>").Match (rawHtml).Value;
					string productId = new Regex (@"product\/[\d]+\_[\d]+\<\/title\>").Match (rawHtml).Value;
			
					productId = productId.Substring (8, productId.Length - 16);
					corpusPrefix = corpusPrefix.Substring (3, 4);
					WebClient wc = new WebClientWithGreatTimeout ();
					
					Log.d ("Downloading lm data");
					byte [] lmData = wc.DownloadData (BASE_FILE_URL + productId + "/" + corpusPrefix + LM_EXTENSION);
					Log.d ("Downloading dic data");
					byte [] dicData = wc.DownloadData (BASE_FILE_URL + productId + "/" + corpusPrefix + DIC_EXTENSION);

					string lmFileName = m_outputPath + Path.DirectorySeparatorChar + FILE_NAME_PREFIX + LM_EXTENSION;
					string dicFileName = m_outputPath + Path.DirectorySeparatorChar + FILE_NAME_PREFIX + DIC_EXTENSION;
					

					SaveDataToFile (lmFileName, lmData);
					SaveDataToFile (dicFileName, dicData);
					
					Log.d ("Langue updated!");
					
					foreach (ILanguageUpdated observer in m_observers) {
						observer.Reload ();
					}
				
					
					
				} else {
					Log.e ("unable to update file: " + corpusFileName + ". Bad response from server: " + POST_URL);
					

				}
				
				m_isRunning = false;
			}
			);
			
			m_updateTask.Start ();
			
			return m_updateTask;
			//m_updateTask.Wait ();

		}
		
		private void SaveDataToFile (string fileName, byte[] fileData)
		{
			if (File.Exists (fileName))
				File.Delete (fileName);
			
			File.WriteAllBytes (fileName, fileData);
		}
	}
}

