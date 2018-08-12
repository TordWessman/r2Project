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
	/// A representation of a remote node
	/// </summary>
	public interface ISerialNode : IDevice {

		/// <summary>
		/// Will synchronize (re-create) each device tracked by this node.
		/// </summary>
		void Synchronize ();

		/// <summary>
		/// Creates the device on remote node. If sleep mode will be enabled, always use this method to synchronize remote devices. Adds a device to this node representations tracking list. This allows devices connected to the node to periodically cache their values.
		/// </summary>
		/// <param name="device">Device.</param>
		void Track (ISerialDevice device);

		/// <summary>
		/// Sends this node to sleep.
		/// </summary>
		/// <value><c>true</c> if sleep; otherwise, <c>false</c>.</value>
		bool Sleep { get; set; }

		/// <summary>
		/// The node id of this node representation
		/// </summary>
		/// <value>The node identifier.</value>
		byte NodeId { get; }

	}

}

