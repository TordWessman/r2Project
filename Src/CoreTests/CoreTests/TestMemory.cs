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

			Assert.AreEqual(m1.Type, "foo");
			Assert.AreEqual(m1.Value, "bar");
		
			IMemory m2 = memorySource.Create("a", "b");

			m1.Associate(m2);

			Assert.NotNull(m1.Associations.Where(m => m == m2).FirstOrDefault());

			m2.Value = "c";
			Assert.AreEqual(m2.Value, "c");

			Assert.AreEqual(m1.Associations.Where(m => m == m2).FirstOrDefault()?.Value, "c");

			Assert.True(memorySource.Delete(m2));

			foreach (IMemory ass in m1.Associations) {
			
				Assert.False(ass == m2);

			}

		}
	}
}

