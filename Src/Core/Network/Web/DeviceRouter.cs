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
using System.Collections.Specialized;
using System.Collections.Generic;
using Core.Device;
using Newtonsoft.Json;
using System.Linq;
using System.Security;
using System.Dynamic;
using System.Reflection;

namespace Core.Network.Web
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
	/// 	Token: "optional password",
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
	public class DeviceRouter: IWebObjectReceiver, IDeviceObserver
	{
		private IDeviceManager m_deviceManager;

		// Optional security parameter.
		private INetworkSecurity m_security;

		// An object which will be used to send changes in devices which has been invoked.
		private IWebSocketSender m_sender;

		// Contains a list identifiers for all devices being invoked. Used by the IDeviceObserver implementation.
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

		public IWebIntermediate OnReceive (dynamic message, IDictionary<string, object> metadata = null) {

			if (m_security?.IsValid(message.Token) == false) {
			
				throw new SecurityException ("Invalid credentials for message. Bad Token.");

			}

			IDevice device = m_deviceManager.Get (message.Identifier);

			if (device == null) {

				throw new DeviceException ($"Device with id: {message.Id} not found.");
			
			}

			if (!m_registeredDevices.Contains (device.Identifier)) {
			
				m_registeredDevices.Add (device.Identifier);

			}

			// Input parameters for methods and setters
			//object[] p = message.Params?.ToArray();

			JsonObjectResponse response = new JsonObjectResponse ();
				
			if (Convert.ToInt32(message.ActionType) == (int)JsonObjectRequest.ActionType.Invoke) {

				ParameterInfo[] paramsInfo = device.GetType ().GetMethod (message.Action).GetParameters ();

				if (paramsInfo.Length != (int)(message.Params?.Count ?? 0)) {
				
					throw new ArgumentException ($"Wrong number of arguments for {message.Action} in {device.Identifier}. {message.Params?.Count} provided but {paramsInfo.Length} are required.");

				}

				IList<object> p = new List<object> ();

				for (int i = 0; i < paramsInfo.Length; i++) {

					//Convert the dynamic parameter to the type required by the method.
					p.Add(Convert.ChangeType(message.Params[i], paramsInfo[i].ParameterType));

				}

				response.ActionResponse = device.GetType ().GetMethod (message.Action).Invoke (device, p.ToArray());
				response.Action = message.Action;

			} else if (Convert.ToInt32(message.ActionType) == (int)JsonObjectRequest.ActionType.Set) {
				
				PropertyInfo propertyInfo = device.GetType().GetProperty(message.Action);

				if (propertyInfo == null) {

					MemberInfo[] members = device.GetType ().GetMember (message.Action);

					if (members.Length == 0) { 
					
						throw new ArgumentException ("Property '{message.Action}' not found in device '{device.Identifier}'.");

					} else if (!(members [0] is FieldInfo)) {
					
						throw new ArgumentException ("Unable to access property '{message.Action}' in device '{device.Identifier}'.");

					}

					FieldInfo fieldInfo = (members [0] as FieldInfo);
					fieldInfo.SetValue (device, Convert.ChangeType (message.Params [0], fieldInfo.FieldType));
					response.Action = fieldInfo.Name;

				} else {
				
					propertyInfo.SetValue(device, Convert.ChangeType (message.Params [0], propertyInfo.PropertyType), null);
					response.Action = propertyInfo.Name;
				}


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