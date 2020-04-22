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

namespace R2Core.Common {

    /// <summary>
    /// Implementations can have their values logged in a ´StatLogger´.
    /// </summary>
    public interface IStatLoggable<T> : IDevice {

        /// <summary>
        /// The value to be logged.
        /// </summary>
        /// <value>The value to be logged.</value>
        T Value { get; }

    }

    public static class StatLoggerExtension {

        public static void Log(this StatLogger self, IStatLoggable<float> device) { self.Log(device); }
        public static void Log(this StatLogger self, IStatLoggable<double> device) { self.Log(device); }
        public static void Log(this StatLogger self, IStatLoggable<int> device) { self.Log(device); }
        public static void Log(this StatLogger self, IStatLoggable<byte> device) { self.Log(device); }

        public static IDictionary<string, IEnumerable<StatLogEntry<double>>> GetEntries(this StatLogger self, IEnumerable<string> identifiers, DateTime? start = null, DateTime? end = null) {

            return self.GetEntries<double>(identifiers, start, end);

        }

    }

}