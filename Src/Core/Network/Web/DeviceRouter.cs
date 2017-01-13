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
using System.Collections.Specialized;
using System.Collections.Generic;
using Core.Device;
using Newtonsoft.Json;
using System.Linq;
using System.Security;
using System.Dynamic;

namespace Core.Network.Web
{
	/// <summary>
	/// A DeviceRouter is a specialized helper IWebObjectReceiver usable for dealing with devices over http/websockets. It's onReceive method requires a JsonObjectRequest formatted message, evaluates it's device id, values and actions, perform the requested action on the requested device and returns the device object affected. It keeps track of devices requested and will communicate changes.
	/// </summary>
	public class DeviceRouter: IWebObjectReceiver, IDeviceObserver
	{
		private IDeviceManager m_deviceManager;

		// Optional security parameter.
		private INetworkSecurity m_security;

		// An object which will be used to send changes in devices which has been invoked.
		private IWebSocketSender m_sender;

		// Contains a list identifiers for all devices being invoked
		private IList<string> m_registeredDevices;

		/// <summary>
		/// The token is used as a very simple mean of authentication...
		/// </summary>
		/// <param name="deviceManager">Device manager.</param>
		/// <param name="token">Token.</param>
		public DeviceRouter (IDeviceManager deviceManager, INetworkSecurity security = null)
		{
			
			m_deviceManager = deviceManager;
			m_security = security;
			m_registeredDevices = new List<string> ();

		}

		public IWebIntermediate OnReceive (dynamic message, string httpMethod, NameValueCollection headers = null) {

			string token = ((IDictionary<string, Object>)message)?.ContainsKey("Token") == true ? message.Token : null; 

			if (m_security?.IsValid(token) == false) {
			
				throw new SecurityException ("Invalid credentials for message.");

			}

			IDevice device = m_deviceManager.Get (message.Identifier);

			if (device == null) {

				throw new DeviceException ("Device with id: " + message.Id + " not found.");
			
			}

			m_registeredDevices.Add (device.Identifier);

			IList<dynamic> parameterList = null;

			if ((((IDictionary<string, Object>)message)?.ContainsKey("Params") == true) && message.Params?.Count > 0) {

				// Only add parameters if Params object array is present.

				parameterList = new List<dynamic> ();

				foreach (dynamic parameter in message.Params) {
				
					parameterList.Add (JsonObjectRequest.Param.ParseValue (parameter));

				}

			}
			 
			object[] p = parameterList?.ToArray();
			JsonObjectResponse response = new JsonObjectResponse ();
				
			/*if (Convert.ToInt32(message.Type) == (int)JsonObjectRequest.ActionType.Invoke) {

				System.Reflection.MethodInfo methodInfo = device.GetType ().GetMethod (message.Action);
				response.Action = message.Action;
				methodInfo.Invoke (device, p);

			} else */if (Convert.ToInt32(message.Type) == (int)JsonObjectRequest.ActionType.Invoke) {

				response.ActionResponse = device.GetType ().GetMethod (message.Action).Invoke (device, p);
				response.Action = message.Action;

			} else if (Convert.ToInt32(message.Type) == (int)JsonObjectRequest.ActionType.Set) {

				System.Reflection.PropertyInfo propertyInfo = device.GetType().GetProperty(message.Action);
				propertyInfo.SetValue(device, Convert.ChangeType(p[0], propertyInfo.PropertyType), null);
				response.Action = propertyInfo.Name;

			}

			response.Object = device;

			return new JsonObjectIntermediate(response);

		}

		public IWebSocketSender Sender { 
			get { return m_sender; }
			set { m_sender = value; }
		}

		#region IDeviceObserver

		public void OnValueChanged(IDeviceNotification<object> notification) {

			IDevice device = m_deviceManager.Get (notification.Identifier);

			if (m_sender != null && device != null && m_registeredDevices.Contains(device.Identifier)) {
			
				// Send device through sender to nofie connected clients about the change.

				JsonObjectResponse response = new JsonObjectResponse ();
				response.Action = notification.Action;
				response.ActionResponse = notification.NewValue;
				response.Object = device;

				m_sender.Send (response);

			}

		}

		#endregion

	}

}