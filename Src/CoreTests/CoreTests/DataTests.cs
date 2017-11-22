using System;
using NUnit.Framework;
using Core.Data;
using Core.Device;
using System.Linq;
using System.Dynamic;
using Core.Network.Web;
using System.Collections.Generic;

namespace Core.Tests
{
	[TestFixture]
	public class DataTests: TestBase
	{
		
		private ISerialization serializer;

		[TestFixtureSetUp]
		public override void Setup() {
		
			base.Setup ();

			serializer = m_dataFactory.CreateSerialization ("serializer", System.Text.Encoding.UTF8);

		}
		public DataTests ()
		{
		}
		[Test]
		public void TestInt32Converter() {
		
			Int32Converter converter = new Int32Converter (256 * 256 * 20 + 256 * 10 + 42);

			Assert.AreEqual (42, converter[0]);
			Assert.AreEqual (10, converter[1]);
			Assert.AreEqual (20, converter[2]);
			Assert.AreEqual (3, converter.Length);

			byte[] bytes = { 42, 10, 20 };

			Int32Converter converter2 = new Int32Converter (bytes);

			Assert.AreEqual (converter2.Value, converter.Value);
			Assert.AreEqual (converter2.Length, converter.Length);

		}

		[Test]
		public void TestLinearDataSet() {
		
			ILinearDataSet<double> dataSet = m_dataFactory.CreateDataSet ("test_device.csv");

			Assert.AreEqual (dataSet.Points.Keys.ElementAt (0), 1.0d);
			Assert.AreEqual (dataSet.Points.Values.ElementAt (0), 10.0d);

			Assert.AreEqual (dataSet.Points.Keys.ElementAt (2), 3.0d);
			Assert.AreEqual (dataSet.Points.Values.ElementAt (2), 30.0d);

			Assert.AreEqual (dataSet.Interpolate (0.5), 10.0d);
			Assert.AreEqual (dataSet.Interpolate (100), 30.0d);

			Assert.AreEqual (dataSet.Interpolate (1.5), 15.0d);
			Assert.AreEqual (dataSet.Interpolate (2.9), 29.0d);

		}

		class TestSer {
		
			public string Foo;
			public int Bar;

		}

		[Test]
		public void TestSerialization() {

			TestSer t = new TestSer ();

			t.Foo = "bar";
			t.Bar = 42;

			byte[] serialized = serializer.Serialize (t);

			dynamic r = serializer.Deserialize (serialized);

			Assert.AreEqual ("bar", r.Foo);
			Assert.AreEqual (42, r.Bar);

		}

		[Test]
		public void TestSerializeWithEnum() {
		
			WebObjectRequest wob = new WebObjectRequest () { 
				Identifier = "dummy_device",
				ActionType = WebObjectRequest.ObjectActionType.Invoke,
				Action = "MultiplyByTen",
				Params = new object[] { 42 }
			};

			byte [] serialized = serializer.Serialize (wob);

			dynamic deserialized = serializer.Deserialize (serialized);

			Assert.AreEqual ("dummy_device", deserialized.Identifier);
			Assert.AreEqual (deserialized.Action, "MultiplyByTen");
			Assert.AreEqual (deserialized.ActionType, 2);


		}

		class Invokable {
		
			public int Member;

			private string m_propertyMember;
			public string Property { get { return m_propertyMember; } set { m_propertyMember = value;} }

			public string AddBar(string value) {
			
				return value + "bar";

			}
		}

		[Test]
		public void TestDynamicInvoker() {

			var invoker = new ObjectInvoker ();
			var o = new Invokable ();

			var res = invoker.Invoke(o, "AddBar", new List<object>() {"foo"});

			Assert.AreEqual ("foobar", res);

			invoker.Set (o, "Member", 42);
			Assert.AreEqual (42, o.Member);

			invoker.Set (o, "Property", "42");
			Assert.AreEqual ("42", o.Property);
			Assert.AreEqual ("42", invoker.Get(o,"Property"));
			Assert.AreEqual (42, invoker.Get(o,"Member"));
			Assert.IsTrue (invoker.ContainsPropertyOrMember (o, "Member"));
			Assert.IsFalse (invoker.ContainsPropertyOrMember (o, "NotAMember"));

		}

	}

}