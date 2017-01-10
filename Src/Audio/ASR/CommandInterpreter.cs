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
using System.Collections.Generic;
using System.Xml.XPath;
using System.Linq;
using Core.Scripting;

namespace Audio.ASR
{
	public class CommandInterpreter : ILanguageUpdated
	{
		private string m_xmlLanguageFile;
		private List<string> m_commands;
		private IScriptFactory m_commandScriptFactory;
		private string m_commandScriptTemplateFileName;
		
		public CommandInterpreter (string xmlLanguageFile,
		                           string commandScriptTemplateFile,
		                           IScriptFactory commandScript)
		{
			m_xmlLanguageFile = xmlLanguageFile;
			Reload ();
			m_commandScriptTemplateFileName = commandScriptTemplateFile;
			m_commandScriptFactory = commandScript;
			
		}
		
		public void Reload ()
		{
			XPathDocument source = new XPathDocument (m_xmlLanguageFile);
			XPathNodeIterator commandIterator = source.CreateNavigator ().Select ("//commands/command");
			
			m_commands = new List<string> ();
			
			while (commandIterator.MoveNext()) {
				m_commands.Add (commandIterator.Current.Value.ToLower ());
			}
		}
		
		public bool CanInterpretFromXml (string text)
		{

			if ((from c in m_commands where text.ToLower().StartsWith(c.ToLower()) select c).Any())
				return true;
			else
				return false;			

		}
		
		public bool Execute (string text)
		{
			ICommandScript script = m_commandScriptFactory.CreateCommand (
				"command_script", m_commandScriptTemplateFileName);
			
			object result = script.Execute (text.Trim ());
			
			return result is bool && ((bool)result) || CanInterpretFromXml(text);
			
			
		}
	}
}

