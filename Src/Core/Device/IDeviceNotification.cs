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

namespace R2Core.Device
{
	/// <summary>
	/// A notification about a change in a device
	/// </summary>
	public interface IDeviceNotification<T> {
		
		/// <summary>
		/// The object type contained.
		/// </summary>
		/// <value>The type.</value>
		Type Type { get; }

		/// <summary>
		/// The identifier of the device changed.
		/// </summary>
		/// <value>The identifier.</value>
		string Identifier { get; }

		/// <summary>
		/// The called method or property changed.
		/// </summary>
		/// <value>The name of the method.</value>
		string Action { get; }

		/// <summary>
		/// The new value of the property or the resulting value of the method called.
		/// </summary>
		/// <value>The new value.</value>
		T NewValue { get; }
	}

}