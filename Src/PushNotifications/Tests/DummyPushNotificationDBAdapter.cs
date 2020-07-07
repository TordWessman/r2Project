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
using System.Linq;

namespace R2Core.PushNotifications.Tests {

    internal class DummyPushNotificationDBAdapter : IPushNotificationDBAdapter {

        public List<PushNotificationRegistryItem> Items;

        public bool Ready => true;

        public IEnumerable<PushNotificationRegistryItem> Get(string identity, string group = null) {

            return Items.Where(i => i.IdentityName == identity && (group == null || i.Group == group));

        }

        public void Remove(string token, string group = null) {

            Items.Remove(i => i.Token == token && (group == null || i.Group == group));

        }

        public void Save(PushNotificationRegistryItem item) {

            Items.Add(item);

        }

        public void SetUp() {

            Items = new List<PushNotificationRegistryItem>();

        }

    }

}
