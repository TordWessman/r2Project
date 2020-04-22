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

namespace R2Core
{
	public static class DeviceExtensions {

		/// <summary>
		/// Returns true if the device is not a remote device
		/// </summary>
		/// <returns><c>true</c> if is local the specified self; otherwise, <c>false</c>.</returns>
		/// <param name="self">Self.</param>
		public static bool IsLocal(this IDevice self) {

			return !(self is IRemoteDevice);

		}

	}

}

