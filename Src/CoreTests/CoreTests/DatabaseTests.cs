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
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using R2Core.Common;
using System.Data;
using System.Timers;
using System.Threading;
using R2Core.Device;
using R2Core.Network;
using System.Globalization;

namespace R2Core.Tests {

    [TestFixture]
    public class DatabaseTests : TestBase {

        [TestFixtureSetUp]
        public override void Setup() {

            base.Setup();

        }


        [Test]
        public void TestDatabase() {
            PrintName();
            DataFactory factory = new DataFactory("factory");
            ISQLDatabase database = factory.CreateTemporaryDatabase();

            database.Start();

            database.Delete("dummy_db");

            DummyDBAdapter adapter = factory.CreateDatabaseAdapter<DummyDBAdapter>(database);

            // Dummy only: set the values of the collumns.
            adapter.Values = new Dictionary<string, string> {
                { "value", "REAL NOT NULL" },
                { "text", "TEXT" },
                { "timestamp", "TEXT NOT NULL" }
            };

            // Make sure the table is created
            adapter.SetUp();

            Assert.AreEqual(1, database.Insert(adapter.InsertSQL(new List<dynamic> {
                42,  "Hej",  DateTime.Now
            })));

            Assert.AreEqual(2, database.Insert(adapter.InsertSQL(new List<dynamic> {
                43, "Nej",  DateTime.MinValue
            })));

            Assert.AreEqual(2, adapter.Count());
            Assert.AreEqual(1, adapter.Count("value = 43"));

            DataSet allRows = database.Select(adapter.SelectSQL());
            Assert.AreEqual(2, allRows.Tables[0].Rows.Count);

            DataSet oneRow = database.Select(adapter.SelectSQL("value = 42"));
            Assert.AreEqual(1, oneRow.Tables[0].Rows.Count);

            Assert.AreEqual("Hej", oneRow.Tables[0].Rows[0]["text"]);

            // Test parse DateTime
            DateTime timestamp = DateTime.Parse((string)oneRow.Tables[0].Rows[0]["timestamp"]);
            Assert.Less(DateTime.Now, timestamp.Add(new TimeSpan(1, 0, 0)));

            // Test delete
            database.Query(adapter.DeleteSQL("value = 44"));
            Assert.AreEqual(2, adapter.Count());

            database.Query(adapter.DeleteSQL("value = 43"));
            Assert.AreEqual(1, adapter.Count());

            Assert.AreEqual(3, database.Insert(adapter.InsertSQL(new List<dynamic> {
                45, "Hej", DateTime.Now
            })));

            // id should have kept increasing
            allRows = database.Select(adapter.SelectSQL());
            Assert.AreEqual(3, allRows.Tables[0].Rows[1].GetId());

            // Clear the table.
            adapter.Truncate();
            Assert.AreEqual(0, adapter.Count());

            // Id increment should have reset
            Assert.AreEqual(1, database.Insert(adapter.InsertSQL(new List<dynamic> {
                99,  "Yay",  DateTime.Now
            })));

            Assert.AreEqual(2, database.Insert(adapter.InsertSQL(new List<dynamic> {
                100,  "Yay",  DateTime.Now
            })));

            Assert.AreEqual(3, database.Insert(adapter.InsertSQL(new List<dynamic> {
                101,  "Yo",  DateTime.Now
            })));

            DataSet notUpdated = database.Select(adapter.SelectSQL("value = 42"));

            Assert.AreEqual(0, notUpdated.Tables[0].Rows.Count);

            // Test update

            database.Query(adapter.UpdateSQL(new Dictionary<string, dynamic> {
                { "value", 42 },
                { "timestamp", DateTime.MinValue }
            }, @"text = ""Yay"""));

            DataSet updated = database.Select(adapter.SelectSQL("value = 42"));

            DataRow row = updated.Tables[0].Rows[0];

            Assert.AreEqual(2, updated.Tables[0].Rows.Count);
            Assert.AreEqual(DateTime.MinValue, row.GetDateTime("timestamp"));

        }

        [Test]
        public void TestDatabaseAlter() {
            PrintName();
            DataFactory factory = new DataFactory("factory");
            ISQLDatabase database = factory.CreateTemporaryDatabase();
            database.Start();
            database.Delete("dummy_db");

            DummyDBAdapter adapter = factory.CreateDatabaseAdapter<DummyDBAdapter>(database);

            // Dummy only: set the values of the collumns.
            adapter.Values = new Dictionary<string, string> {
                { "value", "REAL NOT NULL" },
                { "text", "TEXT" }
            };

            // not created
            Assert.AreEqual(0, database.GetColumns("dummy_db").Count());

            // Make sure the table is created
            adapter.SetUp();

            // 2 values + id
            Assert.AreEqual(3, database.GetColumns("dummy_db").Count());

            for (int i = 0; i < 10; i++) {
                database.Insert(adapter.InsertSQL(new List<dynamic> {
                    i, "Hej"
                }));
            }

            // Add a new column 'timestamp'
            adapter.Values = new Dictionary<string, string> {
                { "value", "REAL NOT NULL" },
                { "text", "TEXT" },
                { "timestamp", "TEXT NOT NULL" }
            };

            adapter.SetUp();

            // 3 values + id
            Assert.AreEqual(4, database.GetColumns("dummy_db").Count());

            DataSet allRows = database.Select(adapter.SelectSQL());
            Assert.AreEqual("", allRows.Tables[0].Rows[1]["timestamp"]);

        }

        [Test]
        public void TestStatLoggerSimple() {
            PrintName();
            DummyStatLoggerDBAdapter adapter = new DummyStatLoggerDBAdapter();
            StatLogger logger = new StatLogger("stat", adapter);

            DummyDevice d1 = new DummyDevice("d1");
            DummyDevice d2 = new DummyDevice("d2");

            DummyDevice d3 = new DummyDevice("d3");

            int count = 100;
            for (int i = 0; i < count; i++) {

                d1.Value = i;
                d2.Value = count - i;

                // 49 % of the entries are logged in the past and 50 % in the future
                adapter.MockTimestamp = DateTime.Now.AddHours(i - count / 2);
                logger.Log(d1);
                logger.Log(d2);

            }

            Assert.AreEqual(count, logger.GetEntries(new string[] { "d1" })["d1"].Count());
            Assert.AreEqual(count, logger.GetEntries(new string[] { "d2" })["d2"].Count());
            var everything = logger.GetEntries(new string[] { "d1", "d2" });
            Assert.AreEqual(count * 2, everything["d1"].Count() + everything["d2"].Count());

            // Ignoring all "past" entries
            Assert.AreEqual(count / 2 - 1, logger.GetEntries(new string[] { "d1" }, DateTime.Now)["d1"].Count());

            // Ignoring "past" and 50 % of the future entries.
            Assert.AreEqual(count / 2 - count / 4, logger.GetEntries(new string[] { "d1" }, DateTime.Now, DateTime.Now.AddHours(count / 4))["d1"].Count());


            // Test most recent:
            d3.Value = 10;
            adapter.MockTimestamp = DateTime.ParseExact("2011-03-21 13:26:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            logger.Log(d3);


            d3.Value = 42;
            adapter.MockTimestamp = DateTime.ParseExact("2011-03-22 13:26:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            logger.Log(d3);

            d3.Value = 100;
            adapter.MockTimestamp = DateTime.ParseExact("2011-03-20 13:26:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            logger.Log(d3);

            var entry = logger.GetMostRecent("d3");

            Assert.AreEqual(42, entry.Value);

        }

        [Test]
        public void TestStatLoggerProcess() {
            PrintName();

            DummyStatLoggerDBAdapter adapter = new DummyStatLoggerDBAdapter();
            StatLogger logger = new StatLogger("stat", adapter);

            adapter.DeubugOutput = true;
            DummyDevice d1 = new DummyDevice("d1");

            // test untracking a device not tracked
            logger.Untrack(d1);

            int count = 5;      // number of records expected
            int interval = 200 ; // interval in ms between records

            // timer to stop the tracking
            System.Timers.Timer stopTimer = new System.Timers.Timer(count * interval - interval / 2);

            stopTimer.Elapsed += delegate { logger.Untrack(d1); };

            logger.Track(d1, interval * (1.0f / (60.0f * 1000f)));
            stopTimer.Enabled = true;
            stopTimer.Start();

            // wait until after the count + 1 tracking could have occured.
            Thread.Sleep((count + 1) * interval);

            IDictionary<string, IEnumerable<StatLogEntry<double>>> entries = logger.GetEntries(new string[] { "d1" });

            Assert.AreEqual(count, entries["d1"].Count());

            // Try with start time:

            stopTimer.Dispose();
            DummyDevice d2 = new DummyDevice("d2");

            stopTimer = new System.Timers.Timer(count * interval + interval / 2);

            stopTimer.Elapsed += delegate {

                logger.Untrack(d1);
                logger.Untrack(d2);
            
            };

            // Track d1 as before
            logger.Track(d1, interval);
            // d2 should start after 2 "ticks"
            logger.Track(d2, interval, DateTime.Now.AddMilliseconds(interval * (count - 2)));
            stopTimer.Enabled = true;
            stopTimer.Start();

            Thread.Sleep((count + 3) * interval);

            entries = logger.GetEntries(new string[] { "d1", "d2" });

            // d1 should have added ´count´ + 1 for the first recording
            Assert.AreEqual(count + 1, entries["d1"].Count());
            // d2 should have started later and only have ´count - 2´ entries.
            Assert.AreEqual(count - 2, entries["d2"].Count());

            // Test parsing string as start time:
            stopTimer.Dispose();

            stopTimer = new System.Timers.Timer(count * interval);
            stopTimer.Elapsed += delegate { logger.Untrack(d1); };
            string startTime = DateTime.Now.AddMilliseconds(interval).ToString("HH:mm:ss fff");
            logger.TrackFrom(d1, interval, startTime);
            stopTimer.Enabled = true;
            stopTimer.Start();

            // wait until after the count + 1 tracking could have occured.
            Thread.Sleep((count + 1) * interval);

            stopTimer.Enabled = false;
            stopTimer.Dispose();
            // d1 should have added ´count´ new entries
            entries = logger.GetEntries(new string[] { "d1" });
            Assert.AreEqual(count * 3 + 1, entries["d1"].Count());

            // Test logger.Stop()
            stopTimer = new System.Timers.Timer(count * interval);

            stopTimer.Elapsed += delegate { logger.Stop(); };
            logger.Track(d1, interval);
            logger.Track(d2, interval);
            stopTimer.Enabled = true;
            stopTimer.Start();

            // wait until after the count + 1 tracking could have occured.
            Thread.Sleep((count + 1) * interval);

            // d1 should have added ´count´ + 1 new entries.
            entries = logger.GetEntries(new string[] { "d1", "d2" });
            Assert.AreEqual(count * 4 + 1 + 1, entries["d1"].Count());

            // d2 should have added ´count´ + 1 new entries.
            Assert.AreEqual(count * 2 - 2 + 1, entries["d2"].Count());

            stopTimer.Dispose();


        }

        [Test]
        public void TestRemoteStatLogger() {
            PrintName();

            DummyStatLoggerDBAdapter adapter = new DummyStatLoggerDBAdapter();
            StatLogger logger = new StatLogger("logger", adapter);

            DummyDevice d1 = new DummyDevice("d1");
            IDeviceManager dm = new DeviceManager("dm");
            dm.Add(d1);
            dm.Add(logger);

            // ------------ Configure network -----------------
            ISerialization serialization = new JsonSerialization("ser");
            TCPPackageFactory packageFactory = new TCPPackageFactory(serialization);
            WebFactory webFactory = new WebFactory("factory", serialization);

            // Set up server
            TCPServer server = webFactory.CreateTcpServer("tcp", 7676);
            server.AddEndpoint(webFactory.CreateDeviceRouterEndpoint(dm));
            server.Start();
            server.WaitFor();

            // Set up client
            IMessageClient client = webFactory.CreateTcpClient("client", "localhost", 7676);
            client.Start();
            client.WaitFor();

            // Create remote device
            dynamic remoteLogger = new RemoteDevice("logger", logger.Guid, client);

            // -------- Log entries ------------
            // Log 5 entries fairly present
            for (int i = 0; i < 5; i++) { logger.Log(d1); }

            // Log 4 entries from the past
            adapter.MockTimestamp = DateTime.Now.AddDays(-2);
            for (int i = 0; i < 4; i++) { logger.Log(d1); }

            // Log 3 entries from the future
            adapter.MockTimestamp = DateTime.Now.AddDays(2);
            for (int i = 0; i < 3; i++) { logger.Log(d1); }

            // -------- Check entries locally ----------

            // Fetch all entries
            Assert.AreEqual(5 + 4 + 3, logger.GetEntries<double>(new string[] { "d1"})["d1"].Count());

            // Fetch entries from today
            // Format "now" to a start date that is parseable. It should include 5 from today and 3 from tomorrow.
            string startDate = DateTime.Now.Date.ToString(StatLoggerDateParsingExtensions.GetStatLoggerDateFormat());
            string endDate = null;
            IEnumerable<dynamic> entries = (IEnumerable<dynamic>)remoteLogger.GetValues(new string[] { "d1" }, startDate, endDate)["d1"];

            Assert.AreEqual(5 + 3, entries.Count());

            // Fetch entries until today
            // Format "now" to an end date that is parseable. It should include 5 from today and 4 from yesterday.
            startDate = null;
            endDate = DateTime.Now.Date.AddDays(1).ToString(StatLoggerDateParsingExtensions.GetStatLoggerDateFormat());
            entries = (IEnumerable<dynamic>)remoteLogger.GetValues(new string[] { "d1" }, startDate, endDate)["d1"];

            Assert.AreEqual(5 + 4, entries.Count());

            // Log 2 entries from tomorrow
            adapter.MockTimestamp = DateTime.Now.Date.AddDays(1);
            for (int i = 0; i < 2; i++) { logger.Log(d1); }

            startDate = DateTime.Now.Date.ToString(StatLoggerDateParsingExtensions.GetStatLoggerDateFormat());
            endDate = DateTime.Now.Date.AddDays(1).ToString(StatLoggerDateParsingExtensions.GetStatLoggerDateFormat());
            entries = (IEnumerable<dynamic>)remoteLogger.GetValues(new string[] { "d1" }, startDate, endDate)["d1"];

            // 5 from today and 2 from tomorrow.
            Assert.AreEqual(5 + 2, entries.Count());

        }

        [Test]
        public void TestStatLoggerWithDatabase() {
            PrintName();

            DataFactory factory = new DataFactory("factory");
            ISQLDatabase database = factory.CreateTemporaryDatabase();
            database.Start();
            IStatLoggerDBAdapter adapter = factory.CreateDatabaseAdapter<StatLoggerDBAdapter>(database);
            adapter.SetUp();
            StatLogger logger = new StatLogger("stat", adapter);

            IEnumerable<StatLogEntry<double>> entries = logger.GetValues(new string[] { "d1" }, null, null)["d1"];

            Log.t(entries.Count());
            Assert.AreEqual(0, entries.Count());

            DummyDevice d1 = new DummyDevice("d1");
            d1.Value = 42;

            logger.Log(d1);

            entries = logger.GetValues(new string[] { "d1" }, null, null)["d1"];

            Assert.AreEqual(1, entries.Count());

            // Description has not been set
            Assert.AreEqual("", entries.First().Description);

            // Name has not been set and should be equal to Identifier
            Assert.AreEqual(d1.Identifier, entries.First().Name);

            logger.SetDescription(d1, "Din mamma");
            logger.DeviceNames[d1.Identifier] = "Pappa"; // Make all "d1" get a Name property

            IEnumerable<StatLogEntry<int>> entriesInt = logger.GetEntries<int>(new string[] { "d1" })["d1"];
            IEnumerable<StatLogEntry<float>> entriesFloat = logger.GetEntries<float>(new string[] { "d1" })["d1"];

            Assert.AreEqual("Din mamma", entriesInt.First().Description);
            Assert.AreEqual("Pappa", entriesInt.First().Name);

            Assert.AreEqual(1, entriesInt.Count());
            Assert.AreEqual(1, entriesFloat.Count());

            Assert.AreEqual(42, entries.First().Value);
            Assert.AreEqual(42, entriesInt.First().Value);
            Assert.AreEqual(42, entriesFloat.First().Value);

        }

    }

}
