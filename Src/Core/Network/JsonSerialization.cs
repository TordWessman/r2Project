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
using Newtonsoft.Json;
using System.Dynamic;
using Newtonsoft.Json.Converters;
using R2Core.Device;

namespace R2Core.Network
{

	/// <summary>
	/// Allows serialization of (generic) non-primitive data types.
	/// </summary>
	public class JsonSerialization : DeviceBase, ISerialization {

		private readonly ExpandoObjectConverter m_converter;

		public JsonSerialization(string id, System.Text.Encoding encoding = null): base(id) {

			Encoding = encoding ?? System.Text.Encoding.UTF8;
			m_converter = new ExpandoObjectConverter();

			JsonConvert.DefaultSettings =  delegate() { return new JsonSerializerSettings {
					NullValueHandling = NullValueHandling.Ignore,
					MissingMemberHandling = MissingMemberHandling.Ignore
				};
			};

		}

		public System.Text.Encoding Encoding { get; private set; }

		public byte[] Serialize(dynamic obj) {
			
			if (obj is string) {

				return Encoding.GetBytes(obj);

			} else if (obj is byte[]) {

				return obj;

			}

			// Data will be serialized to a JSON object string before transformed into raw byte data.
			string outputString = Convert.ToString(JsonConvert.SerializeObject((object)obj));
			return Encoding.GetBytes(outputString) ?? new byte[0];

		}

		public dynamic Deserialize(byte[] data) {
			
			if (data.Length > 0) {

				//Deserialize complex object
				return new R2Dynamic(JsonConvert.DeserializeObject<ExpandoObject>(Encoding.GetString(data), m_converter));
	
			} 

			return new R2Dynamic();

		}

	}

}