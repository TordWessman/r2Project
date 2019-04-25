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
using R2Core.Data;
using R2Core.Network;

namespace R2Core.Scripting.Network
{

	/// <summary>
	/// Default IHttpIntermediate implementation used for transporting http object data and headers.
	/// </summary>
	public class RubyWebIntermediate : IWebIntermediate {
		
		private dynamic m_data;
		private IDictionary<string, object> m_metadata;


		public RubyWebIntermediate() {
		
			Payload = new R2Dynamic();
			m_metadata = new  Dictionary<string, object>();
		}

		public dynamic Payload { get { return m_data; } set { m_data = value; } }
		public IDictionary<string, object> Headers { get { return m_metadata; } set { m_metadata = value; } } 
		public int Code { get; set; }
		public string Destination { get; set; }

		public void AddMetadata(string key, object value) {

			//m_metadata
			m_metadata[key] = value;

		}

		public void CLRConvert() {
		
			Payload = ParseValue(Payload);

		}

		/// <summary>
		/// Checks the type of the value and parses it accordingly
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="value">Value.</param>
		private dynamic ParseValue(dynamic value) {

			if (value is IWebIntermediate) {

				return value.Data;

			} else if (value is IronRuby.Builtins.MutableString) {

				IronRuby.Builtins.MutableString stringValue = value as IronRuby.Builtins.MutableString; 

				if (stringValue.IsBinary) {

					// The string was a container of raw byte data. Convert to byte[].

					return stringValue.GetBinarySlice(0);

				}

				return(string)value;

			} else if (value is IronRuby.Builtins.RubyArray) {

				ICollection<dynamic> array = new List<dynamic>();

				foreach (dynamic arrayValue in (ICollection<dynamic>)value) {

					array.Add(ParseValue(arrayValue));

				}

				return array;

			}  else if (value is IronRuby.Builtins.Hash) {

				IDictionary<string, dynamic> dictionary = new Dictionary<string, dynamic>();
				IronRuby.Builtins.Hash hash = (IronRuby.Builtins.Hash) value;

				foreach (object key in hash.Keys) {

					dictionary[key.ToString()] = ParseValue(hash[key]);

				}

				return dictionary;

			} else {

				return value;

			}

		}

	}

}