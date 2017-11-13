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

namespace Core.Network
{
	/// <summary>
	/// Implementations handles the serialization & deserialization of TCPMessages.
	/// </summary>
	public interface ITCPPackageFactory<T> where T: INetworkMessage
	{
		/// <summary>
		/// Serializes the TCPMessage into a byte array.
		/// </summary>
		/// <returns>The package.</returns>
		/// <param name="package">Package.</param>
		byte[] SerializeMessage (T message);

		/// <summary>
		/// Deserializes the package retrieved from the stream into a TCPMessage.
		/// </summary>
		/// <returns>The package.</returns>
		/// <param name="stream">Stream.</param>
		T DeserializePackage (System.IO.Stream stream);

	}
}

