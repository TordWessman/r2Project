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
using System.Collections.Generic;

namespace R2Core.PushNotifications {

    /// <summary>
    /// Maintaining a registry push notification devices and their associated groups and identities.
    /// </summary>
    public interface IPushNotificationStorage {

        /// <summary>
        /// Register a device for push notifications.
        /// </summary>
        /// <param name="request">Request.</param>
        void Register(PushNotificationRegistryItem request);

        /// <summary>
        /// Remove device token(s) from push notifications. Will be constrained by Group if specified.
        /// </summary>
        /// <param name="request">Request.</param>
        void Remove(PushNotificationRegistryItem request);

        /// <summary>
        /// Return all devices matching the request's Identity and ´Group´ if specified.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="request">Request.</param>
        IEnumerable<PushNotificationRegistryItem> Get(PushNotificationRegistryItem request);

    }

}
