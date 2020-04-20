using System;
using NUnit.Framework;
using R2Core.Data;
using R2Core.Device;
using System.Linq;
using System.Dynamic;
using R2Core.Network;
using System.Collections.Generic;
using R2Core.Common;
using System.Data;

namespace R2Core.Tests
{
	[TestFixture]
	public class DataTests: TestBase {
		
		private ISerialization serializer;

		[TestFixtureSetUp]
		public override void Setup() {
		
			base.Setup();

			serializer = new JsonSerialization("serializer", System.Text.Encoding.UTF8);

		}

		[Test]
		public void TestInt32Converter() {
			PrintName();

			Int32Converter converter = new Int32Converter(256 * 256 * 20 + 256 * 10 + 42);

			Assert.AreEqual(42, converter[0]);
			Assert.AreEqual(10, converter[1]);
			Assert.AreEqual(20, converter[2]);
			Assert.AreEqual(3, converter.Length);

			byte[] bytes = { 42, 10, 20 };

			Int32Converter converter2 = new Int32Converter(bytes);

			Assert.AreEqual(converter2.Value, converter.Value);
			Assert.AreEqual(converter2.Length, converter.Length);

		}

		[Test]
		public void TestLinearDataSet() {
			PrintName();

			ILinearDataSet<double> dataSet = m_dataFactory.CreateDataSet("test_device.csv");

			Assert.AreEqual(dataSet.Points.Keys.ElementAt(0), 1.0d);
			Assert.AreEqual(dataSet.Points.Values.ElementAt(0), 10.0d);

			Assert.AreEqual(dataSet.Points.Keys.ElementAt(2), 3.0d);
			Assert.AreEqual(dataSet.Points.Values.ElementAt(2), 30.0d);

			Assert.AreEqual(dataSet.Interpolate(0.5), 10.0d);
			Assert.AreEqual(dataSet.Interpolate(100), 30.0d);

			Assert.AreEqual(dataSet.Interpolate(1.5), 15.0d);
			Assert.AreEqual(dataSet.Interpolate(2.9), 29.0d);

		}

		class TestSer {
		
			public string Foo;
			public int Bar;

		}

		[Test]
		public void TestSerialization() {
			PrintName();

			TestSer t = new TestSer();

			t.Foo = "bar";
			t.Bar = 42;

			byte[] serialized = serializer.Serialize(t);

			dynamic r = serializer.Deserialize(serialized);

			Assert.AreEqual("bar", r.Foo);
			Assert.AreEqual(42, r.Bar);

		}

		[Test]
		public void TestSerializeWithEnum() {
			PrintName();

			DeviceRequest wob = new DeviceRequest() { 
				Identifier = "dummy_device",
				ActionType = DeviceRequest.ObjectActionType.Invoke,
				Action = "MultiplyByTen",
				Params = new object[] { 42 }
			};

			byte [] serialized = serializer.Serialize(wob);

			dynamic deserialized = serializer.Deserialize(serialized);

			Assert.AreEqual("dummy_device", deserialized.Identifier);
			Assert.AreEqual(deserialized.Action, "MultiplyByTen");
			Assert.AreEqual(deserialized.ActionType, 2);


		}

		[Test]
		public void TestSimpleObjectInvoker() {
			PrintName();

			ObjectInvoker invoker = new ObjectInvoker();

			DummyDevice d = new DummyDevice("duuumm");
			dynamic res = invoker.Invoke(d, "NoParamsNoNothing", null);
			dynamic ros = invoker.Invoke(d, "OneParam", new List<dynamic>() {9999} );

			Assert.IsNull(res);
			Assert.IsNull(ros);
		}

		[Test]
		public void TestDynamicInvoker() {
			PrintName();

			var invoker = new ObjectInvoker();
			var o = new InvokableDummy();

			var res = invoker.Invoke(o, "AddBar", new List<object>() {"foo"});

			Assert.AreEqual("foobar", res);

			invoker.Set(o, "Member", 42);
			Assert.AreEqual(42, o.Member);

			invoker.Set(o, "Property", "42");

			Assert.AreEqual("42", o.Property);
			Assert.AreEqual("42", invoker.Get(o,"Property"));
			Assert.AreEqual(42, invoker.Get(o,"Member"));
			Assert.IsTrue(invoker.ContainsPropertyOrMember(o, "Member"));
			Assert.IsFalse(invoker.ContainsPropertyOrMember(o, "NotAMember"));

			//Test invoking a IEnumerable<>
			IEnumerable<object> aStringArray = new List<object>() { "foo", "bar" };
			int count = invoker.Invoke(o, "GiveMeAnArray", new List<object>() {aStringArray});
			Assert.AreEqual(aStringArray.Count(), count);

			IEnumerable<object> OneTwoThree = new List<object>() { 1, 2, 3 };
			invoker.Set(o, "EnumerableInts", OneTwoThree);

			for (int i = 0; i < OneTwoThree.Count(); i++) {
			
				Assert.AreEqual(OneTwoThree.ToList() [i], o.EnumerableInts.ToList() [i]);

			}

			//Test invoking an IDictionary<,>
			IDictionary<object, object> dictionary = new Dictionary<object,object>();
			foreach (int i in OneTwoThree) {
			
				dictionary [$"apa{i}"] = i;
			}

			IEnumerable<int> allValues = invoker.Invoke(o, "GiveMeADictionary", new List<object>() {dictionary});

			for (int i = 0; i < OneTwoThree.Count(); i++) {
			
				Assert.AreEqual(allValues.ToArray() [i], OneTwoThree.ToArray() [i]);

			}

			//var dummyDevice = new DummyDevice();

		}

        [Test]
        public void TestDatabase() {
            PrintName();
            DataFactory factory = new DataFactory("factory");
            ISQLDatabase database = factory.CreateSqlDatabase("db", "dummy_db");

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

            Assert.AreEqual(2, database.Insert(adapter.InsertSQL(new List<dynamic>() {
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

            Assert.AreEqual(3, database.Insert(adapter.InsertSQL(new List<dynamic>() {
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

            database.Query(adapter.UpdateSQL(new Dictionary<string, dynamic>() {
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
            ISQLDatabase database = factory.CreateSqlDatabase("db", "dummy_db");
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
                database.Insert(adapter.InsertSQL(new List<dynamic>() {
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
    
    }

}