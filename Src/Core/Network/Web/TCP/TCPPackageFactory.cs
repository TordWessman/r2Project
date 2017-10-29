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
using System.Collections.Generic;
using System.Dynamic;
using Core.Device;
using Core.Data;
using System.Linq;
using System.IO;

namespace Core.Network
{
	internal static class StreamExtensions {
	
		public static byte[] Read(this Stream self, int size) {

			byte[] buff = new byte[size];
			self.Read (buff, 0, size);
			return buff;

		}

		public static int ReadInt(this Stream self, int? size = null) {
		
			return new Int32Converter( Read (self, size ?? Int32Converter.ValueSize)).Value;
		
		}

	}

	public class TCPPackageFactory: ITCPPackageFactory 
	{
		/// <summary>
		/// Defines the data types which are transmittable
		/// </summary>
		public enum PayloadType: int {

			// Complex object
			Dynamic = 1,

			//CLR string
			String = 2,

			// Raw byte array
			Bytes = 3

		}

		private ISerialization m_serialization;

		public TCPPackageFactory (ISerialization serialization) {

			m_serialization = serialization;

		}

		public byte[] SerializeMessage(TCPMessage message) {
		
			byte[] code = new Int32Converter (message.Code).GetContainedBytes(2);
			byte[] path = m_serialization.Encoding.GetBytes (message.Destination ?? "");
			byte[] headerData = message.Headers != null ? m_serialization.Serialize (message.Headers) : new byte[0];
			byte[] payloadData = new byte[0];
			byte[] payloadDataType = new byte[0];

			if (!Object.ReferenceEquals(message.Payload, null)) {
			
				if (message.Payload is byte[]) {

					payloadData = message.Payload as byte[];
					payloadDataType = new Int32Converter ((int)PayloadType.Bytes).GetContainedBytes (2);

				} else if (message.Payload is string) {

					payloadData = m_serialization.Encoding.GetBytes (message.Payload);
					payloadDataType = new Int32Converter ((int)PayloadType.String).GetContainedBytes (2);

				} else {

					payloadData = m_serialization.Serialize (message.Payload);
					payloadDataType = new Int32Converter ((int)PayloadType.Dynamic).GetContainedBytes (2);

				}

			}

			byte[] pathSize = new Int32Converter (path.Length).Bytes; 
			byte[] payloadSize = new Int32Converter (payloadData.Length).Bytes;
			byte[] headerSize = new Int32Converter (headerData.Length).Bytes;

			return CreateRawPackage (code, pathSize, headerSize, payloadSize, path, headerData, payloadDataType, payloadData);
		
		}

		public dynamic DeserializePayload(TCPMessage message) {
		
			if (message.PayloadType == PayloadType.Dynamic) {
			
				return m_serialization.Deserialize (message.Payload);
					
			} else if (message.PayloadType == PayloadType.String) {
			
				return m_serialization.Encoding.GetString(message.Payload);

			}

			return message.Payload;

		}

		public TCPMessage DeserializePackage(Stream stream) {
			
			int code = stream.ReadInt (2);
			int pathSize = stream.ReadInt ();
			int headerSize = stream.ReadInt ();
			int payloadSize = stream.ReadInt ();
			byte[] path = pathSize > 0 ? stream.Read (pathSize) : new byte[0];
			byte[] headers = headerSize > 0 ? stream.Read (headerSize) : new byte[0];
			PayloadType payloadType = (PayloadType)stream.ReadInt (2);
			byte[] payloadData = stream.Read (payloadSize);

			dynamic payload;

			if (payloadType ==  PayloadType.Bytes) {

				payload = payloadData;

			} else if (payloadType == PayloadType.String) {

				payload = m_serialization.Encoding.GetString(payloadData);

			} else {

				payload = m_serialization.Deserialize(payloadData);

			}

			return new TCPMessage () { 
				Destination = m_serialization.Encoding.GetString (path),
				Headers = m_serialization.Deserialize (headers),
				Payload = payload,
				Code = code,
				PayloadType = payloadType
			};

		}

		/// <summary>
		/// Concatinates the byte arrays into a slingle byte[] (using parameter order).
		/// </summary>
		/// <returns>The raw package.</returns>
		/// <param name="datasets">Datasets.</param>
		private byte[] CreateRawPackage(params byte[][] datasets) {

			int size = 0;
			foreach (byte[] dataset in datasets) { size += dataset.Length; }

			byte[] data = new byte[size];

			int position = 0;

			foreach (byte[] dataset in datasets) {

				if (dataset != null) {
				
					Array.Copy (dataset, 0, data, position, dataset.Length);
					position += dataset.Length;

				}

			}

			return data;

		}
	}

}

