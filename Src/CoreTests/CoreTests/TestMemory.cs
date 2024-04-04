using System;
using NUnit.Framework;
using R2Core.Device;
using System.Linq;
using R2Core.Common;

namespace R2Core.Tests
{
	[TestFixture]
	public class TestMemory: TestBase {

		[Test]
		public void TestTemporaryMemory() {
		
			IMemorySource memorySource = new TemporaryMemorySource("s");

			IMemory m1 = memorySource.Create("foo", "bar");

			Assert.Equals(m1.Type, "foo");
			Assert.Equals(m1.Value, "bar");
		
			IMemory m2 = memorySource.Create("a", "b");

			m1.Associate(m2);

			Assert.That(m1.Associations.FirstOrDefault(m => m == m2) != null);

			m2.Value = "c";
			Assert.Equals(m2.Value, "c");

			Assert.Equals(m1.Associations.FirstOrDefault(m => m == m2)?.Value, "c");

			Assert.That(memorySource.Delete(m2));

			foreach (IMemory ass in m1.Associations) {
			
				Assert.That(ass != m2);

			}

		}
	}
}

