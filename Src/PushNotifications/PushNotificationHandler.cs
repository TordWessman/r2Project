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
using R2Core.Device;
using System.Collections.Generic;
using R2Core;
using System.Linq;
using System.Threading.Tasks;
using R2Core.DataManagement.Memory;

namespace R2Core.PushNotifications
{
	/// <summary>
	/// Uses the IMemorySource to keep track of and send push notifications.
	/// </summary>
	public class PushNotificationHandler: DeviceBase, IPushNotificationProxy, ITaskMonitored
	{

		public const string PUSH_CLIENT_ID = "push_client_id";
		public const string PUSH_CLIENT_TYPE = "push_client_type";
		public const string PUSH_CLIENT_TOKEN = "push_client_token";

		private IMemorySource m_memory;
		private ICollection<IPushNotificationFacade> m_facades;
		private bool m_isSending;
		private Task m_sendTask;

		public PushNotificationHandler (string id, IMemorySource memory) : base (id)
		{
			
			m_memory = memory;
			m_facades = new List<IPushNotificationFacade> ();
		
		}

		public void Register (string deviceId, string deviceToken, int rawType) {
		
			RegisterClient (deviceId, deviceToken, (PushNotificationClientType) rawType);

		}

		public override bool Ready { get { return !m_isSending; } }

		public void RegisterClient (string deviceId, string deviceToken, PushNotificationClientType type)
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

		public void AddFacade (IPushNotificationFacade facade) { m_facades.Add (facade); }

		public void Broadcast(IPushNotification notification) {

			m_isSending = true;

			m_sendTask = Task.Factory.StartNew( () => {
			
				m_facades.AsParallel().ForAll (facade => {

					SendNotifictation(notification, facade);

				});

				m_isSending = false;

			});

		}

		public IDictionary<string,Task> GetTasksToObserve() {
		
			return new Dictionary<string,Task> () { { this.ToString (), m_sendTask } };
		
		}

		private void SendNotifictation (IPushNotification notification, IPushNotificationFacade facade) {

			if (facade.AcceptsNotification(notification)) {

				List<string> tokens = new List<string> ();

				foreach (IMemory type in m_memory.All(PUSH_CLIENT_TYPE)) {

					if ((notification.ClientTypeMask & Convert.ToInt32 (type.Value)) > 0) {

						IMemory token = type.GetAssociation (PUSH_CLIENT_TOKEN);

						if (token != null) {

							tokens.Add (token.Value);

						} else {

							Log.e ($"PushNotification Broadcast error: A device of type '{PUSH_CLIENT_TYPE}' has no token.");

						}

					}

				}

				facade.QueuePushNotification (notification, tokens);

			}

		}

	}

}