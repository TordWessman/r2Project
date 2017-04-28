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

namespace Core.Network.Web
{
	public class TCPPackageFactory
	{
		/// <summary>
		/// Defines the data types which are transmittable
		/// </summary>
		private enum PayloadType: int {

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

		public byte[] SerializePayload(dynamic payload) {
		
			return m_serialization.Serialize (payload);

		}

		public byte[] CreateTCPData(TCPPackage package) {
		
			byte[] code = new Int32Converter ((int)package.Code).GetContainedBytes(2);
			byte[] path = m_serialization.Encoding.GetBytes (package.Path);
			byte[] headerData = package.Headers != null ? m_serialization.Serialize (package.Headers) : new byte[0];
			byte[] payloadData = new byte[0];
			byte[] payloadDataType = new byte[0];

			if (package.Payload != null) {
			
				if (package.Payload is byte[]) {

					payloadData = package.Payload as byte[];
					payloadDataType = new Int32Converter ((int)PayloadType.Bytes).GetContainedBytes (2);

				} else if (package.Payload is string) {

					payloadData = m_serialization.Encoding.GetBytes (package.Payload);
					payloadDataType = new Int32Converter ((int)PayloadType.String).GetContainedBytes (2);

				} else {

					payloadData = m_serialization.Serialize (package.Payload);
					payloadDataType = new Int32Converter ((int)PayloadType.Dynamic).GetContainedBytes (2);

				}

			}

			byte[] pathSize = new Int32Converter (path.Length).Bytes; 
			byte[] payloadSize = new Int32Converter (payloadData.Length).Bytes;
			byte[] headerSize = new Int32Converter (headerData.Length).Bytes;

			return CreateRawPackage (code, pathSize, headerSize, payloadSize, path, headerData, payloadDataType, payloadData);
		
		}

		public TCPPackage CreateTCPPackage(byte [] rawData) {
		
			int position = 0;
			int code = new Int32Converter (rawData.Skip (position).Take (2)).Value;
			position += 2;
			int pathSize = new Int32Converter (rawData.Skip (position).Take (Int32Converter.ValueSize)).Value;
			position += Int32Converter.ValueSize;
			int headerSize = new Int32Converter (rawData.Skip (position).Take (Int32Converter.ValueSize)).Value;
			position += Int32Converter.ValueSize;
			int payloadSize = new Int32Converter (rawData.Skip (position).Take (Int32Converter.ValueSize)).Value;
			position += Int32Converter.ValueSize;

			byte[] path = rawData.Skip (position).Take (pathSize).ToArray();
			position += pathSize;
			byte[] headers = rawData.Skip (position).Take (headerSize).ToArray();
			position += headerSize;

			dynamic payload = null;

			if (payloadSize > 0) {

				PayloadType payloadType = (PayloadType) (new Int32Converter (rawData.Skip (position).Take (2)).Value);
				position += 2;
				byte[] payloadData = rawData.Skip (position).Take (payloadSize).ToArray();

				if (payloadType == PayloadType.String) {

					payload = m_serialization.Encoding.GetString (payloadData);

				} else if (payloadType == PayloadType.Dynamic) {

					payload = m_serialization.Deserialize (payloadData);

				} else if (payloadType == PayloadType.Bytes) {

					payload = payloadData;

				} else {

					throw new NotImplementedException ($"Packaging of payload type: {payloadType} not yet implemented.");

				}

			}

			return new TCPPackage (
				m_serialization.Encoding.GetString (path),
				m_serialization.Deserialize (headers),
				payload,
				(WebStatusCode) code);

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

