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
using System.Dynamic;

namespace Core.Network.Http
{

	/// <summary>
	/// 
	/// Used to pass parsed input json to the IHttpObjectReceiver (Currently used since it enables us to expose various ExpandoObject properties to scripts).
	/// </summary>
	public class JsonExportObject<T> where T : IDictionary<string, Object> {

		private T m_exportObjectContainer;

		public JsonExportObject(T data) {

			m_exportObjectContainer = data;

		}

		/// <summary>
		/// Returns the actual data contained.
		/// </summary>
		/// <value>The data.</value>
		public T Data { get { return m_exportObjectContainer; } }


		/// <Docs>The object to locate in the current collection.</Docs>
		/// <para>Determines whether an object of the internal data type (T) contains a specific value.</para>
		/// <summary>
		/// Returns true if the key was found amon the object's members.
		/// </summary>
		/// <param name="key">Key.</param>
		public bool Contains(string key) {

			return m_exportObjectContainer.ContainsKey (key);

		}

		/// <summary>
		/// Used by scripts to retrieve data from this object.
		/// </summary>
		/// <param name="key">Key.</param>
		public Object Get(string key) {
		
			if (!Contains (key)) {
			
				return null;

			}

			if (m_exportObjectContainer [key] is T) {
			
				return new JsonExportObject<T> ((T) m_exportObjectContainer [key]);

			} else {
			
				return m_exportObjectContainer [key];

			}

		}

	}

}