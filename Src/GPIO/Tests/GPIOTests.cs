using System;
using Core.Tests;
using Core;
using NUnit.Framework;

namespace GPIO
{

	[TestFixture]
	public class GPIOTests : TestBase
	{
		private IGPIOFactory m_gpioFactory;

		public GPIOTests ()
		{
		}



		[TestFixtureSetUp]
		public void III() {
		
			//Console.WriteLine ("LOL");
			base.Setup ();

			try {

				m_gpioFactory = new GPIOFactory ("f", Settings.Paths.TestData ());

			} catch (Exception ex) {
			
				Console.WriteLine (ex.Message);

			}
		

		}

		[Test]
		public void TestInterpolation() {
		
			//Assert.True (false);
		}
	}
}

