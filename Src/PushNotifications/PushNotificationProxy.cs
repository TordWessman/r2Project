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

using R2Core.Device;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R2Core.PushNotifications
{
	/// <summary>
	/// Uses the IMemorySource to keep track of and send push notifications.
	/// </summary>
	public class PushNotificationProxy : DeviceBase, IPushNotificationProxy, ITaskMonitored {

        private readonly IPushNotificationStorage m_storage;
		private readonly IDictionary<PushNotificationClientType, IPushNotificationFacade> m_facades;

		private bool m_isSending;
		private Task m_sendTask;

		public PushNotificationProxy(string id, IPushNotificationStorage storage) : base(id) {

            m_storage = storage;
			m_facades = new Dictionary<PushNotificationClientType, IPushNotificationFacade>();
		
		}

		public override bool Ready { get { return !m_isSending; } }

		public void AddFacade(IPushNotificationFacade facade) {

            m_facades[facade.ClientType] = facade;

        }

		public void Broadcast(PushNotification notification) {

			m_isSending = true;

			m_sendTask = Task.Factory.StartNew( () => {

                foreach (PushNotificationRegistryItem item in m_storage.Get(new PushNotificationRegistryItem { Group = notification.Group, IdentityName = notification.IdentityName })) {

                    try {

                        m_facades[item.ClientType].Send(notification, item.Token);

                    } catch (Exception ex) {
    
                        Log.e(ex.Message, "Push");

                    }

                }

				m_isSending = false;

			});

		}

		public IDictionary<string,Task> GetTasksToObserve() {
		
			return new Dictionary<string,Task> { { ToString(), m_sendTask } };
		
		}

	}

}