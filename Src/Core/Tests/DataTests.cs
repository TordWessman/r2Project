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
		private DataFactory m_factory;
	
		[TestFixtureSetUp]
		public override void Setup() {
		
			base.Setup ();

			m_factory = new DataFactory("f", new System.Collections.Generic.List<string>() {Settings.Paths.TestData()});
				

		}
		public DataTests ()
		{
		}

		[Test]
		public void TestLinearDataSet() {
		
			ILinearDataSet<double> dataSet = m_factory.CreateDataSet ("test_device.csv");

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

			IR2Serialization serializer = new R2DynamicJsonSerialization ();

			byte[] serialized = serializer.Serialize (t);

			dynamic r = serializer.Deserialize (serialized);

			Assert.AreEqual ("bar", r.Foo);
			Assert.AreEqual (42, r.Bar);

		}
	}
}

