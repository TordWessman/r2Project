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

namespace R2Core.GPIO
{
	/// <summary>
	/// Represents a device available through the serial bus(i.e. I2C or Serial)
	/// </summary>
	public interface ISerialDevice : IDevice {

		/// <summary>
		/// (Re)creates the device representation on the node, typically after node reboot.
		/// </summary>
		void Synchronize();

		/// <summary>
		/// Allows the device to retrieve it's value from the node. 
		/// </summary>
		void Update();

		/// <summary>
		/// The node to which this device belongs
		/// </summary>
		/// <value>The node.</value>
		ISerialNode Node { get; }

		/// <summary>
		/// If the node to which this device is attached is in sleep mode.
		/// </summary>
		/// <value><c>true</c> if this instance is sleeping; otherwise, <c>false</c>.</value>
		bool IsSleeping { get; }

		/// <summary>
		/// Used for verifying the integrity of a device
		/// </summary>
		/// <value>The checksum.</value>
		byte Checksum { get; }

	}

}