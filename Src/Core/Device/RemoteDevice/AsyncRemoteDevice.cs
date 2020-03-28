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
using System.Dynamic;
using System.Threading.Tasks;

namespace R2Core.Device
{

	/// <summary>
	/// Provides mechanisms for dynamic asynchronous invocation.
	/// </summary>
	public class AsyncRemoteDevice : DynamicObject {

		private RemoteDevice m_device;
		private Action<dynamic, Exception> m_callback;

		/// <summary>
		/// An instance of a <see cref="R2Core.Device.AsyncRemoteDevice"/> is intended
		/// to be used for (an) asynchronous invocation. Get and Invoke
		/// will return the Task performing the invocation.
		/// </summary>
		/// <param name="callback">Callback.</param>
		/// <param name="remoteDevice">Remote device.</param>
		public AsyncRemoteDevice(Action<dynamic, Exception> callback, RemoteDevice remoteDevice) {

			m_device = remoteDevice;
			m_callback = callback;

		}

		public override bool TrySetMember(SetMemberBinder binder, object value) {

			AsyncRemoteDeviceRequest request = new AsyncRemoteDeviceRequest(m_callback, m_device, binder, value);
			request.Set();

			return true;

		}

		public override bool TryGetMember(GetMemberBinder binder, out object result) {

			AsyncRemoteDeviceRequest request = new AsyncRemoteDeviceRequest(m_callback, m_device, binder);
			result = request.Get();

			return true;

		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {

			AsyncRemoteDeviceRequest request = new AsyncRemoteDeviceRequest(m_callback, m_device, binder, args); 
			result = request.Invoke();

			return true;

		}

		/// <summary>
		/// Gets a remote value with the name ´propertyName´. callback's value will contain the value of ´propertyName´ on success 
		/// or null upon failure.
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="propertyName">Property name.</param>
		public Task GetValue(string propertyName) {
		
			AsyncRemoteDeviceRequest request = new AsyncRemoteDeviceRequest (propertyName, m_callback, m_device);
			return request.Get();

		}

	}

}

