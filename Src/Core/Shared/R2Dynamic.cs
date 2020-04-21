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
using TKey = System.String;
using TValue = System.Object;
using System.Collections;

namespace R2Core
{
	/// <summary>
	/// Represents a dynamic object which implements IDictionary and whose dynamic properties are exposed through the IDictionary interface..
	/// </summary>
	public class R2Dynamic : DynamicObject, IDictionary<TKey, TValue> {

		private IDictionary<TKey, TValue> m_members;

		public R2Dynamic(ExpandoObject expandoObject) {
			
			m_members = new Dictionary<TKey, TValue>();

			foreach (KeyValuePair<TKey, Object> kvp in (expandoObject as IDictionary<TKey, Object>)) {
				
				m_members[kvp.Key] = kvp.Value is ExpandoObject ? new R2Dynamic(kvp.Value as ExpandoObject) : kvp.Value;

			}

		}

		public R2Dynamic(IDictionary<TKey, TValue> dictionary) {
		
			m_members = dictionary;

		}

		public R2Dynamic() {

			m_members = new Dictionary<TKey, TValue>();

		}

		/// <summary>
		/// IronPython doesn't seem to be able to handle null return values. (Equivalent to this.ContainsKey)
		/// </summary>
		/// <returns><c>true</c> if this instance has memberName; otherwise, <c>false</c>.</returns>
		/// <param name="memberName">Member name.</param>
		public bool Has(string memberName) {

			return m_members.ContainsKey(memberName) && m_members[memberName] != null;

		}

		public override bool TrySetMember(SetMemberBinder binder, TValue value) {

			m_members[binder.Name] = value;
			return true;

		}

		public override bool TryGetMember(GetMemberBinder binder, out TValue result) {

			result = m_members.ContainsKey(binder.Name) ? m_members[binder.Name] : default(TValue);
			return true;

		}

		#region IDictionary

		public ICollection<TKey> Keys {
			get { return m_members.Keys; }
		}

		public ICollection<TValue> Values {
			get { return m_members.Values; }
		}

		//
		// Indexer
		//
		public TValue this[TKey key] {
			get { return m_members[key]; }
			set { m_members[key] = value; }
		}

		//
		// Methods
		//
		public void Add(TKey key, TValue value) { m_members.Add(key, value); }

		public bool ContainsKey(TKey key) { return m_members.ContainsKey(key); }

		public bool Remove(TKey key) { return m_members.Remove(key); }

		public bool TryGetValue(TKey key, out TValue value) { return m_members.TryGetValue(key, out value); }

		#endregion

		#region IEnumerable

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
			return m_members.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return m_members.GetEnumerator();
		}

		#endregion

		#region ICollection

		//
		// Properties
		//
		public int Count {
			get { return m_members.Count; }
		}

		public bool IsReadOnly {
			get { return m_members.IsReadOnly; }
		}

		//
		// Methods
		//
		public void Add(KeyValuePair<TKey, TValue> item) { m_members.Add(item); }

		public void Clear() {
			m_members.Clear();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item) { return m_members.Contains(item); }

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
			m_members.CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<TKey, TValue> item) {
			return m_members.Remove(item);
		}

		#endregion

		public override string ToString() {
			
			string dictionary = "";

			foreach (var kvp in m_members) {

				dictionary += $"[{kvp.Key}:{kvp.Value}]";

			}

			return $"[R2Dynamic: {dictionary}]";

		}

	}

}