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
using R2Core.Device;

namespace R2Core.GPIO {

	/// <summary>
	/// A device capable of serial communication. Currently I2C (using libr2I2C.so) or USB/Serial connection is implemented.
	/// </summary>
	public interface ISerialConnection : IDevice {

        /// <summary>
        /// Returns <c>true</c> if the connection should be up and running (<c>Ready</c> returns <c>true</c> only if it's connected, which might be unreliable.
        /// </summary>
        /// <value><c>true</c> if should runt; otherwise, <c>false</c>.</value>
        bool ShouldRun { get; }

        /// <summary>
        /// Will send the array of bytes to node and return the reply(if any)
        /// </summary>
        /// <param name="data">Data.</param>
        byte[] Send(byte[] data);

        /// <summary>
        /// Will try to read from the node.
        /// </summary>
        byte[] Read();

	}

}