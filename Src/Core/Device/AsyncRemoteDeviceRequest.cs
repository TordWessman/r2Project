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
	public class AsyncRemoteDeviceRequest : DynamicObject {

		private RemoteDevice m_device;
		private Action<dynamic, Exception> m_callback;

		/// <summary>
		/// An instance of a <see cref="R2Core.Device.AsyncRemoteDeviceRequest"/> is intended
		/// to be used for (an) asynchronous invocation. Get and Invoke
		/// will return the Task performing the invocation.
		/// </summary>
		/// <param name="callback">Callback.</param>
		/// <param name="remoteDevice">Remote device.</param>
		public AsyncRemoteDeviceRequest(Action<dynamic, Exception> callback, RemoteDevice remoteDevice) {

			m_device = remoteDevice;
			m_callback = callback;

		}

		public override bool TrySetMember(SetMemberBinder binder, object value) {

			AsyncRemoteDeviceRequestTask request = new AsyncRemoteDeviceRequestTask(m_callback, m_device, binder, value);
			request.Set();

			return true;

		}

		public override bool TryGetMember(GetMemberBinder binder, out object result) {

			AsyncRemoteDeviceRequestTask request = new AsyncRemoteDeviceRequestTask(m_callback, m_device, binder);
			result = request.Get();

			return true;

		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {

			AsyncRemoteDeviceRequestTask request = new AsyncRemoteDeviceRequestTask(m_callback, m_device, binder, args); 
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
		
			AsyncRemoteDeviceRequestTask request = new AsyncRemoteDeviceRequestTask (propertyName, m_callback, m_device);
			return request.Get();

		}

	}

	public class AsyncRemoteDeviceRequestTask {

		private RemoteDevice m_device;
		private Action<dynamic, Exception> m_callback;
		private GetMemberBinder m_getBinder;
		private InvokeMemberBinder m_invokeBinder;
		private object[] m_invokeArgs;
		private SetMemberBinder m_setBinder;
		private object m_setArg;
		private string m_propertyName;

		public AsyncRemoteDeviceRequestTask(string propertyName, Action<dynamic, Exception> callback, RemoteDevice remoteDevice) {
		
			m_propertyName = propertyName;
			m_callback = callback;
			m_device = remoteDevice;
		}

		public AsyncRemoteDeviceRequestTask(Action<dynamic, Exception> callback, RemoteDevice remoteDevice, SetMemberBinder binder, object arg) {

			m_device = remoteDevice;
			m_callback = callback;
			m_setBinder = binder;
			m_setArg = arg;

		}

		public AsyncRemoteDeviceRequestTask(Action<dynamic, Exception> callback, RemoteDevice remoteDevice, GetMemberBinder binder) {

			m_device = remoteDevice;
			m_callback = callback;
			m_getBinder = binder;

		}

		public AsyncRemoteDeviceRequestTask(Action<dynamic, Exception> callback, RemoteDevice remoteDevice, InvokeMemberBinder binder, object[] args) {

			m_device = remoteDevice;
			m_callback = callback;
			m_invokeBinder = binder;
			m_invokeArgs = args;

		}

		public Task Get() {
		
			return Task.Factory.StartNew(() => {

				object asyncResult = default(dynamic);

				try {

					if (m_getBinder != null) {
					
						m_device.TryGetMember(m_getBinder, out asyncResult);

					} else if (m_propertyName != null) {
					
						asyncResult = m_device.Get(m_propertyName);

					} else {
					
						throw new DeviceException("Unable to fetch remote value: no propertyName or binder set.");

					}

					m_callback(asyncResult, null);

				} catch (Exception ex) {

					m_callback(asyncResult, ex);

				}


			});

		}

		public Task Invoke() {

			return Task.Factory.StartNew(() => {
				
				object asyncResult = default(dynamic);

				try {

					m_device.TryInvokeMember(m_invokeBinder, m_invokeArgs, out asyncResult);
					m_callback(asyncResult, null);

				} catch (Exception ex) {

					m_callback(asyncResult, ex);

				}

			});

		}

		public Task Set() {
		
			return Task.Factory.StartNew(() => {

				try {

					m_device.TrySetMember(m_setBinder, m_setArg);
					m_callback(true, null);

				} catch (Exception ex) {

					m_callback(false, ex);

				}

			});
		}

	}

}

