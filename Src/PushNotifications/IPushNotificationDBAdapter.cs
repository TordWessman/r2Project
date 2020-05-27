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
using R2Core.Common;

namespace R2Core.PushNotifications {

    /// <summary>
    /// Database adapter handling push notification entries
    /// </summary>
    public interface IPushNotificationDBAdapter : IDBAdapter {

        /// <summary>
        /// Store a new token/identity/group record (if not duplicate).
        /// </summary>
        /// <param name="item">Item.</param>
        void Save(PushNotificationRegistryItem item);

        /// <summary>
        /// Remove all registered devices with ´token´. Only remove from ´group´ if ´group´ is not null.
        /// </summary>
        /// <param name="token">Token.</param>
        /// <param name="group">Group.</param>
        void Remove(string token, string group = null);

        /// <summary>
        /// Return all registry items from ´identity´ (and ´group´ if ´group´ is not null).
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="identity">Identity.</param>
        /// <param name="group">Group.</param>
        IEnumerable<PushNotificationRegistryItem> Get(string identity, string group = null);

    }

}
