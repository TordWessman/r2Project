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
using System.IO;


namespace Core.Network
{
	
	public enum PackageFactoryDefaults {

		BadChecksum = 1,
		NoReceiver = 2,
		BadLength = 3,
		GoodBye = 4,
		Error = 5,
		NoKnownResponse = 8,
		DataReceived = 9 // <- Generic answer if (nobody will probably listen to this)

	}
	
	public enum PackageTypes {

		Unknown = 0,
		DefaultPackage = 1,
		DataPackage = 42,
		ClientPackage = 43
		
	}
	
	public class RawPackageFactory
	{

		public const int PACKAGE_TYPE_POSITION = 0;
		public const int PACKAGE_SUB_TYPE_POSITION = 1;
		public const int PACKAGE_HEADER_SIZE = 2;
		
		public RawPackageFactory ()
		{
		}
		
		public byte[] CreateClientPackage (string message, byte header = 0)
		{

			byte [] binaryMessage = null;
			
			if (message != null) {
			
				using (MemoryStream m = new MemoryStream()) {
				
					using (BinaryWriter writer = new BinaryWriter(m)) {
					
						writer.Write (message);
					
					}
					
					binaryMessage = m.ToArray ();
				
				}
			
			}
			
			byte [] result = new byte[3 + binaryMessage.Length ];
			result [PACKAGE_TYPE_POSITION] = (byte)PackageTypes.ClientPackage;
			result [2] = header;
			
			if (message != null) {
			
				Array.Copy (binaryMessage, 0, result, 3, binaryMessage.Length);
			
			}
			
			return result;
		
		}
		
		public byte[] CreateClientPackage (byte[] data, byte header)
		{
		
			byte [] result = new byte[3 + data.Length ];
			result [PACKAGE_TYPE_POSITION] = (byte)PackageTypes.ClientPackage;
			result [1] = 1;
			result [2] = header;
			
			Array.Copy (data, 0, result, 3, data.Length);
			
			return result;

		}
		
		public byte[] CreateDataPackage (byte[] output)
		{

			byte [] result = new byte[output.Length + 1];
			result [PACKAGE_TYPE_POSITION] = (byte)PackageTypes.DataPackage;
			System.Array.Copy (output, 0, result, sizeof(byte), output.Length);
			
			return result;

		}
		
		public byte[] CreateDefaultPackage (PackageFactoryDefaults type, string message = null)
		{
			
			byte [] binaryMessage = null;
			
			if (message != null) {

				using (MemoryStream m = new MemoryStream()) {

					using (BinaryWriter writer = new BinaryWriter(m)) {

						writer.Write (message);
					
					}
					
					binaryMessage = m.ToArray ();
				
				}
			
			}
			
			byte [] result = new byte[message == null ? PACKAGE_HEADER_SIZE : PACKAGE_HEADER_SIZE + binaryMessage.Length ];
			result [PACKAGE_TYPE_POSITION] = (byte)PackageTypes.DefaultPackage;
			result [PACKAGE_SUB_TYPE_POSITION] = (byte)type;
			
			if (message != null) {

				Array.Copy (binaryMessage, 0, result, PACKAGE_HEADER_SIZE, binaryMessage.Length);
			
			}
			
			return result;
		
		}
	
	}

}

