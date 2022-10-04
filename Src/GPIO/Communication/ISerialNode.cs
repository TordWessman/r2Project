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
using R2Core.Device;

namespace R2Core.GPIO
{

    /// <summary>
    /// A representation of a "remote node". A remote node is a hardware device with it's own set of periphials. 
    /// Implementations of this interface, like `SerialNode` will contain a list of ISerialDevice object that 
    /// represent each i/o periphial connected to that node and it should act as a gateway for the connected devices.
    /// </summary>
    public interface ISerialNode : IDevice {

		/// <summary>
		/// Will synchronize(re-create) each device tracked by this node.
		/// </summary>
		void Synchronize();

		/// <summary>
		/// Creates the device on remote node. If sleep mode will be enabled, always use this method to synchronize remote devices. Adds a device to this node representations tracking list. This allows devices connected to the node to periodically cache their values.
		/// </summary>
		/// <param name="device">Device.</param>
		void Track(ISerialDevice device);

        /// <summary>
        /// Sends this node to sleep. This is a power saving feature that - if implemented on the remote node - allows it to stay in
        /// a low-power sleep mode for some time. A node that is sleeping will return the latest cached values of it's devices.
        /// </summary>
        /// <value><c>true</c> if sleep; otherwise, <c>false</c>.</value>
        bool Sleep { get; set; }

		/// <summary>
		/// The node id of this node representation
		/// </summary>
		/// <value>The node identifier.</value>
		byte NodeId { get; }

        /// <summary>
        /// If the node should perpetually be trying to fetch the values (Update) it's associated devices. Defaults to true. This
        /// is usefull if the node is sleeping, since it will require less frequent update. A node that is sleeping will return
        /// the latest cached values of it's devices.
        /// </summary>
        /// <value><c>true</c> if should update; otherwise, <c>false</c>.</value>
        bool ContinousSynchronization { get; set; }

        /// <summary>
        /// Removes a device from the nodes internal list of devices.
        /// </summary>
        /// <param name="device">Device.</param>
        void RemoveDevice(ISerialDevice device);

    }

}

