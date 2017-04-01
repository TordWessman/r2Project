using System;
using NUnit.Framework;
using Core.Data;
using Core.Device;
using System.Linq;

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
	}
}

