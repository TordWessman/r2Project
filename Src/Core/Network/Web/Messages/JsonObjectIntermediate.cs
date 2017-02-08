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
using Core.Device;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace Core.Network.Web
{
	//TODO: finish this class...

	/// <summary>
	/// Used as a raw intermediate type. 
	/// </summary>
	public class JsonObjectIntermediate : IWebIntermediate
	{
		private dynamic m_data;

		public JsonObjectIntermediate (dynamic data)
		{
			m_data = data; 
		}

		public void CLRConvert() {}

		public void AddMetadata(string key, object value) {}

		public dynamic Data { get { return m_data; } set { m_data = value; } }

		public IDictionary<string, object> Metadata { get { return new Dictionary<string, object>(); } }
	}
}

