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

        /// <summary>
        /// Reads the entire stream.
        /// http://stackoverflow.com/questions/1080442/how-to-convert-an-stream-into-a-byte-in-c
        /// </summary>
        /// <returns>The to end.</returns>
        /// <param name="stream">Stream.</param>
        public static byte[] ReadToEnd(this Stream stream) {

            long originalPosition = 0;

            if (stream.CanSeek) {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0) {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length) {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1) {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead) {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            } finally {
                if (stream.CanSeek) {
                    stream.Position = originalPosition;
                }
            }
        }

    }

}

