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
using System.Collections.Generic;
using R2Core.Device;
using System.Linq;

namespace R2Core.Common {

    public class StatLogger : DeviceBase {

        private IStatLoggerDBAdapter m_adapter;

        /// <summary>
        /// The types of IStatLoggable that has logging supported.
        /// </summary>
        public readonly Type[] AllowedTypes = { typeof(int), typeof(float), typeof(byte), typeof(double) };

        public StatLogger(string id, IStatLoggerDBAdapter adapter) : base (id) {

            m_adapter = adapter;

        }

        public void Log<T>(IStatLoggable<T> device) {

            if (!AllowedTypes.Contains(typeof(T))) {

                throw new NotImplementedException($"Logging a value of type {typeof(T)} is not yet implemented.");
            
            }


            LogEntry(device.Identifier, Convert.ToDouble(device.Value));

        }

        private void LogEntry(string identifier, double value) {

            m_adapter.SaveEntry(new StatLogEntry<double> { Identifier = identifier, Value = value, Timestamp = DateTime.Now });

        }

        public IDictionary<string, IEnumerable<StatLogEntry<T>>> GetEntries<T>(IEnumerable<string> identifiers, DateTime? start = null, DateTime? end = null) {

            IDictionary<string, IEnumerable<StatLogEntry<T>>> entries = new Dictionary<string, IEnumerable<StatLogEntry<T>>>();

            foreach (string identifier in identifiers) {

                if (!entries.ContainsKey(identifier)) {

                    entries[identifier] = new List<StatLogEntry<T>>();

                }

                foreach (StatLogEntry<T> entry in m_adapter.GetEntries<T>(identifier)) {

                    if (start != null && start > entry.Timestamp) { continue; }
                    if (end != null && end < entry.Timestamp) { continue; }

                    (entries[identifier] as List<StatLogEntry<T>>).Add(entry);

                }

            }

            return entries;

        }

    }

}
