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
using System.Collections.Generic;
using System.Linq;
using R2Core.Device;

namespace R2Core.Tests
{
    struct InvokableNestedStruct {

        public string AString;
    
    }

    struct InvokableDecodableDummy {

        public int AnInt;
        public IEnumerable<string> SomeStrings;

        public InvokableNestedStruct Nested;

    }

    class InvokableDummy : DeviceBase {

		public int Member = 0;

		public string Property { get; set; }

        public InvokableDecodableDummy Decodable { get; set; }
        public IEnumerable<int> EnumerableInts = null;

        public InvokableDummy(string id = null) : base (id ?? "invokable_dummy") { }

        public string AddBar(string value) {

			return value + "bar";

		}

        public void SetDecodable(InvokableDecodableDummy obj) {

            Decodable = obj;

        }


        public int GiveMeAnArray(IEnumerable<string> array) {

			return array.Count();

		}

		public IEnumerable<int> GiveMeADictionary(IDictionary<string,int> dictionary) {

			IEnumerable<int> values = dictionary.Values.ToArray();
			return values;

		}

	}

}

