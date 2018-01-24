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

namespace GPIO
{
	/// <summary>
	/// Creates device packages used for serial communication. Implementations are tightly coupled to the target platform.
	/// </summary>
	public interface ISerialPackageFactory
	{

		/// <summary>
		/// Creates a "Create device package". ports-requirements must be applicable to the requirements of the device.  
		/// </summary>
		/// <returns>The device.</returns>
		/// <param name="type">Type.</param>
		/// <param name="port">Port.</param>
		DeviceRequestPackage CreateDevice(byte hostId, SerialDeviceType type, byte[] ports);

		/// <summary>
		/// Creates a "Set device package" used to set the value of a device (previously created through a CreateDevice package call).
		/// </summary>
		/// <returns>The device.</returns>
		/// <param name="remoteDeviceId">Remote device identifier.</param>
		/// <param name="value">Value.</param>
		DeviceRequestPackage SetDevice(byte deviceId, byte hostId, int value);

		/// <summary>
		/// Used for returning the value of a device.
		/// </summary>
		/// <returns>The device.</returns>
		/// <param name="remoteDeviceId">Remote device identifier.</param>
		DeviceRequestPackage GetDevice(byte deviceId, byte hostId);

		/// <summary>
		/// Initializes (resets) the host and prepare it for new devices (and clearing any configuration of the slave). `host` is the address of the host to reset.
		/// </summary>
		/// <param name="host">Host.</param>
		DeviceRequestPackage Initialize (byte hostId);

		/// <summary>
		/// Parses raw response into a response package.
		/// </summary>
		/// <returns>The response.</returns>
		/// <param name="response">Response.</param>
		DeviceResponsePackage ParseResponse (byte[] response);

	}

}