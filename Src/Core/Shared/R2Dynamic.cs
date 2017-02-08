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
using System.Dynamic;
using System.Collections.Generic;

namespace Core
{
	/// <summary>
	/// Base implementation of an object with dynamic members.
	/// </summary>
	public class R2Dynamic: DynamicObject
	{

		private IDictionary<string, object> m_members;

		public R2Dynamic(ExpandoObject expandoObject) {
			
			m_members = new Dictionary<string,object> ();

			foreach (KeyValuePair<string, Object> kvp in (expandoObject as IDictionary<string, Object>)) {

				var obj = kvp.Value is ExpandoObject ? new R2Dynamic (kvp.Value as ExpandoObject) : kvp.Value;
				m_members [kvp.Key] = obj;

			}

		}

		public R2Dynamic ()
		{

			m_members = new Dictionary<string,object> ();

		}

		public override bool TrySetMember (SetMemberBinder binder, object value) {

			m_members [binder.Name] = value;
			return true;

		}

		public override bool TryGetMember (GetMemberBinder binder, out object result) {

			result = m_members.ContainsKey (binder.Name) ? m_members [binder.Name] : null;

			return true;

		}

		public void SetMember(string key, object value) {
		
			m_members [key] = value;

		}


		public bool ContainsMember(string key) { return m_members.ContainsKey (key); }

	}

}