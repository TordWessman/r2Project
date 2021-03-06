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
using System.Collections.Generic;
using R2Core.Device;
using Newtonsoft.Json;
using System.Linq;
using System.Security;
using System.Dynamic;
using System.Reflection;
using System.Net;

namespace R2Core.Network
{
	/// <summary>
	/// A DeviceRouter is a specialized helper IWebObjectReceiver usable for dealing with devices using the a WebJsonEndpoint. It's onReceive method requires a JsonObjectRequest formatted message, evaluates it's device id, values and actions, perform the requested action on the requested device and returns the device object affected. It keeps track of devices requested and will communicate changes.
	/// 
	///  - Object to invoke:
	/// interface ITestObject: IDevice {
	/// 
	/// 	// Return true if values were updated.
	/// 	bool UpdateNumberAndString(int aNumber, string aString);
	/// 	int MyNumber {get;}
	/// 	string MyString {get;}
	/// 
	/// }
	/// 
	///  - Example of a request structure:
	/// {
	/// 	Params [ 42, "foo" ],
	/// 	ActionType: 2,
	/// 	Action: "UpdateNumberAndString",
	/// 	Identifier: "device_with_a_number_and_a_string"
	/// }
	/// 
	///  - Example of a response structure
	/// {
	/// 	Object: {
	/// 		Identifier: "device_with_a_number_and_a_string",
	/// 		Ready: true,
	/// 		MyNumber: 42,
	/// 		MyString: "foo"
	/// 	},
	/// 	Action: "UpdateNumberAndString",
	/// 	ActionResponse: true
	/// }
	/// </summary>
	public class DeviceRouter : IWebObjectReceiver, IDeviceObserver {

		private ObjectInvoker m_invoker;

		private IDeviceContainer m_deviceContainer;

		// Contains a list identifiers for all devices available.
		private IDictionary<string, IDevice> m_devices;

		public string DefaultPath { get { return Settings.Consts.DeviceDestination(); } }

        /// <summary>
        /// The token is used as a very simple mean of authentication...
        /// </summary>
        /// <param name="deviceContainer">Device manager.</param>
        public DeviceRouter(IDeviceContainer deviceContainer = null) {

			m_devices = new Dictionary<string, IDevice>();
			m_invoker = new ObjectInvoker();
			m_deviceContainer = deviceContainer;
		}

		public void AddDevice(IDevice device) {
		
			m_devices[device.Identifier] = device;

		}

		public INetworkMessage OnReceive(INetworkMessage message, IPEndPoint source) {

			if (!(message.Payload is IDynamicMetaObjectProvider) && !(message.Payload is DeviceRequest)) {
			
				throw new NetworkException($"Unable process request: {message}. Payload is not of type DeviceRequest or IDynamicMetaObjectProvider (i.e. R2Dynamic or ExpandoObject).");

			}

			IDevice device = null;

			IEnumerable<IDevice> devices = m_deviceContainer == null ? m_devices.Values : m_devices.Values.Concat(m_deviceContainer.LocalDevices);

			if (message.Payload?.Identifier != null) {

				device = devices.Where((d) => d.Identifier == message.Payload.Identifier).Select((d) => d).FirstOrDefault();

			}

			if (device == null) {

				throw new DeviceException($"Device with id: '{message.Payload?.Identifier}' not found.");
			
			}

			DeviceResponse response = new DeviceResponse();
				
			response.Action = message.Payload.Action;

			if (Convert.ToInt32(message.Payload.ActionType) == (int)DeviceRequest.ObjectActionType.Invoke) {

				response.ActionResponse = m_invoker.Invoke(device, message.Payload.Action, message.Payload.Params);

			} else if (Convert.ToInt32(message.Payload.ActionType) == (int)DeviceRequest.ObjectActionType.Set) {
				
				m_invoker.Set(device, message.Payload.Action, message.Payload.Params?[0]);
				response.ActionResponse = m_invoker.Get(device, message.Payload.Action);

			} else if (Convert.ToInt32(message.Payload.ActionType) == (int)DeviceRequest.ObjectActionType.Get) { 

				if (m_invoker.ContainsMember(message.Payload, "Action")) {

					// Include the value of the invoked property
					response.ActionResponse = m_invoker.Get(device, message.Payload.Action);

				} 

			} else {
			
				throw new DeviceException($"ActionType {Convert.ToInt32 (message.Payload.ActionType)} not identified(device: {device.Identifier}).");

			}

			response.Object = device;

			return new NetworkMessage() {Code = 200, Payload = response};

		}

		#region IDeviceObserver

		public void OnValueChanged(IDeviceNotification<object> notification) {

			/*
			 * 
			 * //Wait for TCP Duplex implementation
			IDevice device = m_deviceManager.Get(notification.Identifier);

			if (m_sender != null && device != null && m_registeredDevices.Contains(device.Identifier)) {
			
				// Send device through sender to nofie connected clients about the change.

				WebObjectResponse response = new WebObjectResponse();
				response.Action = notification.Action;
				response.ActionResponse = notification.NewValue;
				response.Object = device;

				m_sender.Send(response);

			}*/

		}

		#endregion

	}

}