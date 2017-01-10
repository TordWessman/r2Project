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

using System.Xml.XPath;
using System.Xml;
using System.Xml.Xsl;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Core.Device;

namespace Audio.ASR
{
	public class StaticAnswerMachine : DeviceBase, ISpeechInterpreter
	{
		
		private Random m_randomISH;
		
		
		private bool m_isReady;
		private string m_xmlLanguageFile;
		private List<string> m_paragraphs;
		
		private System.Collections.Hashtable m_paragraphsU;
		
		public StaticAnswerMachine (string id, string xmlLanguageFile) : base (id)
		{
		
			m_randomISH = new Random ( );
			m_xmlLanguageFile = xmlLanguageFile;
		}
		
		public bool KnowReply (string question)
		{
			if (!m_isReady)
				throw new InvalidOperationException ("OOPs. concurency problem: Answering machine not ready: reloading data!");
	
			question = question.ToLower ();
			
			foreach (string paragraph in m_paragraphs) {
				if (new Regex ("^" + paragraph).IsMatch (question.ToLower ())) {
					return true;
				}
			}
			
			return false;
		}
		
		public string GetReply (string question)
		{
			
			if (!m_isReady)
				throw new InvalidOperationException ("OOPs. concurency problem: Answering machine not ready: reloading data!");
			
			
			foreach (string paragraph in m_paragraphs)
			{
				if (new Regex("^" + paragraph).IsMatch(question.ToLower()))
				{
					
					foreach (string ss in (List<string>)m_paragraphsU[paragraph])
						Console.WriteLine( " -- " + ss);
					int i = m_randomISH.Next(((List<string>)m_paragraphsU[paragraph]).Count);
					Console.WriteLine("regexp:{0} matches:{1}", paragraph.ToLower(), question.ToLower());
					string reply = ((List<string>)m_paragraphsU[paragraph])[i];
					
					return reply;
				}
			}
			return null;
	
		}
		
		private void LoadLanguageData ()
		{
			//TODO: handle double question entries
			XPathDocument source = new XPathDocument (m_xmlLanguageFile);
			XPathNodeIterator paragraphIterator = source.CreateNavigator ().Select ("//speech/p");
			
			m_paragraphsU = new System.Collections.Hashtable ();
			//paragraphs = new Dictionary<string, List<string>>();
			m_paragraphs = new List<string> ();
			//List<string> unsortedP = new List<string>();

			while (paragraphIterator.MoveNext()) {
				
				List<string> answers = new List<string> ();
				
				XPathNodeIterator answerIterator = (XPathNodeIterator)paragraphIterator.Current.Select ("a");
				while (answerIterator.MoveNext())
					answers.Add (answerIterator.Current.Value);
				
				XPathNodeIterator questionIterator = (XPathNodeIterator)paragraphIterator.Current.Select ("q");
				while (questionIterator.MoveNext()) {
					m_paragraphsU.Add (questionIterator.Current.Value.ToLower (), answers);
					//Console.WriteLine(questionIterator.Current.Value);
				}
				
			}
			
			foreach (string key in from s in m_paragraphsU.Keys.Cast<string>() orderby s.Length descending select s) {
				
				//paragraphs.Add(key.ToLower(),(List<string>)paragraphsU[key]);
				m_paragraphs.Add (key);
			}

		}
		
		public override void Start ()
		{
			LoadLanguageData ();
			m_isReady = true;
		}
		
		
		public override void Stop ()
		{
			m_isReady = false;
		}
		
		
		public override bool Ready {
			get {
				return m_isReady;
			}
		}

		#region ILanguageUpdated implementation
		public void Reload ()
		{
			m_isReady = false;
			LoadLanguageData ();
			m_isReady = true;
		}
		#endregion






	}
}

