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
using NUnit.Framework;
using R2Core.Tests;

namespace R2Core.PushNotifications.Tests {

    [TestFixture]
    public class PushNotificationHandlerTests : TestBase {
       
        [Test]
        public void TestHandler() {
            PrintName();

            var storage = new DummyPushNotificationStorage();
            var factory = new PushNotificationFactory("factory", m_dataFactory);
            var handler = factory.CreateProxy("handler", storage);

            var appleFacade = new DummyPushNotificationFacade(PushNotificationClientType.Apple);
            var androidFacade = new DummyPushNotificationFacade(PushNotificationClientType.Android);

            var items = new PushNotificationRegistryItem[] {
                new PushNotificationRegistryItem { Token = "t1", Group = "g1", IdentityName = "i1", ClientType = PushNotificationClientType.Apple },
                new PushNotificationRegistryItem { Token = "t2", Group = "g1", IdentityName = "i1", ClientType = PushNotificationClientType.Apple },
                new PushNotificationRegistryItem { Token = "t1", Group = "g2", IdentityName = "i1", ClientType = PushNotificationClientType.Apple },
                new PushNotificationRegistryItem { Token = "t3", Group = "g1", IdentityName = "i1", ClientType = PushNotificationClientType.Android },
                new PushNotificationRegistryItem { Token = "t1", Group = "g1", IdentityName = "i2", ClientType = PushNotificationClientType.Apple },
                new PushNotificationRegistryItem { Token = "t3", Group = "g2", IdentityName = "i2", ClientType = PushNotificationClientType.Android },
            };

            handler.AddFacade(appleFacade);
            handler.AddFacade(androidFacade);

            foreach (var item in items) { storage.Register(item); }

            handler.Broadcast(new PushNotification { Group = "g1", IdentityName = "i1", Message = "foo" });

            handler.WaitFor();
            Assert.Equals(1, appleFacade.SentNotifications["t1"].Count);
            Assert.Equals(1, appleFacade.SentNotifications["t2"].Count);
            Assert.Equals(1, androidFacade.SentNotifications["t3"].Count);
            //Assert.That(false == appleFacade.SentNotifications.ContainsKey("t3"));

            handler.Broadcast(new PushNotification { IdentityName = "i2", Message = "foo" });
            handler.WaitFor();
            Assert.Equals(2, appleFacade.SentNotifications["t1"].Count);
            Assert.Equals(1, appleFacade.SentNotifications["t2"].Count);
            Assert.Equals(2, androidFacade.SentNotifications["t3"].Count);

            handler.Broadcast(new PushNotification { IdentityName = "i2", Group = "g2", Message = "foo" });
            handler.WaitFor();

            Assert.Equals(2, appleFacade.SentNotifications["t1"].Count);
            Assert.Equals(1, appleFacade.SentNotifications["t2"].Count);
            Assert.Equals(3, androidFacade.SentNotifications["t3"].Count);
        }

    }

}
