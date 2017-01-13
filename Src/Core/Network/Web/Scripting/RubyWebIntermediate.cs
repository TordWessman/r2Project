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
using System.Dynamic;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Runtime.Remoting;

namespace Core.Network.Web
{

	/// <summary>
	/// Default IHttpIntermediate implementation used for transporting http object data and headers.
	/// </summary>
	public class RubyWebIntermediate: IWebIntermediate
	{
		private dynamic m_data;
		private NameValueCollection m_headers;

		public RubyWebIntermediate ()
		{
			m_data = new Dictionary<string, dynamic> ();
			m_headers = new NameValueCollection ();
		}

		public void AddHeader(string key, string value) {

			m_headers.Add (key, value);
		}

		public void SetValue(string key, dynamic value) {

			m_data.Add(key, ParseValue(value));

		}

		public IWebIntermediate New { get { return new RubyWebIntermediate(); } }

		public dynamic Data {
		
			get { return m_data; }
			set { m_data = value; }
		}

		public NameValueCollection Headers { get { return m_headers; } }

		/// <summary>
		/// Checks the type of the value and parses it accordingly
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="value">Value.</param>
		private dynamic ParseValue(dynamic value) {

			if (value is IWebIntermediate) {

				return value.Data;

			} else if (value is IronRuby.Builtins.MutableString) {
				
				return (string)value;

			} else if (value is IronRuby.Builtins.RubyArray) {

				ICollection<dynamic> array = new List<dynamic> ();

				foreach (dynamic arrayValue in (ICollection<dynamic>) value) {

					array.Add(ParseValue(arrayValue));

				}

				return array;

			}  else if (value is IronRuby.Builtins.Hash) {

				IDictionary<string, dynamic> dictionary = new Dictionary<string, dynamic> ();
				IronRuby.Builtins.Hash hash = (IronRuby.Builtins.Hash) value;

				foreach (object key in hash.Keys) {

					dictionary.Add (key.ToString(), ParseValue (hash[key]));

				}

				return dictionary;

			} else {

				return value;

			}
		}

	}
}