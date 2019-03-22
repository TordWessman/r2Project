using System;
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

