﻿// This file is part of r2Poject.
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

        public StatLogger(string id, IStatLoggerDBAdapter adapter) : base (id) {

            m_adapter = adapter;

        }

        public void Log(IStatLoggable<double> device) { LogEntry(device.Identifier, device.Value); }

        public void Log(IStatLoggable<int> device) { LogEntry(device.Identifier, device.Value); }

        public void Log(IStatLoggable<float> device) { LogEntry(device.Identifier, device.Value); }

        private void LogEntry(string identifier, double value) {

            m_adapter.LogEntry(new StatLogEntry<double>() { Identifier = identifier, Value = value, Timestamp = DateTime.Now });

        }

        public IDictionary<string, IEnumerable<StatLogEntry<T>>> GetEntries<T>(IEnumerable<string> identifiers, DateTime? start = null, DateTime? end = null) {

            IDictionary<string, IEnumerable<StatLogEntry<T>>> entries = new Dictionary<string, IEnumerable<StatLogEntry<T>>>();

            foreach (string identifier in identifiers) {

                foreach (StatLogEntry<T> entry in m_adapter.GetEntries<T>(identifier)) {

                    if (!entries.ContainsKey(identifier)) {
                        entries[identifier] = new List<StatLogEntry<T>>();
                    }

                    (entries[identifier] as List<StatLogEntry<T>>).Add(entry);

                }

            }

            return entries;

        }

    }

}