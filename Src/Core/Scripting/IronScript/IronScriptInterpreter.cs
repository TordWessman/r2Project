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

namespace Core.Scripting
{
	public class IronScriptInterpreter: IScriptInterpreter
	{
		// The interpret method of the script. Takes a string argument containing the command to be interpreted.
		public const string METHOD_INTERPRET = "interpret"; 

		private IronScript m_rubyScript;
		private string m_interpretMethodName;

		/// <summary>
		/// The IScript must be of type 
		/// </summary>
		/// <param name="rubyScript">Ruby script.</param>
		public IronScriptInterpreter (IronScript rubyScript, string interpretMethodName = METHOD_INTERPRET)
		{

			m_rubyScript = rubyScript;
			m_rubyScript.Invoke (IronScript.HANDLE_SETUP);
			m_interpretMethodName = interpretMethodName;
		
		}

		public bool Interpret(string expression) {

			return m_rubyScript.Invoke(m_interpretMethodName, expression);
		
		}

	}

}