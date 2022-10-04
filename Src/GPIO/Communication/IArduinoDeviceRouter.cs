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
using R2Core.Device;

namespace R2Core.GPIO
{

	public struct DeviceData<T> {
	
		public byte Id;
		public T Value;

	}

	/// <summary>
	/// A hight level interface for interaction to a serial host exposing the available interactions. 
	/// </summary>
	public interface IArduinoDeviceRouter : IDevice {

		/// <summary>
		/// Number of retries for a transmission before giving up and rethrowing IO & Serial exceptions.
		/// </summary>
		/// <value>The retry count.</value>
		int RetryCount { get; set; }

		/// <summary>
		/// Returns the value property of a device on node with id `nodeId`. `deviceId` is the id(local on the node) of the device.
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="nodeId">Node identifier.</param>
		DeviceData<T> GetValue<T>(byte deviceId, int nodeId);

		/// <summary>
		/// Sets the value of an object. `deviceId` is the id of the device located on the node with id `nodeId`. 
		/// </summary>
		/// <param name="deviceId">Device identifier.</param>
		/// <param name="nodeId">Id of the node.</param>
		/// <param name="value">Value.</param>
		void Set(byte deviceId, int nodeId, int value);

		/// <summary>
		/// Creates a device on node with id `nodeId` of type `type` using `parameters` as arguments. Returns the remote id and the values of the new device.
		/// </summary>
		/// <param name="nodeId">Host identifier.</param>
		/// <param name="type">Type.</param>
		/// <param name="parameters">Parameters.</param>
		DeviceData<T> Create<T>(int nodeId, SerialDeviceType type, byte[] parameters);

        /// <summary>
        /// Deletes a remote  device
        /// </summary>
        /// <param name="deviceId">Device identifier.</param>
        /// <param name="nodeId">Node identifier.</param>
        void DeleteDevice(byte deviceId, int nodeId);

        /// <summary>
        /// Initializes(resets) the specified host.
        /// </summary>
        /// <param name="nodeId">Host.</param>
        void Initialize(int nodeId);

		/// <summary>
		/// Resets the node. Set state to uninitialized and remove all devices.
		/// </summary>
		/// <param name="nodeId">Node identifier.</param>
		void Reset(int nodeId);

		/// <summary>
		/// Returns true if the specified node is currently registered at the master (not to be confused with sleep mode).
		/// </summary>
		/// <returns><c>true</c> if this instance is node available the specified host; otherwise, <c>false</c>.</returns>
		/// <param name="nodeId">Host.</param>
		bool IsNodeAvailable(int nodeId);

		/// <summary>
		/// Returns true if the node has been sent to sleep
		/// </summary>
		/// <returns><c>true</c> if this instance is node sleeping the specified nodeId; otherwise, <c>false</c>.</returns>
		/// <param name="nodeId">Node identifier.</param>
		bool IsNodeSleeping(int nodeId);

		/// <summary>
		/// Retrieves the nodes connected to the host. The number might be limited to response content size.
		/// </summary>
		/// <returns>The nodes.</returns>
		byte[] GetNodes();

		/// <summary>
		/// Sets a node to sleep mode. Sleep the specified nodeId for `cycles` (1 cycle equals ~8 seconds).
		/// </summary>
		void Sleep(int nodeId, bool toggle, int cycles = ArduinoSerialPackageFactory.RH24_SLEEP_UNTIL_MESSAGE_RECEIVED);

		/// <summary>
		/// Allows the node to stay awake for up to 60 seconds. Use this method if multiple actions are required(i.e. synchronization and value updates) in order to lower the writing to the nodes EEPROM.
		/// </summary>
		/// <param name="nodeId">Node identifier.</param>
		/// <param name="seconds">Seconds.</param>
		void PauseSleep(int nodeId, int seconds);

		/// <summary>
		/// Permanently name this node `nodeId`. This method is not propagated.
		/// </summary>
		/// <param name="nodeId">Node identifier.</param>
		void SetNodeId(int nodeId);

		/// <summary>
		/// Returns the checksum calculated by the node with id ´nodeId´.
		/// </summary>
		/// <returns>The checksum.</returns>
		/// <param name="nodeId">Node identifier.</param>
		byte[] GetChecksum(int nodeId);

		/// <summary>
		/// Delegate called whenever a host replied that it will be reset. The argument is the host name.
		/// </summary>
		Action<byte> HostDidReset { get; set; }

        /// <summary>
        /// Turns on or off the microcontrollers ability to power on or off a Raspberry Pi (or similar).
        /// See the README.md in the arduino device router project for details.
        /// </summary>
        /// <param name="on">If set to <c>true</c> on.</param>
        void SetRPiPowerController(bool on = true);


    }

}
