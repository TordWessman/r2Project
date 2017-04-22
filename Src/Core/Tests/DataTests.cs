using System;
using NUnit.Framework;
using Core.Data;
using Core.Device;
using System.Linq;
using System.Dynamic;

namespace Core.Tests
{
	[TestFixture]
	public class DataTests: TestBase
	{
		
		private IR2Serialization serializer;

		[TestFixtureSetUp]
		public override void Setup() {
		
			base.Setup ();

			serializer = m_dataFactory.CreateSerialization ("serializer", System.Text.Encoding.UTF8);

		}
		public DataTests ()
		{
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
		public void TestStringSerialization() {

			string jsonString = "{ \"Value\": 42, \"Params\": [1,2,\"Hund\"] }";
			byte[] jsonBytes = serializer.Encoding.GetBytes (jsonString);

			dynamic o = serializer.Deserialize (jsonBytes);

			Assert.AreEqual (42, o.Value);
			Assert.AreEqual (1, o.Params[0]);
			Assert.AreEqual (2, o.Params[1]);
			Assert.AreEqual ("Hund", o.Params[2]);
		}
	}
}

