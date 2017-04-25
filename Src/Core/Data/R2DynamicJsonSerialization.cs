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
using Core.Device;

namespace Core.Data
{
	/// <summary>
	/// Allows serialization of (generic) non-primitive data types.
	/// </summary>
	public class R2DynamicJsonSerialization: DeviceBase, IR2Serialization {

		/// <summary>
		/// Packages with this header is considered to be serialized using this serialization engine.
		/// </summary>
		private readonly byte[] R2DynamicHeaderDefenition = new byte[4] {0,0xFF,0,0xFE};

		private enum R2SerializationType: byte {
		
			// Complex object
			Dynamic = 0,

			//CLR string
			String = 1,
	
			// Raw byte array
			Bytes = 2
				
		}

		private System.Text.Encoding m_encoding;
		private ExpandoObjectConverter m_converter;

		public R2DynamicJsonSerialization(string id, System.Text.Encoding encoding = null): base(id) {

			m_encoding = encoding ?? System.Text.Encoding.UTF8;
			m_converter = new ExpandoObjectConverter();

			JsonConvert.DefaultSettings =  delegate() { return new JsonSerializerSettings {
					NullValueHandling = NullValueHandling.Ignore,
					MissingMemberHandling = MissingMemberHandling.Ignore
				};
			};

		}

		public System.Text.Encoding Encoding { get { return m_encoding; } }

		public byte[] Serialize (dynamic obj) {

			//TODO: fix generic serialization of numeric values.
			byte type = 0;
			byte[] data;

			if (obj is string) {

				type = (byte) R2SerializationType.String;
				data = m_encoding.GetBytes (obj);

			} else if (obj is byte[]) {

				type = (byte) R2SerializationType.Bytes;
				data = obj;

			} else {
			
				// Data will be serialized to a JSON object string defore transformed into raw byte data.
				type = (byte) R2SerializationType.Dynamic;
				string outputString = Convert.ToString (JsonConvert.SerializeObject (obj));
				data = m_encoding.GetBytes(outputString) ?? new byte[0];

			}

			byte[] serialized = new byte[1 + R2DynamicHeaderDefenition.Length + data.Length];
			serialized [R2DynamicHeaderDefenition.Length] = type;
			Array.Copy (data, 0, serialized, 1 + R2DynamicHeaderDefenition.Length, data.Length);
			Array.Copy (R2DynamicHeaderDefenition, 0, serialized, 0, R2DynamicHeaderDefenition.Length);

			return serialized;

		}

		private bool IsR2DynamicSerialized (byte[] data) {
		
			if (data.Length < R2DynamicHeaderDefenition.Length) { return false; }

			for (int i = 0; i < R2DynamicHeaderDefenition.Length; i++) {
			
				if (data[i] != R2DynamicHeaderDefenition[i]) { return false; }
			
			}

			return true;

		}

		public dynamic Deserialize (byte[] data) {


			if (IsR2DynamicSerialized(data)) {

				byte[] payload = new byte[data.Length - 1 - R2DynamicHeaderDefenition.Length];
				Array.Copy (data, 1 + R2DynamicHeaderDefenition.Length, payload, 0, payload.Length);

				if (data [0] == (byte)R2SerializationType.Bytes) {
				
					return payload;

				} else if (data [0] == (byte)R2SerializationType.String) {

					return m_encoding.GetString (payload);

				} else {

					//Deserialize complex object
					return new R2Dynamic(JsonConvert.DeserializeObject<ExpandoObject>(m_encoding.GetString (payload), m_converter));

				}

			} 

			//No header found. Try to deserialize as a complex object
			return new R2Dynamic(JsonConvert.DeserializeObject<ExpandoObject>(m_encoding.GetString (data), m_converter));


		}

	}

}