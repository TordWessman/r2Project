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
using NLua;
using System.Linq;

namespace Core.Scripting
{
	public class LuaScript : DeviceBase, IScript
	{
		private Lua m_state;
		private string m_fileName;

		public LuaScript (string id, string fileName) : base (id)
		{
			
			m_state = new Lua ();
			m_fileName = fileName;

			Reload()
		
		}

		public void Set (string handle, dynamic value) {
		
			m_state [handle] = value;

		}

		public dynamic Get (string handle) {
		
			return m_state [handle];

		}

		public dynamic Invoke (string handle, params dynamic[] args) {
		
			LuaFunction function = m_state [handle] as LuaFunction;
		
			return function.Call (args)?.FirstOrDefault ();

		}

		public override void Start () {
			
		}

		public void Reload() {

			m_state.LoadCLRPackage ();
			m_state.DoFile (m_fileName);

		}
	
	}

}