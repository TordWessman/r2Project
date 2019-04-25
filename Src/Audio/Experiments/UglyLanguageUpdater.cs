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
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using R2Core;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using System.Xml;
using R2Core.Device;

namespace R2Core.Audio.ASR
{
	public class UglyLanguageUpdater : DeviceBase, ILanguageUpdater
	{


		public const string CORPUS_FILE_NAME = "corupus.txt";
		public const string WORDS_FILE_NAME = "words.txt";
		
		IModelCreator m_modelCreator;
		
		private string m_xmlFileName;
		
		public UglyLanguageUpdater(string id, string xmlFileName, string outputPath) : base(id) {
		
			m_modelCreator = new SphinxModelCreator (outputPath);
			m_xmlFileName = xmlFileName;

		}
		
		public override bool Ready {
			get {
				return m_modelCreator.Ready;
			}
		}
		
		public string BasePath { get {
				return m_modelCreator.BasePath;
			}}
		
		public void AddObserver(ILanguageUpdated observer) {
			m_modelCreator.AddObserver (observer);
		}
		
		
		public Task UpdateAsync() {
			CreateCorpusFromLanguageFile(m_xmlFileName, CORPUS_FILE_NAME, WORDS_FILE_NAME);
			
			return m_modelCreator.UpdateAsync(CORPUS_FILE_NAME);
		}
		
		private void CreateCorpusFromLanguageFile(string sourceFileName, string corpusFileName, string wordsFileName) {
		
			if (!File.Exists(sourceFileName)) {
				throw new InvalidOperationException("Source XML-file does not exist: " + sourceFileName);
			}
			
			XPathDocument source = new XPathDocument(sourceFileName);
			
			List<string> output = new List<string>();
			List<string> words = new List<string>();
			
			XPathNodeIterator inputIterator = source.CreateNavigator ().Select("//q | //command");
			
			while(inputIterator.MoveNext()) {
				//remove everything that is not letters or spaces
				string line = new Regex(@"[^a-z\s\']+").Replace(inputIterator.Current.Value.ToLower ().Trim(), "");
				//adding lines to comprhends(sentences or words);
				output.Add(line);
				foreach (string word in new Regex(@"[\s]+").Split(line))
					if (!words.Contains(word))
						words.Add(word);
			}
			
			words.Sort();
			
			m_modelCreator.CreateLanguageFiles(output, corpusFileName, wordsFileName);
		}
		
		public void InsertCommands(IList<string> commands) {
				
			if (!Ready) {
				throw new InvalidOperationException("Unable to update language data. Update of some kind in progress.");
			}

			XmlTextReader reader = new XmlTextReader (m_xmlFileName);
			XmlDocument doc = new XmlDocument();
			doc.Load(reader);
			reader.Close();
	
			foreach (string question in commands) {
				XPathNodeIterator paragraphIterator = doc.CreateNavigator ().Select("//commands/command");
				
				while(paragraphIterator.MoveNext()) {
					if (HasQuestion(paragraphIterator.Current, question)) {
						
						Console.WriteLine("question alrready found: " + question);

					} else {
						doc.CreateNavigator ().SelectSingleNode("//commands").AppendChild("<command>" + question + "</command>");

					}
				}
			}
		
			doc.Save(m_xmlFileName);
	

			
		}

		
		public void InsertQuestionsAnswers(IList<string> questions, IList<string> answers) {
				
			if (!m_modelCreator.Ready) {
				throw new InvalidOperationException("Unable to update language data. Update of some kind in progress.");
			}

			XmlTextReader reader = new XmlTextReader (m_xmlFileName);
			XmlDocument doc = new XmlDocument();
			doc.Load(reader);
			reader.Close();
			
			bool foundQuestion = false;
			
			foreach (string question in questions) 
			{
				XPathNodeIterator paragraphIterator = doc.CreateNavigator().Select("//speech/p");
				
				while(paragraphIterator.MoveNext())
				{
					if (HasQuestion(paragraphIterator.Current, question)) 
					{
						//paragraphIterator.Current
						//Console.WriteLine("question alrready found: " + question);
						//paragraphIterator.Current.AppendChild(
						foundQuestion = true;
					}
				}
			}
			
			if (!foundQuestion) {
				XmlElement newP = doc.CreateElement("p");
				foreach (string question in questions) 
				{
					XmlElement newQ = doc.CreateElement("q");
					newQ.InnerText = question.ToLower();
					newP.AppendChild(newQ);
				}
				
				foreach (string answer in answers) 
				{
					XmlElement newA = doc.CreateElement("a");
					newA.InnerText = answer.ToLower();
					newP.AppendChild(newA);
				}
				
				doc.CreateNavigator().SelectSingleNode("//speech").AppendChild("<p>" + newP.InnerXml + "</p>");
				
				doc.Save(m_xmlFileName);

			}

			
		}
		
	
		private bool HasQuestion(XPathNavigator questionNode, string question) 
		{
			question = question.ToLower();
			
			XPathNodeIterator questionIterator = (XPathNodeIterator) questionNode.Select("q");
			while(questionIterator.MoveNext())
				if (((string)questionIterator.Current.Value).ToLower() == question)
					return true;
			
			return false;
		}





	}
}

