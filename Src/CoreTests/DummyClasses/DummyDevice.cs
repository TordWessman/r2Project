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
using R2Core.Device;
using NUnit.Framework;
using System.Collections.Generic;
using R2Core.Common;

namespace R2Core.Tests
{
    /// <summary>
    /// An IDevice implementation used by various test cases.
    /// </summary>
	public class DummyDevice : DeviceBase, IStatLoggable<int> {
		
		private bool m_isRunning;
        private int m_value;

		public DummyDevice(string id): base(id) { }

        public override bool Ready => m_isRunning;

		public override void Stop() {
			
			m_isRunning = false;
		
		}

		public override void Start() {

			m_isRunning = true;

		}

        public bool ValueDidChange { get; private set; }
        public int Value { 
            get { return m_value;  }
            set { 
                m_value = value; 
                ValueDidChange = true; 
                } 
            }

		public string Bar { get; set; }

        public double HAHA;

		public float GiveMeFooAnd42AndAnObject(string foo, int _42, dynamic anObject) {
		
			Bar = foo;
			Assert.AreEqual("Foo", foo);
			Assert.AreEqual(42, _42);
			Assert.AreEqual("Dog", anObject.Cat);
			return 12.34f;

		}

		public IDictionary<string,string> AddCatToKeysAnd42ToValues(IDictionary<string, string> dictionary) {

			IDictionary<string,string> result = new Dictionary<string, string>();

			foreach (var kvp in dictionary) {
			
				result [kvp.Key + "Cat"] = $"{kvp.Value}42";
			
			}

			return result;

		}

		public void NoParamsNoNothing() {
		}

		public void OneParam(int param) {

		}

		public int MultiplyByTen(int value) {

			return value * 10;

		}
    }
}

