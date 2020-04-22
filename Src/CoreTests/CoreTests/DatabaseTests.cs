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

        }
    }
}
