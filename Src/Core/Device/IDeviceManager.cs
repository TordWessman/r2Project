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
using System.Collections.Generic;


namespace R2Core.Device
{

	public interface IDeviceContainer {
	
		/// <summary>
		/// Return all devices(local & remote)
		/// </summary>
		/// <value>The devices.</value>
		IEnumerable<IDevice> Devices { get; }

		/// <summary>
		/// Return all local devices
		/// </summary>
		/// <value>The devices.</value>
		IEnumerable<IDevice> LocalDevices { get; }

	}

	/// <summary>
	/// A devcie manager is respnsible for keepin track of all available IDevices
	/// </summary>
	public interface IDeviceManager : IDevice, IDeviceObserver, IDeviceContainer {

		/// <summary>
		/// Add a ´IDevice´ to the container.
		/// </summary>
		/// <param name="device">Device.</param>
		void Add(IDevice device);

		/// <summary>
		/// Returns a device of type T with the specified identifier if found or null. Will throw exception if unable to cast. 
		/// </summary>
		/// <param name="identifier">Identifier.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		T Get<T>(string identifier);

		/// <summary>
		/// Returns a device with the specified identifier or null.
		/// </summary>
		/// <param name="identifier">Identifier.</param>
		dynamic Get(string identifier);

		/// <summary>
		/// Returns a device with the specified Guid.
		/// </summary>
		/// <returns>The by GUID.</returns>
		/// <param name="guid">GUID.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		T GetByGuid<T>(Guid guid);

		/// <summary>
		/// Returns true if a device with the specified identifier exists
		/// </summary>
		/// <returns><c>true</c> if this instance has identifier; otherwise, <c>false</c>.</returns>
		/// <param name="identifier">Identifier.</param>
		bool Has(string identifier);

		/// <summary>
		/// Removes an IDevice if it exists.
		/// </summary>
		/// <param name="device">Device to be removed.</param>
		void Remove(IDevice device);

        /// <summary>
        /// Removes any device with the specified Identifier.
        /// </summary>
        /// <param name="id">Identifier.</param>
        void Remove(string id);

        /// <summary>
        /// Adds an IDeviceManagerObserver which will be notified if a device is added or removed.
        /// </summary>
        /// <param name="observer">Observer.</param>
        void AddObserver(IDeviceManagerObserver observer);

		/// <summary>
		/// Sends stop signal to all local devices. Use optional ignoreDevice in order to exclude devices from being stopped
		/// </summary>
		void Stop(IDevice[] ignoreDevices);

	}

}

