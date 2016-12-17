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

namespace Core.Network.Web
{
	public class DeviceReceiver: IWebObjectReceiver
	{
		private IDeviceManager m_deviceManager;
		private INetworkSecurity m_security;

		/// <summary>
		/// The token is used as a very simple mean of authentication...
		/// </summary>
		/// <param name="deviceManager">Device manager.</param>
		/// <param name="token">Token.</param>
		public DeviceReceiver (IDeviceManager deviceManager, INetworkSecurity security = null)
		{
			m_deviceManager = deviceManager;
			m_security = security;
		}

		public IWebIntermediate onReceive (dynamic message, string httpMethod, NameValueCollection headers = null) {


			if (m_security?.IsValid(message.Token) == false) {
			
				throw new SecurityException ("Invalid credentials for message.");

			}

			IDevice device = m_deviceManager.Get (message.Object.Identifier);

			IList<dynamic> parameterList = null;

			if (message.Params?.Count > 0) {
				
				parameterList = new List<dynamic> ();

				foreach (dynamic parameter in message.Params) {
				
					parameterList.Add (JsonObjectRequest.Param.ParseValue (parameter));

				}

			}
			 
			object[] p = parameterList?.ToArray();
			JsonObjectResponse response = new JsonObjectResponse ();
				
			if (message.Type == (int)JsonObjectRequest.ActionType.Invoke) {

				System.Reflection.MethodInfo methodInfo = device.GetType ().GetMethod (message.Action);
				methodInfo.Invoke (device, p);

			} else if (message.Type == (int)JsonObjectRequest.ActionType.InvokeWithResponse) {

				response.ActionResponse = device.GetType ().GetMethod (message.Action).Invoke (device, p);

			} else if (message.Type == (int)JsonObjectRequest.ActionType.Set) {

				System.Reflection.PropertyInfo propertyInfo = device.GetType().GetProperty(message.Action);
				propertyInfo.SetValue(device, Convert.ChangeType(p[0], propertyInfo.PropertyType), null);

			}

			response.Object = device;

			return new JsonObjectIntermediate(response);

		}
	}
}

