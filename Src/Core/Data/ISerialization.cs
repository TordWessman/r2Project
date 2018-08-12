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
using R2Core.Device;

namespace R2Core.Data
{


	/// <summary>
	/// Capable of serializing from any type and deserializing objects into a dynamic type.
	/// </summary>
	public interface ISerialization: IDevice
	{

		/// <summary>
		/// Serialize the data provided to byte array representation.
		/// </summary>
		/// <param name="data">Data.</param>
		byte[] Serialize (dynamic data);

		/// <summary>
		/// Generates a dynamic object from the serialized data container. 
		/// </summary>
		/// <param name="data">Data.</param>
		dynamic Deserialize (byte[] data);

		/// <summary>
		/// The encoding used by the deserializer.
		/// </summary>
		/// <value>The encoding.</value>
		System.Text.Encoding Encoding { get; }

	}
}

