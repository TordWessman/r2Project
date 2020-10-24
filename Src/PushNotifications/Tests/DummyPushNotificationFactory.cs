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
using R2Core.Device;

namespace R2Core.PushNotifications.Tests {

    public class DummyPushNotificationFactory : DeviceBase, IPushNotificationFactory {

        public DummyPushNotificationFactory() : base (Settings.Identifiers.PushFactory()) {

           
        }

        public void SetPushSharpLogger(IMessageLogger logger) { }

        public PushNotification CreateSimple(string message, string identityName, string group = null) {

            PushNotification note = new PushNotification {
                Message = message,
                IdentityName = identityName,
                Group = group
            };

            return note;

        }

        public IPushNotificationFacade CreateAppleFacade(string id, string password, string appleCertFile) {

            return new DummyPushNotificationFacade(PushNotificationClientType.Apple);

        }

        public IPushNotificationFacade CreateAndroidFacade(string id, string senderId, string authToken) {

            return new DummyPushNotificationFacade(PushNotificationClientType.Android);

        }

        public IPushNotificationProxy CreateProxy(string id, IPushNotificationStorage storage = null) {

            return new PushNotificationProxy(id, storage);

        }

        public IPushNotificationStorage CreateStorage(string id) {

            return new DummyPushNotificationStorage();

        }

    }

}
