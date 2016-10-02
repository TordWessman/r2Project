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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Drawing;

/// <summary>
/// Data package factory.
/// You have to include all data types this package factory should be able to serialize/deserialize.
/// </summary>


namespace Core.Network.Data
{
	public abstract class DataPackageFactory : IDataPackageFactory
	{

		protected INetworkSecurity m_security;

		protected const int HEADER_SIZE_POSITION = 2;
		
		protected RawPackageFactory m_packageFactory;
		
		public DataPackageFactory (INetworkSecurity security)
		{

			m_packageFactory = new RawPackageFactory ();
			m_security = security;

		}

		#region IPackageFactory implementation
		
		public IDataPackage CreateEmptyPackage (DataPackageType type, IDictionary<string,string> fields = null)
		{
			IDataPackageHeader header = new DataPackageHeader (
				fields != null ? fields :
				new Dictionary<string, string> ()
			                                                                    );
			return new DataPackage (header, type);
		}
		
		protected byte[] CreateHeader (IDataPackageHeader header, DataPackageType packageType)
		{
			
			byte [] serializedHeader = SerializeHeader (header);

			byte [] rawPackage = new byte[
			    sizeof(byte) + // packageType
				serializedHeader.Length 
			                              ];
			
			//Set the packagu type:
			rawPackage [0] = (byte)packageType;
		
			System.Array.Copy (serializedHeader, 0, rawPackage, sizeof(byte), serializedHeader.Length);
			
			return rawPackage;
		}
		
		public byte [] Serialize (IDataPackage sourcePackage)
		{
			return m_packageFactory.CreateDataPackage(
				CreateHeader (sourcePackage.GetHeader (), sourcePackage.GetType ()));
		}

		public byte [] Serialize<T> (IDataPackage<T> sourcePackage)
		{
			byte [] serializedValue = SerializeValue<T> (sourcePackage.Value);
			byte [] serializedHeader = CreateHeader (sourcePackage.GetHeader (), sourcePackage.GetType ());
			byte [] rawPackage = new byte[serializedHeader.Length + serializedValue.Length];
			
			System.Array.Copy (
				serializedHeader,
				0,
				rawPackage,
				0, 
				serializedHeader.Length);
			
			System.Array.Copy (
				serializedValue,
				0,
				rawPackage,
				serializedHeader.Length, 
				serializedValue.Length);

			return m_packageFactory.CreateDataPackage (
				rawPackage);
		}
		
		public int GetHeaderSize (byte[]rawPackage)
		{
			/*
			using (MemoryStream m = new MemoryStream(rawPackage)) {
				m.Seek (HEADER_SIZE_POSITION);
				return
			}*/
			return BitConverter.ToInt32 (rawPackage, HEADER_SIZE_POSITION);

		}
		
		/**
		 * 0 [1] base type
		 * 1 [1] sub type
		 * 2 headers
		 * 		2 [4] header size
		 * 		6 [1] headers count
		 * 		7 [..] header fields
		 * 
		 * */
		public IDataPackageHeader UnserializeHeader (byte[]rawPackage)
		{

			if (rawPackage == null) {

				throw new NullReferenceException ("Package cannot be null");
			
			}
			
			IDictionary<string,string> keyValues = new Dictionary<string,string> ();
			
			using (MemoryStream m = new MemoryStream(rawPackage)) {
	
				m.Seek(
					  sizeof(byte) //base type 
					+ sizeof(byte) //sub type
					+ sizeof (int), //total header size
					SeekOrigin.Begin);
				
				int keyCount = m.ReadByte ();

				using (BinaryReader reader = new BinaryReader(m)) {
					
					for (int i = 0; i < keyCount; i++) {

						string key = reader.ReadString ();
						string value = reader.ReadString ();
						                  
						keyValues.Add (key, value);

					}
					
					m.Close ();
					return new DataPackageHeader (keyValues);

				}
			}

		}

		/**
		 * 
		 * Return byte[] containing 
		 * 0 [4] headerSize
		 * 4 [1] keycount
		 * 5 [...] header fields
		 * 
		 * */
		protected byte [] SerializeHeader (IDataPackageHeader header)
		{
			using (MemoryStream m = new MemoryStream()) {
				using (BinaryWriter writer = new BinaryWriter(m)) {
					
					if (header.GetKeys () != null) {
						writer.Write ((byte)header.GetKeys ().Count);
						
						foreach (string key in header.GetKeys()) {
							writer.Write (key);
							writer.Write (header.GetValue (key));
						}
					} else {
						writer.Write ((byte)0);
					}
				}
				byte [] headerData = m.ToArray ();
				
				int totalSize = headerData.Length
					+ sizeof(int);
					//+ TYPE_HEADER_SIZE;
				
				byte [] headerSizeData = BitConverter.GetBytes (totalSize);

				byte [] returnData = new byte [headerData.Length + sizeof(int)];
				
				System.Array.Copy (headerSizeData, returnData, headerSizeData.Length);
				System.Array.Copy (headerData, 0, returnData, sizeof(int), headerData.Length);
				
				
				return returnData;
				
				
			}
		}
		
		protected byte [] SerializeValue <T> (T value)
		{
			using (MemoryStream stream = new MemoryStream ()) {
				BinaryFormatter bf = new BinaryFormatter ();
				bf.Serialize (stream, value);
				
				return stream.ToArray ();
			}
			
		}
			            
		public T Unserialize<T> (byte[]rawPackage)
		{
			
			if (rawPackage == null) {

				throw new NullReferenceException ("Package cannot be null");
			
			}

			IDataPackageHeader header = UnserializeHeader (rawPackage);

			if (header.GetValue(HeaderFields.Checksum.ToString()) != m_security.Token) {
			
				throw new AccessViolationException("Invalid security token.");

			}
			
			long headerPosition = GetHeaderSize (rawPackage);

			using (MemoryStream m = new MemoryStream(rawPackage)) {
				
				m.Seek (headerPosition + HEADER_SIZE_POSITION, 
				        SeekOrigin.Begin);
				
				BinaryFormatter bf = new BinaryFormatter ();
				
				return (T)bf.Deserialize (m);

			}

		}
		
		#endregion

	}

}