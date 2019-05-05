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

namespace R2Core.Scripting
{
	/// <summary>
	/// Default IScriptInterpreter implementation.
	/// </summary>
	public class ScriptInterpreter : IScriptInterpreter {
		
		// The interpret method of the script. Takes a string argument containing the command to be interpreted.
		public const string METHOD_INTERPRET = "interpret"; 

		private IScript m_script;
		private string m_interpretMethodName;

		/// <summary>
		/// The IScript must implement the ´METHOD_INTERPRET´ (interpret) method. The default implementation
		/// of this script can be found in Common/run_loop_script.py
		/// </summary>
		/// <param name="script">IronScript implementing ´METHOD_INTERPRET´.</param>
		public ScriptInterpreter(IScript script, string interpretMethodName = METHOD_INTERPRET) {

			m_script = script;

			m_script.Invoke(IronScript.HANDLE_SETUP);
			m_interpretMethodName = interpretMethodName;
		
		}

		public bool Interpret(string expression) {

			return m_script.Invoke(m_interpretMethodName, expression);
		
		}

	}

}