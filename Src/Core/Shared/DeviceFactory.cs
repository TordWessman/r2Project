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

﻿using System;
using Core.Device;
using Core.Network.Http;
using Core.Memory;
using MemoryType = System.String;
using System.Linq;
using System.Dynamic;

namespace Core
{
	/// <summary>
	/// Device factory for the creation of uncategorized shared devices. Most of the factory methods should be moved to a more domain specific factory.
	/// </summary>
	public class DeviceFactory : DeviceBase
	{
		private IDeviceManager m_deviceManager;
		private IMemorySource m_memory;

		public DeviceFactory (string id, IDeviceManager deviceManager, IMemorySource memory) : base (id)
		{
			m_deviceManager = deviceManager;
			m_memory = memory;
		}

		public IDeviceManager DeviceManager { get { return m_deviceManager; } }
		public IMemorySource Memory { get { return m_memory; } }

		/// <summary>
		/// Creates a simple gstreamer pipeline. Requires libr2gstparseline.so to be compiled and installed into your system library directory.
		/// </summary>
		/// <returns>The gstream.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="pipeline">Pipeline.</param>
		public IGstream CreateGstream(string id, string pipeline) {
		
			return new Gstream (id, pipeline);
		
		}

		public HttpFactory CreateHttpFactory(string id) {
		
			return new HttpFactory (id);

		}

		/*
		public IHttpEndpoint CreateJsonInterpreter(string deviceListenerPath) {
			
			return new HttpJsonEndpoint(deviceListenerPath,
				(message, method) => {

					IMemory requestToken = Memory.Get("client_token");

					if (requestToken == null) { 

						string err = "client_data.token not set. Aborting JsonDeviceMessage interpretation"; 
						Log.e(err);
						return new JsonDeviceMessage() {Status = (int)JsonDeviceMessage.Statuses.ServerNotRegisteredAToken, Data = err} ;

					} 

					if (requestToken.Value != message.Token) {

						string err = "Bad token sent from client: " + message.Token; 
						Log.w(err);
						return new JsonDeviceMessage() {Status = (int)JsonDeviceMessage.Statuses.GeneralError, Data = err} ;

					}

					if (message == null) {

						string err = "Message was null!"; 
						Log.e(err);
						return new JsonDeviceMessage() {Status = (int)JsonDeviceMessage.Statuses.GeneralError, Data = err} ;

					} else if (message.Device == null) {

						string err ="Message.Device was null!"; 
						Log.e(err);
						return new JsonDeviceMessage() {Status = (int)JsonDeviceMessage.Statuses.GeneralError, Data = err};

					} else if (DeviceManager.Has(message.Device)) {

						if (method == "post") {

							IDevice device = DeviceManager.Get(message.Device);
							if (device is IJSONAccessible) {
								message.Data = (device as IJSONAccessible).Interpret(message.Function, message.Params);
								return message;
							}

							Log.w("JsonRequest: Device not IExternallyAccessible: " + message.Device);

						} else {

							Log.w("JsonRequest: Method not supported " + method);

						}

					}

					string error = "JsonRequest: Device not found: " + message.Device; 
					Log.e(error);
					dynamic response = new ExpandoObject();
					response.Status = (int)JsonDeviceMessage.Statuses.GeneralError;
					response.Data = error;
					return response;

				});

			throw new NotImplementedException ();

		}*/

	}

}

