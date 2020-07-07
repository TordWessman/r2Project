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
using System.IO;
using System.Runtime;
using System.Linq;

namespace R2Core.Network
{
	
    /// <summary>
    /// Default serializer/deserializer for TCPPackages
    /// </summary>
	public class TCPPackageFactory : ITCPPackageFactory<TCPMessage> {
		
		/// <summary>
		/// Defines the data types which are transmittable
		/// </summary>
		public enum PayloadType : int {

			// No payload
			None = 0,

			// Complex object
			Dynamic = 1,

			//CLR string
			String = 2,

			// Raw byte array
			Bytes = 3

		}

		private ISerialization m_serialization;

        /// <summary>
        /// Used to identify TCP packages (and filter out unexpected traffic).
        /// </summary>
        public static byte[] Signature = new byte[] { 42, 0, 42, 0 };

		public TCPPackageFactory(ISerialization serialization) {

			m_serialization = serialization;

		}

		/// <summary>
		/// Gets the PayloadType of the message.Payload.
		/// </summary>
		/// <returns>The payload type.</returns>
		/// <param name="message">Message.</param>
		public static PayloadType GetPayloadType(INetworkMessage message) {

		    if (!(message.Payload is null)) {

				return IdentifyPayloadType((object)message.Payload);

            }

		    return PayloadType.None;

		}

		public byte[] SerializeMessage(TCPMessage message) {
		
			byte[] code = new Int32Converter(message.Code).GetContainedBytes(2);
			byte[] path = m_serialization.Encoding.GetBytes(message.Destination ?? "");
			byte[] headerData = message.Headers != null ? m_serialization.Serialize(message.Headers) : new byte[0];
			byte[] payloadData = new byte[0];
			byte[] payloadDataType = new byte[0];

			PayloadType payloadType = GetPayloadType(message);

			if (payloadType == PayloadType.Bytes) { payloadData = message.Payload as byte[]; }
			else if (payloadType == PayloadType.String) { payloadData = m_serialization.Encoding.GetBytes(message.Payload); }
			else if (payloadType == PayloadType.Dynamic) { payloadData =  m_serialization.Serialize(message.Payload); }

			payloadDataType = new Int32Converter((int)payloadType).GetContainedBytes(2);

			byte[] pathSize = new Int32Converter(path.Length).Bytes; 
			byte[] payloadSize = new Int32Converter(payloadData.Length).Bytes;
			byte[] headerSize = new Int32Converter(headerData.Length).Bytes;

			return CreateRawPackage(Signature, code, pathSize, headerSize, payloadSize, path, headerData, payloadDataType, payloadData);
		
		}

		public dynamic DeserializePayload(TCPMessage message) {
		
			if (message.PayloadType == PayloadType.Dynamic) {
			
				return m_serialization.Deserialize(message.Payload);
					
			} else if (message.PayloadType == PayloadType.String) {
			
				return m_serialization.Encoding.GetString(message.Payload);

			}

			return message.Payload;

		}

		public TCPMessage DeserializePackage(Stream stream) {

            byte[] signature = stream.Read(Signature.Length);
            for (int i = 0; i < Signature.Length; i++) {

                if (signature[i] != Signature[i]) {

                    return default(TCPMessage);

                }

            }

            int code = stream.ReadInt(2);
			int destinationSize = stream.ReadInt();
			int headerSize = stream.ReadInt();
			int payloadSize = stream.ReadInt();
			byte[] destination = destinationSize > 0 ? stream.Read(destinationSize) : new byte[0];
			byte[] headers = headerSize > 0 ? stream.Read(headerSize) : new byte[0];
			PayloadType payloadType = (PayloadType)stream.ReadInt(2);
			byte[] payloadData = stream.Read(payloadSize);

			dynamic payload;

			if (payloadType ==  PayloadType.Bytes) {

				payload = payloadData;

			} else if (payloadType == PayloadType.String) {

				payload = m_serialization.Encoding.GetString(payloadData);

			} else {

				payload = m_serialization.Deserialize(payloadData);

			}

			return new TCPMessage { 
				Destination = m_serialization.Encoding.GetString(destination),
				Headers = m_serialization.Deserialize(headers),
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
				
					Array.Copy(dataset, 0, data, position, dataset.Length);
					position += dataset.Length;

				}

			}

			return data;

		}

		private static PayloadType IdentifyPayloadType(object payload) {
			if (payload is byte[]) {

				return PayloadType.Bytes;

			} else if (payload is string) {

				return PayloadType.String;

			}

			return PayloadType.Dynamic;

		}

	}

}

