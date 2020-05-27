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
using NUnit.Framework;
using R2Core.PushNotifications;
using System.Linq;
using R2Core.Network;
using R2Core.Device;
using R2Core.Common;

namespace R2Core.Tests {

   
    [TestFixture]
    public class PushNotificationTests : TestBase {

        [Test]
        public void TestPushNotificationStorageAdapter() {
            PrintName();

            DataFactory factory = new DataFactory("factory");
            ISQLDatabase database = factory.CreateTemporaryDatabase();
            database.Start();
            PushNotificationDBAdapter adapter = new PushNotificationDBAdapter(database);
            adapter.SetUp();

            PushNotificationRegistryItem item1 = new PushNotificationRegistryItem {
                Token = "t1", Group = "g1", IdentityName = "i1",
                ClientType = PushNotificationClientType.Apple
            };

            // Item should be saved
            adapter.Save(item1);
            Assert.AreEqual(1, adapter.Get("i1").Count());

            // Duplicates should be ignored
            adapter.Save(item1);
            Assert.AreEqual(1, adapter.Get("i1").Count());

            // No identity named i2
            Assert.AreEqual(0, adapter.Get("i2").Count());

            // No group named g2
            Assert.AreEqual(0, adapter.Get("i1", "g2").Count());

            PushNotificationRegistryItem item2 = new PushNotificationRegistryItem {
                Token = "t1", Group = "g2", IdentityName = "i1",
                ClientType = PushNotificationClientType.Apple
            };

            adapter.Save(item2);

            // g2 should exist
            Assert.AreEqual(1, adapter.Get("i1", "g2").Count());

            // fetch all from identity i1
            Assert.AreEqual(2, adapter.Get("i1").Count());

            adapter.Remove("t1", "g2");

            // g2 should have been removed
            Assert.AreEqual(0, adapter.Get("i1", "g2").Count());
            Assert.AreEqual(1, adapter.Get("i1").Count());

            adapter.Save(item2);

            // g2 should have been recreated
            Assert.AreEqual(1, adapter.Get("i1", "g2").Count());

            PushNotificationRegistryItem item3 = new PushNotificationRegistryItem {
                Token = "t2", Group = "g2", IdentityName = "i1",
                ClientType = PushNotificationClientType.Apple
            };

            adapter.Save(item3);
            adapter.Remove("t1");

            // All t1 entries should be removed, but t2 should still be there
            Assert.AreEqual(1, adapter.Get("i1", "g2").Count());

            PushNotificationRegistryItem item4 = new PushNotificationRegistryItem {
                Token = "t1", Group = "g2", IdentityName = "i2",
                ClientType = PushNotificationClientType.Apple
            };

            adapter.Save(item4);

            // The new identity i2 should have been created without affecting i1
            Assert.AreEqual(1, adapter.Get("i1", "g2").Count());
            Assert.AreEqual(1, adapter.Get("i2", "g2").Count());

        }

        [Test]
        public void TestRemotePushNotificationStorage() {
            PrintName();
            IPushNotificationDBAdapter adapter = new DummyPushNotificationDBAdapter();

            PushNotificationStorage storage = new PushNotificationStorage("storage", adapter);

            PushNotificationRegistryItem item1 = new PushNotificationRegistryItem {
                Token = "t1", Group = "g1", IdentityName = "i1",
                ClientType = PushNotificationClientType.Apple
            };

            Assert.AreEqual(0, storage.Get(item1).Count());
            storage.Register(item1);
            Assert.AreEqual(1, storage.Get(item1).Count());

            WebFactory webFactory = new WebFactory("wf", new JsonSerialization("s"));

            HttpServer httpServer = webFactory.CreateHttpServer("http", 2010);
            TCPServer tCPServer = webFactory.CreateTcpServer("tcp", 2011);

            IWebEndpoint ep = webFactory.CreateDeviceRouterEndpoint(new DummyDeviceContainer(new IDevice[] { storage }));
            httpServer.AddEndpoint(ep);
            tCPServer.AddEndpoint(ep);
            httpServer.Start();
            tCPServer.Start();
            httpServer.WaitFor();
            tCPServer.WaitFor();

            PushNotificationRegistryItem item2 = new PushNotificationRegistryItem {
                Token = "t2", Group = "g1", IdentityName = "i1",
                ClientType = PushNotificationClientType.Apple
            };

            HttpMessage httpMessage = new HttpMessage {

                Destination = $"http://localhost:{2010}/{Settings.Consts.DeviceDestination()}",
                Payload = new DeviceRequest {
                    Action = "Register",
                    ActionType = DeviceRequest.ObjectActionType.Invoke,
                    Identifier = "storage",
                    Params = new object[] {item2}
                },

                ContentType = HttpMessage.DefaultContentType
            };

            IMessageClient httpClient = webFactory.CreateHttpClient("http_c");

            INetworkMessage httpResponse = httpClient.Send(httpMessage);
            Assert.AreEqual((int)NetworkStatusCode.Ok, httpResponse.Code);

            TCPClient client = webFactory.CreateTcpClient("tcp_c", "localhost", 2011);

            client.Start();
            client.Timeout = 100000;
            client.WaitFor();

            dynamic remoteStorage = new RemoteDevice("storage", System.Guid.Empty, client);

            IEnumerable<dynamic> item2Array = remoteStorage.Get(item2);

            Assert.AreEqual(2, item2Array.Count());

            PushNotificationRegistryItem item3 = new PushNotificationRegistryItem {
                Token = "t1", Group = "g2", IdentityName = "i1",
                ClientType = PushNotificationClientType.Apple
            };

            remoteStorage.Register(item3);

            IEnumerable<dynamic> item3Array = remoteStorage.Get(item3);

            Assert.AreEqual(1, item3Array.Count());

        }
    
    }

}
