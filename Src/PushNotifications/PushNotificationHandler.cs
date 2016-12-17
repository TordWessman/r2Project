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
using Core.Memory;
using System.Collections.Generic;
using Core;
using System.Linq;

namespace PushNotifications
{
	public class PushNotificationHandler : DeviceBase, IPushNotificationProxy
	{

		private const string PUSH_CLIENT_ID = "push_client_id";
		private const string PUSH_CLIENT_TYPE = "push_client_type";
		private const string PUSH_CLIENT_TOKEN = "push_client_token";

		private IMemorySource m_memory;
		private ICollection<IPushNotificationFacade> m_facades;

		public PushNotificationHandler (string id, IMemorySource memory) : base (id)
		{
			m_memory = memory;
			m_facades = new List<IPushNotificationFacade> ();
		}


		public void RegisterClient (string deviceId, string deviceToken, PushNotificationClientTypes type = PushNotificationClientTypes.Apple)
		{
			IMemory entry = m_memory.All (PUSH_CLIENT_ID).Where(d => d.Value == deviceId).FirstOrDefault();

			if (entry == null) {

				entry = m_memory.Create (PUSH_CLIENT_ID, deviceId);
			
			}

			IMemory clientType = entry.GetAssociation (PUSH_CLIENT_TYPE);

			if (clientType == null) {

				clientType = m_memory.Create (PUSH_CLIENT_TYPE, ((int)type).ToString());
				entry.Associate (clientType);
			
			}

			IMemory token = entry.GetAssociation (PUSH_CLIENT_TOKEN);

			if (token == null) {

				token = m_memory.Create (PUSH_CLIENT_TOKEN, deviceToken);
			
			} else {

				token.Value = deviceToken;

			}


			entry.Associate (token);
			clientType.Associate (token);

		}

		public void AddFacade (IPushNotificationFacade facade)
		{
		
			m_facades.Add (facade);
		
		}

		public void Broadcast(IPushNotification notification) {

			foreach (IPushNotificationFacade facade in m_facades) {

				if (facade.AcceptsNotification(notification)) {

					List<string> tokens = new List<string> ();

					foreach (IMemory type in m_memory.All(PUSH_CLIENT_TYPE)) {

						if ((notification.ClientTypeMask & Convert.ToInt32 (type.Value)) > 0) {

							IMemory token = type.GetAssociation (PUSH_CLIENT_TOKEN);

							if (token != null) {
							
								tokens.Add (token.Value);

							} else {

								Log.e ("PushNotification Broadcast error: A device of type: " + PUSH_CLIENT_TYPE + " has no token.");
							
							}

						}

					}

					facade.QueuePushNotification (notification, tokens);

				}

			}

		}

	}

}

