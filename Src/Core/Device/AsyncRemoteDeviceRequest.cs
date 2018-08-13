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
	public class AsyncDeviceRetriever {

		private RemoteDevice m_device;
		private Action<dynamic> m_callback;
		private GetMemberBinder m_getBinder;
		private InvokeMemberBinder m_invokeBinder;
		private object[] m_invokeArgs;

		public AsyncDeviceRetriever(Action<dynamic> callback, RemoteDevice remoteDevice, GetMemberBinder binder)
		{

			m_device = remoteDevice;
			m_callback = callback;
			m_getBinder = binder;

		}

		public AsyncDeviceRetriever(Action<dynamic> callback, RemoteDevice remoteDevice, InvokeMemberBinder binder, object[] args)
		{

			m_device = remoteDevice;
			m_callback = callback;
			m_invokeBinder = binder;
			m_invokeArgs = args;

		}

		public Task Get() {
		
			return Task.Factory.StartNew (() => {

				object asyncResult = default(dynamic);
				m_device.TryGetMember (m_getBinder, out asyncResult);
				m_callback(asyncResult);

			});

		}

		public Task Invoke() {

			return Task.Factory.StartNew (() => {
				
				object asyncResult = default(dynamic);
				m_device.TryInvokeMember (m_invokeBinder, m_invokeArgs, out asyncResult);
				m_callback(asyncResult);

			});

		}

	}

	public class AsyncRemoteDeviceRequest: DynamicObject
	{
		private RemoteDevice m_device;
		private Action<dynamic> m_callback;

		public AsyncRemoteDeviceRequest (Action<dynamic> callback, RemoteDevice remoteDevice)
		{
		
			m_device = remoteDevice;
			m_callback = callback;

		}

		public override bool TrySetMember (SetMemberBinder binder, object value) {

			Task.Factory.StartNew( () => {
			
				m_device.TrySetMember (binder, value);
				m_callback(true);

			});

			return true;

		}

		public override bool TryGetMember (GetMemberBinder binder, out object result) {

			result = new AsyncDeviceRetriever (m_callback, m_device, binder);

			return true;

		}

		public override bool TryInvokeMember (InvokeMemberBinder binder, object[] args, out object result) {
			
			result = new AsyncDeviceRetriever (m_callback, m_device, binder, args);

			return true;

		}

	}


}

