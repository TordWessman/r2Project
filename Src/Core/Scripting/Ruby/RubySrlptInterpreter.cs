﻿// This file is part of r2Poject.
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
	public class RubySrlptInterpreter: IScriptInterpreter
	{
		private RubyScript m_rubyScript;

		/// <summary>
		/// The IScript must be of type 
		/// </summary>
		/// <param name="rubyScript">Ruby script.</param>
		public RubySrlptInterpreter (RubyScript rubyScript)
		{

			m_rubyScript = rubyScript;
		
		}

		public bool Interpret(string expression) {

			return m_rubyScript.Get(RubyScript.HANDLE_MAIN_CLASS).@interpret (expression);
		
		}

	}

}