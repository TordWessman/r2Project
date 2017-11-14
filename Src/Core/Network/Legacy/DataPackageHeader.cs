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
using Core.Device;
using System.Collections.Generic;

namespace Core.Network.Data
{
	public class DataPackageHeader : IDataPackageHeader
	{
		private IDictionary<string,string> m_values;

		public ICollection<string> GetKeys ()
		{
			return m_values.Keys;
		}
		
		public DataPackageHeader (IDictionary<string,string> values)
		{
			/*
			foreach (string key in values.Keys) {
				Console.WriteLine ("ADDED KEY: " + key + " and value: " + values [key]);
			}*/
			m_values = values;
		}

		public string GetValue (string key)
		{
			if (!m_values.ContainsKey(key)) {
				throw new KeyNotFoundException("Header has no key named: '" + key + "'");
			}
			//Console.WriteLine ("KEY: " + key);
			return m_values[key];
		}

	}
}

