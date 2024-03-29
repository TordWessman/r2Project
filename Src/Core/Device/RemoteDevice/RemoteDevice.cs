﻿// This file is part of r2Poject.
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
using System.Collections.Generic;

namespace R2Core.Device
{
	/// <summary>
	/// A RemoteDevice represents a device not present in this instance. It's heavily coupled with the message strategies of the `DeviceRouter`
	/// </summary>
	public class RemoteDevice : DynamicObject, IRemoteDevice, IInvokable {
		
		protected string m_identifier;
		protected INetworkConnection m_host;
		protected Guid m_guid;
		private INetworkMessage m_message;
        private AsyncRemoteDeviceTask m_lastAsyncTask;

        public string Identifier { get { return m_identifier; } }
        public bool Busy => m_host.Busy;
        public Guid Guid { get { return m_guid; } }
        public INetworkConnection Connection { get { return m_host; } }

        public bool Ready { 

			get { 

				if (!m_host.Ready) { return false; }

				DeviceRequest request = new DeviceRequest() {
					Action = "Ready",
					Params = new object[] {},
					ActionType = DeviceRequest.ObjectActionType.Get,
					Identifier = m_identifier
				};

				m_message.Payload = request;

				var response = m_host.Send(m_message).Payload;

				if (response.ActionResponse is bool) { return response.ActionResponse; } 

				throw new DeviceException($"Unable to check ´Ready´ state for RemoteDevice '{Identifier}'. Response Payload was not bool, but {response}.");

			} 

		}

        /// <summary>
        /// If <c>true</c>, only the last AsyncRemoteDeviceRequest will
        /// be executed if there are ongoing requests.
        /// </summary>
        /// <value><c>true</c> if lossy; otherwise, <c>false</c>.</value>
        public bool LossyRequests { get; set; }

        /// <summary>
        /// Instantiate a remote device representation. ´destination´ is the path (i.e. "/destination" which the other end is listening to). 
        /// Set to null using default settings.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="guid">GUID.</param>
        /// <param name="host">Host.</param>
        /// <param name="destination">Destination.</param>
        public RemoteDevice(string id, Guid guid, INetworkConnection host, string destination = null) {

			m_identifier = id;
			m_host = host;
			m_guid = guid;
			m_message = new NetworkMessage() { 
				Headers = new Dictionary<string, object>(), 
				Destination = destination ?? Settings.Consts.DeviceDestination() };

		}

		public dynamic Async(Action<dynamic, Exception> callback) {

			return new AsyncRemoteDevice(callback, this);

		}

		public void Start() {

			DeviceRequest request = new DeviceRequest() {
				Action = "Start",
				Params = null,
				ActionType = DeviceRequest.ObjectActionType.Invoke,
				Identifier = m_identifier
			};

			m_message.Payload = request;

			m_host.Send(m_message);

		}

		public void Stop() {

			DeviceRequest request = new DeviceRequest() {
				Action = "Stop",
				Params = null,
				ActionType = DeviceRequest.ObjectActionType.Invoke,
				Identifier = m_identifier
			};

			m_message.Payload = request;
			m_host.Send(m_message);

		}

		public void AddObserver(IDeviceObserver observer) { throw new InvalidOperationException("RemoteDevice can't have observers"); }
		public void RemoveObserver(IDeviceObserver observer) { throw new InvalidOperationException("RemoteDevice can't have observers"); }

		public void Set(string handle, dynamic value) {

			DeviceRequest request = new DeviceRequest() {
				Action = handle,
				Params = new object[] { value },
				ActionType = DeviceRequest.ObjectActionType.Set,
				Identifier = m_identifier
			};

            INetworkMessage response = Send(request);

        }


        private INetworkMessage Send(DeviceRequest request) {

            m_message.Payload = request;
            INetworkMessage response = m_host.Send(m_message);

            if (m_lastAsyncTask?.Status != System.Threading.Tasks.TaskStatus.Running) {

                m_lastAsyncTask?.Start();
            
            }

            m_lastAsyncTask = null;

            if (response.IsError()) {

                string errorMessage = response.ErrorDescription();
                Log.i($"{this} got error: {errorMessage}");
                throw new DeviceException(errorMessage);

            }

            return response;

        }

        public dynamic Get(string handle) {

			DeviceRequest request = new DeviceRequest() {
				Action = handle,
				Params = new object[] {},
				ActionType = DeviceRequest.ObjectActionType.Get,
				Identifier = m_identifier
			};

			INetworkMessage response = Send(request);

            return response.Payload.ActionResponse;

		}

		public dynamic Invoke(string handle, params dynamic[] args) {
		
			DeviceRequest request = new DeviceRequest() {
				Action = handle,
				Params = args,
				ActionType = DeviceRequest.ObjectActionType.Invoke,
				Identifier = m_identifier
			};

            INetworkMessage response = Send(request);

            return response.Payload.ActionResponse;

		}

        /// <summary>
        /// Will add a request to the execution queue for this device
        /// and execute it when eligible.
        /// </summary>
        /// <param name="task">Task to be executed.</param>
        public void AddTask(AsyncRemoteDeviceTask task) {

            if (LossyRequests) {

                if (Busy) { m_lastAsyncTask = task; } else { task.Start(); }
           
            } else {

                m_lastAsyncTask = null;
                task.Start();

            }

        }

        #region DynamicObject

        public override bool TrySetMember(SetMemberBinder binder, object value) {

			Set(binder.Name, value);
			return true;

		}

		public override bool TryGetMember(GetMemberBinder binder, out object result) {

			result = Get(binder.Name);
			return true;

		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {

			result = Invoke(binder.Name, args);
			return true;

		}

        #endregion

        public override string ToString() {

            return $"RemoteDevice[{Identifier}]: Host {m_host}";

        }

    }

}