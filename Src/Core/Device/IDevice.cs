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

namespace R2Core.Device
{
	/// <summary>
	/// Implementations are required to be able to inform observers upon mutation.
	/// </summary>
	public interface IDeviceObservable {

		void AddObserver (IDeviceObserver observer);
        void RemoveObserver(IDeviceObserver observer);

    }

	public interface IDevice : IStartable, IStopable, IDeviceObservable
	{
		/// <summary>
		/// Textual representation of the device. Used as key for retrieving the device from a device manager.
		/// </summary>
		/// <value>The identifier.</value>
		string Identifier { get; }

		/// <summary>
		/// Returns true if the IDevice is ready for any proactive actions.
		/// </summary>
		/// <value><c>true</c> if ready; otherwise, <c>false</c>.</value>
		bool Ready { get; }	

		/// <summary>
		/// A unique objectual representation.
		/// </summary>
		/// <value>The checksum.</value>
		Guid Guid { get; }

	}

}

