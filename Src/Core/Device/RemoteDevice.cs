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
using R2Core.Network;

namespace R2Core.Device
{
	/// <summary>
	/// A RemoteDevice represents a device not present in this instance. It's heavily coupled with the message strategies of the `DeviceRouter`
	/// </summary>
	public class RemoteDevice : DynamicObject, IRemoteDevice
	{
		protected string m_identifier;
		protected IHostConnection m_host;
		protected Guid m_guid;

		public string Identifier { get { return m_identifier; } }

		public bool Ready { 

			get { 

				if (!m_host.Ready) { return false; }

				DeviceRequest request = new DeviceRequest () {
					Action = "Ready",
					Params = new object[] {},
					ActionType = DeviceRequest.ObjectActionType.Get,
					Identifier = m_identifier
				};

				return m_host.Send (request) == true;

			} 
		
		}

		public Guid Guid { get { return m_guid; } }

		public dynamic Async(Action<dynamic, Exception> callback) {

			return new AsyncRemoteDeviceRequest (callback, this);

		}

		public void Start() {

			DeviceRequest request = new DeviceRequest () {
				Action = "Start",
				Params = null,
				ActionType = DeviceRequest.ObjectActionType.Invoke,
				Identifier = m_identifier
			};

			m_host.Send (request);

		}

		public void Stop() {
		
			DeviceRequest request = new DeviceRequest () {
				Action = "Stop",
				Params = null,
				ActionType = DeviceRequest.ObjectActionType.Invoke,
				Identifier = m_identifier
			};

			m_host.Send (request);

		}

		public void AddObserver (IDeviceObserver observer) { throw new InvalidOperationException("RemoteDevice can't have observers"); }
        public void RemoveObserver(IDeviceObserver observer) { throw new InvalidOperationException("RemoteDevice can't have observers"); }

        public RemoteDevice (string id, Guid guid, IHostConnection host)
		{

			m_identifier = id;
			m_host = host;
			m_guid = guid;

		}

		public override bool TrySetMember (SetMemberBinder binder, object value) {

			DeviceRequest request = new DeviceRequest () {
				Action = binder.Name,
				Params = new object[] { value },
				ActionType = DeviceRequest.ObjectActionType.Set,
				Identifier = m_identifier
			};

			m_host.Send (request);
			return true;

		}

		public override bool TryGetMember (GetMemberBinder binder, out object result) {

			DeviceRequest request = new DeviceRequest () {
				Action = binder.Name,
				Params = new object[] {},
				ActionType = DeviceRequest.ObjectActionType.Get,
				Identifier = m_identifier
			};

			dynamic response = m_host.Send (request);

			result = response.ActionResponse;
			return true;

		}

		public override bool TryInvokeMember (InvokeMemberBinder binder, object[] args, out object result) {
			
			DeviceRequest request = new DeviceRequest () {
				Action = binder.Name,
				Params = args,
				ActionType = DeviceRequest.ObjectActionType.Invoke,
				Identifier = m_identifier
			};

			dynamic response = m_host.Send (request);

			result = response.ActionResponse;
			return true;

		}
		
	}

}