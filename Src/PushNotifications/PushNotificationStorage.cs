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
using System;
using System.Collections.Generic;
using R2Core.Device;

namespace R2Core.PushNotifications {

    public class PushNotificationStorage : DeviceBase {

        private IPushNotificationDBAdapter m_adapter;

        public PushNotificationStorage(string id, IPushNotificationDBAdapter adapter) : base (id) {

            m_adapter = adapter;
            m_adapter.SetUp();

        }

        /// <summary>
        /// Register a device for push notifications.
        /// </summary>
        /// <param name="request">Request.</param>
        public void Register(PushNotificationRegistryItem request) {

            m_adapter.Save(request);

        }

        /// <summary>
        /// Remove device token(s) from push notifications. Will be constrained by Group if specified.
        /// </summary>
        /// <param name="request">Request.</param>
        public void Remove(PushNotificationRegistryItem request) {

            m_adapter.Remove(request.Token, request.Group);

        }

        /// <summary>
        /// Return all devices matching the request's Identity and ´Group´ if specified.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="request">Request.</param>
        public IEnumerable<PushNotificationRegistryItem>Get(PushNotificationRegistryItem request) {

            return m_adapter.Get(request.IdentityName, request.Group);

        }

    }

}
