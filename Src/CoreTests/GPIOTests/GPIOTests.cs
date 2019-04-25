using System;
using R2Core.Tests;
using R2Core;
using NUnit.Framework;

namespace R2Core.GPIO
{

	[TestFixture]
	public class GPIOTests : TestBase {
		
		[TestFixtureSetUp]
		public void III() {
		
			//Console.WriteLine("LOL");
			base.Setup();

			try {

				//m_gpioFactory = new GPIOFactory("f", Settings.Paths.TestData());

			} catch (Exception ex) {
			
				Console.WriteLine(ex.Message);

			}
		

		}

		[Test]
		public void TestInterpolation() {
		
			//Assert.True(false);
		}
	}
}

