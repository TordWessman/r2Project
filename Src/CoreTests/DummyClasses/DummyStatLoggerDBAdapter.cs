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
using System.Collections.Generic;
using R2Core.Common;
using System.Linq;
using System;

namespace R2Core.Tests {

    public class DummyStatLoggerDBAdapter : IStatLoggerDBAdapter {

        IList<StatLogEntry<dynamic>> Entries = new List<StatLogEntry<dynamic>>();

        public bool Ready { get; set; }
        public DateTime? MockTimestamp;

        public void ClearEntries(string identifier) {

            Entries = new List<StatLogEntry<dynamic>>();

        }

        public IEnumerable<StatLogEntry<T>> GetEntries<T>(string identifier) {

            IEnumerable<StatLogEntry<T>> res = Entries.Where(e => e.Identifier == identifier).Select(entry => new StatLogEntry<T> {
                Value = entry.Value,
                Identifier = entry.Identifier,
                Id = entry.Id,
                Timestamp = entry.Timestamp,
                Description = entry.Description
            });

            return res;
        }

        public void SaveEntry<T>(StatLogEntry<T> entry) {

            Entries.Add(new StatLogEntry<dynamic> {
                    Value = entry.Value,
                    Identifier = entry.Identifier,
                    Id = entry.Id,
                    Timestamp = MockTimestamp ?? entry.Timestamp,
                    Description = entry.Description
             });

        }

        public void SetUp() { }
    }
}
