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
//
using System;
using NUnit;
using NUnit.Framework;
using Core.Device;
using Core.Network;
using Core.Scripting;

namespace Core.Tests
{
	
	[TestFixture]
	public class ScriptTests: TestBase
	{
		
		private IScriptFactory<RubyScript> m_scriptFactory;

		[TestFixtureSetUp]
		public override void Setup() {

			base.Setup ();

			m_scriptFactory = new RubyScriptFactory ("sf", Settings.Paths.TestData (), BaseContainer.DEFAULT_RUBY_PATHS, m_deviceManager, m_dummyTaskMonitor);

		}

		[Test]
		public void RubyTest1() {
	
			dynamic ruby = m_scriptFactory.CreateScript ("RubyTest1");
			Assert.NotNull (ruby);

			Assert.AreEqual (ruby.set_up_foo (), "baz");

			Assert.AreEqual ("bar", (string) ruby.foo);

			ruby.bar = 42;

			Assert.AreEqual (42, ruby.Get ("bar"));

		}
	
	}
}

