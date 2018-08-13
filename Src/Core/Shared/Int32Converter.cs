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
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;

namespace R2Core
{

	public static class IntConversionExtensions {
	
		public static byte[]ToBytes(this int self, int size = Int32Converter.ValueSize) {
		
			return new Int32Converter(self).GetContainedBytes (size);

		}

	}

	public static class ByteArrayConversionExtension {
	
		public static int ToInt(this byte[] self, int offset = 0, int length = Int32Converter.ValueSize) {
		
			return new Int32Converter (self.Skip (offset).Take (length)).Value;

		}

	}

	/// <summary>
	/// Conversion between int values and byte arrays. Original code by Christ Taylor (http://stackoverflow.com/users/314028/chris-taylor)
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct Int32Converter
	{
		
		public const int ValueSize = 4;

		[FieldOffset(0)] public int Value;
		[FieldOffset(0)] public byte Byte1;
		[FieldOffset(1)] public byte Byte2;
		[FieldOffset(2)] public byte Byte3;
		[FieldOffset(3)] public byte Byte4;

		[FieldOffset(4)]
		private int m_lenght;

		public Int32Converter(int value) {

			Byte1 = Byte2 = Byte3 = Byte4 = 0;
			Value = value;
			m_lenght = 0;

			for (int i = Int32Converter.ValueSize; i > 0; i--) {
			
				if (this [i - 1] > 0) {
				
					m_lenght = i;
					break;

				}

			}

		}

		public Int32Converter(IEnumerable<byte> bytes) {

			Value = 0;
			Byte1 = Byte2 = Byte3 = Byte4 = 0;

			m_lenght = 0;

			foreach (byte b in bytes) { this [m_lenght++] = b; }

		}

		public int Length { get { return m_lenght; } }

		//Returns all 4 bytes.
		public byte[] Bytes { get { return new byte[] { Byte1, Byte2, Byte3, Byte4 }; } }

		/// <summary>
		/// Returns only the bytes being used to contain the value.
		/// </summary>
		/// <returns>The bytes.</returns>
		public byte[] GetContainedBytes(int? size = null) {

			IList<byte> byteArray = new List<byte> ();

			for (int i = 0; i < (size ?? Length); i++) { byteArray.Add (this [i]); }

			return byteArray.ToArray ();

		}

		public byte this [int key] {
			
			get { 
			
				switch (key) {

				case 0:
					return Byte1;
				case 1:
					return Byte2;
				case 2:
					return Byte3;
				case 3:
					return Byte4;
				default:
					
					throw new IndexOutOfRangeException ();
				}
			}

			set { 

				switch (key) {

				case 0:
					Byte1 = value;
					break;
				case 1:
					Byte2 = value;
					break;
				case 2:
					Byte3 = value;
					break;
				case 3:
					Byte4 = value;
					break;
				default:

					throw new IndexOutOfRangeException ();
				}
			}
		}

		public static implicit operator Int32(Int32Converter value) {
			
			return value.Value;
		
		}

		public static implicit operator Int32Converter(int value) {
			
			return new Int32Converter(value);
		
		}

		public static implicit operator Int32Converter(byte[] bytes) {

			return new Int32Converter(bytes);

		}
	
	}

}