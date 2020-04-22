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
using System.Linq;
using R2Core.Network;
using System.Collections.Generic;

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

			DeviceRequest wob = new DeviceRequest { 
				Identifier = "dummy_device",
				ActionType = DeviceRequest.ObjectActionType.Invoke,
				Action = "MultiplyByTen",
				Params = new object[] { 42 }
			};

			byte[] serialized = serializer.Serialize(wob);

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
			dynamic ros = invoker.Invoke(d, "OneParam", new List<dynamic> {9999} );

			Assert.IsNull(res);
			Assert.IsNull(ros);
		}

		[Test]
		public void TestDynamicInvoker() {
			PrintName();

			var invoker = new ObjectInvoker();
			var o = new InvokableDummy();

			var res = invoker.Invoke(o, "AddBar", new List<object> {"foo"});

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
			IEnumerable<object> aStringArray = new List<object>{ "foo", "bar" };
			int count = invoker.Invoke(o, "GiveMeAnArray", new List<object> {aStringArray});
			Assert.AreEqual(aStringArray.Count(), count);

			IEnumerable<object> OneTwoThree = new List<object> { 1, 2, 3 };
			invoker.Set(o, "EnumerableInts", OneTwoThree);

			for (int i = 0; i < OneTwoThree.Count(); i++) {
			
				Assert.AreEqual(OneTwoThree.ToList()[i], o.EnumerableInts.ToList()[i]);

			}

			//Test invoking an IDictionary<,>
			IDictionary<object, object> dictionary = new Dictionary<object,object>();
			foreach (int i in OneTwoThree) {
			
				dictionary[$"apa{i}"] = i;
			}

			IEnumerable<int> allValues = invoker.Invoke(o, "GiveMeADictionary", new List<object> {dictionary});

			for (int i = 0; i < OneTwoThree.Count(); i++) {
			
				Assert.AreEqual(allValues.ToArray()[i], OneTwoThree.ToArray()[i]);

			}

			//var dummyDevice = new DummyDevice();

		}


    }

}