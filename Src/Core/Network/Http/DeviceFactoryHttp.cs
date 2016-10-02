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
using Core.Memory;
using Core.Device;
using System.Linq;

namespace Core.Network.Http
{
	public static class DeviceFactoryHttp
	{
		/** 
		 * TODO: Not really sure how to make IronRuby accept extension methods..
		public static IHttpServerInterpreter CreateImageInterpreter (this DeviceFactory self, string imagePath, string imageUri) {

			return new ImageInterpreter (imagePath, imageUri);

		}

		public static IHttpServerInterpreter CreateJsonInterpreter(this DeviceFactory self, string deviceListenerPath) {

			return new JsonInterpreter<JsonDeviceMessage,JsonDeviceMessage>(deviceListenerPath,
				(message, method) => {

					var tokens = self.Memory.GetMemoriesOfType("client_token");

					if (tokens.Count == 0) { 

						string err = "client_data.token not set. Aborting JsonDeviceMessage interpretation"; 
						Log.e(err);
						return new JsonDeviceMessage() {Status = (int)JsonDeviceMessage.Statuses.ServerNotRegisteredAToken, Data = err} ;

					} else if (tokens.Count > 1) { 

						string err = "Memory contains > 1 client_token (implying that there have been at least two tokens saved separately)! Aborting JsonDeviceMessage interpretation"; 
						Log.e(err);
						return new JsonDeviceMessage() {Status = (int)JsonDeviceMessage.Statuses.GeneralError, Data = err} ;
					}

					IMemory requestToken = tokens.First();

					if (requestToken.Name != message.Token) {

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

					} else if (self.DeviceManager.Has(message.Device)) {

						if (method== "post") {
							IDevice device = self.DeviceManager.Get(message.Device);
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
					return new JsonDeviceMessage() {Status = (int)JsonDeviceMessage.Statuses.GeneralError, Data = error} ;

				});

		}

		public static IHttpServer CreateHttpServer (this DeviceFactory self, string id, int port) {

			return new HttpServer (id, port);

		}

		public static IJsonClient CreateJsonClient(this DeviceFactory self, string id, string serverUrl) {

			return new JsonClient (id, serverUrl);

		}

		public static JsonMessageFactory CreateJsonMessageFactory(this DeviceFactory self, string id) {

			return new JsonMessageFactory (id);

		}
		*/
		/*
		public static object Create (string type, string id, params object parameters) {

			if (type == "json_client") {
			
				return new JsonClient (id, (string)parameters [0]);

			}

			return null;

		}*/

	}
}

