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
using System.IO;

namespace R2Core
{
	
	internal static class StreamExtensions {

		/// <summary>
		/// Reads ´size´ bytes or throw an IOException.
		/// </summary>
		/// <returns>byte[] containing the read data.</returns>
		/// <param name="size">Size.</param>
		public static byte[] Read(this Stream self, int size) {

			byte[] buff = new byte[size];

			if (size != self.Read(buff, 0, size)) {

				throw new IOException($"Unable to read {size} bytes from stream");

			}

			return buff;

		}

		/// <summary>
		/// Reads an integer. If size is unspecified it defaults to a 4-byte integer (´Int32Converter.ValueSize´).
		/// </summary>
		/// <returns>An int.</returns>
		/// <param name="size">Size to read.</param>
		public static int ReadInt(this Stream self, int? size = null) {

			return new Int32Converter(Read(self, size ?? Int32Converter.ValueSize)).Value;

		}

	}

}

