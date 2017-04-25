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
	public class TCPPackageFactory: DeviceBase
	{
		private IR2Serialization m_serialization;

		public TCPPackageFactory (string id, IR2Serialization serialization): base(id) {

			m_serialization = serialization;

		}

		public byte[] CreateData(TCPPackage package) {
		
			byte[] headerData = m_serialization.Serialize (package.Headers);
			byte[] bodyData = m_serialization.Serialize (package.Payload);
			byte[] path = m_serialization.Encoding.GetBytes (package.Path);
			 
			byte[] pathSize = new Int32Converter (path.Length).Bytes; 
			byte[] bodySize = new Int32Converter (bodyData.Length).Bytes;
			byte[] headerSize = new Int32Converter (headerData.Length).Bytes;

			return CreateRawPackage (pathSize, headerSize, bodySize, path, headerData, bodyData);
		
		}

		public TCPPackage CreatePackage(byte [] rawData) {
		
			int position = 0;
			int pathSize = new Int32Converter (rawData.Skip (position).Take (Int32Converter.ValueSize)).Value;
			position += Int32Converter.ValueSize;
			int headerSize = new Int32Converter (rawData.Skip (position).Take (Int32Converter.ValueSize)).Value;
			position += Int32Converter.ValueSize;
			int bodySize = new Int32Converter (rawData.Skip (position).Take (Int32Converter.ValueSize)).Value;
			position += Int32Converter.ValueSize;

			byte[] path = rawData.Skip (position).Take (pathSize).ToArray();
			position += pathSize;

			byte[] headers = rawData.Skip (position).Take (headerSize).ToArray();
			position += headerSize;

			byte[] payload = rawData.Skip (position).Take (bodySize).ToArray();

			return new TCPPackage (
				m_serialization.Encoding.GetString (path),
				m_serialization.Deserialize (headers),
				m_serialization.Deserialize (payload));

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

				Array.Copy (dataset, 0, data, position, dataset.Length);
				position += dataset.Length;

			}

			return data;

		}
	}

}

