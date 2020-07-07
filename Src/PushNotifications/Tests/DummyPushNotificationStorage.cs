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
using R2Core.Device;
using System.Linq;

namespace R2Core.PushNotifications.Tests {

    internal class DummyPushNotificationStorage : DeviceBase, IPushNotificationStorage {

        public List<PushNotificationRegistryItem> Items = new List<PushNotificationRegistryItem>();

        public DummyPushNotificationStorage() : base ("dummy_push_storage") { }

        public IEnumerable<PushNotificationRegistryItem> Get(PushNotificationRegistryItem request) {

            return Items.Where(i => (request.Group == null || i.Group == request.Group) && i.IdentityName == request.IdentityName);

        }

        public void Register(PushNotificationRegistryItem request) {

            Items.Add(request);
        }

        public void Remove(PushNotificationRegistryItem request) {

            Items.Remove(i => { return i.Token == request.Token && i.IdentityName == request.IdentityName && i.Group == request.Group; });
        
        }
    
    }

}
