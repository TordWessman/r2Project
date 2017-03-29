using System;
using NUnit.Framework;
using Core.Data;
using Core.Device;

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
		public void TestTemporaryMemory() {
		
			//IMemorySource
		}
	}
}

