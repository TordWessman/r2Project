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
	/// A notification indicatiing a change in a device.
	/// </summary>
	public class DeviceNotification : IDeviceNotification<object> {
		
		public DeviceNotification(string id, string action, object newValue = null) {
			
			Identifier = id;
			Action = action;
			NewValue = newValue;
		}

		public Type Type => NewValue?.GetType();

		/// <summary>
		/// The string identifier (IDevice.Identifier) of the object changed.
		/// </summary>
		/// <value>The identifier.</value>
		public string Identifier { get; private set; }

		/// <summary>
		/// The string representation of the method or property that has been changed.
		/// </summary>
		/// <value>The name of the method.</value>
		public string Action { get; private set; }

		/// <summary>
		/// The new value of the property or the return value of the method(should be null if it's a void method).
		/// </summary>
		/// <value>The new value.</value>
		public object NewValue { get; private set; }

	}

}

