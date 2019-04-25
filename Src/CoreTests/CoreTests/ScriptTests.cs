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
using R2Core.Device;
using R2Core.Network;
using R2Core.Scripting;
using System.Collections.Generic;
using R2Core.Common;

namespace R2Core.Tests
{
	
	[TestFixture]
	public class ScriptTests: TestBase {
		
		private IScriptFactory<IronScript> m_rubyScriptFactory;
		private IScriptFactory<IronScript> m_pythonScriptFactory;
		private IScriptFactory<LuaScript> m_luaScriptFactory;

		[TestFixtureSetUp]
		public override void Setup() {

			base.Setup();

			Log.t("mamma");
			Log.t("L: " + BaseContainer.RubyPaths.Count);

			foreach (string path in BaseContainer.RubyPaths) {
			
				Log.t(path);

			}

			m_rubyScriptFactory = new RubyScriptFactory("rf", BaseContainer.RubyPaths, m_deviceManager);
			m_rubyScriptFactory.AddSourcePath(Settings.Paths.TestData());

			m_pythonScriptFactory = new PythonScriptFactory("rf", BaseContainer.PythonPaths , m_deviceManager);
			m_pythonScriptFactory.AddSourcePath(Settings.Paths.TestData());
			m_pythonScriptFactory.AddSourcePath(Settings.Paths.Common());

			m_luaScriptFactory = new LuaScriptFactory("ls");
			m_luaScriptFactory.AddSourcePath(Settings.Paths.TestData());

		}

		[Test]
		public void RubyTest1() {
	
			dynamic ruby = m_rubyScriptFactory.CreateScript("RubyTest1");
			Assert.NotNull(ruby);

			Assert.AreEqual(ruby.set_up_foo(), "baz");

			Assert.AreEqual("bar", (string)ruby.foo);

			ruby.bar = 42;

			Assert.AreEqual(42, ruby.Get("bar"));

		}

		[Test]
		public void LuaTest1() {
		
			dynamic lua = m_luaScriptFactory.CreateScript("LuaTest1");
		
			Assert.AreEqual("fish", lua.str);

		}

		[Test]
		public void PythonTests() {
		
			dynamic python = m_pythonScriptFactory.CreateScript("python_test");

			Assert.AreEqual(142, python.add_42 (100));

			python.katt = 99;

			Assert.AreEqual(99, python.katt);

			Assert.AreEqual(99 * 10 , python.return_katt_times_10());

			python.dog_becomes_value("foo");

			Assert.AreEqual("foo", python.dog);
		
		}
	
	}
}

