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
using Core.Device;

namespace Core.Scripting
{
	public class LuaScriptFactory: DeviceBase, IScriptFactory<LuaScript>
	{
		private const string LUA_FILE_EXTENSION = ".lua";

		public LuaScriptFactory (string id) : base(id)
		{
		}

		public LuaScript CreateScript (string id, string sourceFile = null) {
		
			sourceFile = sourceFile ?? id + LUA_FILE_EXTENSION;

			return new LuaScript (id, sourceFile);

		}

		public IScriptProcess CreateProcess (string id, string sourceFile = null) {
		
			throw new NotImplementedException ();

		}

		public IScriptProcess CreateProcess (string id, LuaScript script) {

			throw new NotImplementedException ();

		}

		public ICommandScript CreateCommand (string id, string sourceFile = null) {

			throw new NotImplementedException ();

		}

		public string GetSourceFilePath (string id, string sourceFile = null) {

			throw new NotImplementedException ();

		}

		public IScriptInterpreter CreateInterpreter(LuaScript script) {

			throw new NotImplementedException ();

		}
	
	}
}

