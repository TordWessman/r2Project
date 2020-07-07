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

namespace R2Core.Common {

    /// <summary>
    /// Represents a data point in the ´StatLogger´ context.
    /// </summary>
    public struct StatLogEntry<T> {

        /// <summary>
        /// Id of the log entry.
        /// </summary>
        public long Id;

        /// <summary>
        /// The identifier of the object being logged.
        /// </summary>
        public string Identifier;

        /// <summary>
        /// Time of log entry creation.
        /// </summary>
        public DateTime Timestamp;

        /// <summary>
        /// The value being logged
        /// </summary>
        public T Value;

        /// <summary>
        /// An (optional) description of the logged value.
        /// </summary>
        public string Description;

        /// <summary>
        /// A user friendly name of the device related to the entry. This value will not be persisted.
        /// </summary>
        public string Name;

        public override string ToString() {
            return $"StatLogEntry<{typeof(T)}>: [Identifier: {Identifier}, Name: {Name ?? Identifier}, Value: {Value}, Timestamp: {Timestamp}" + 
            (Description != null ? $", Description: {Description}]>" : "]");
        }
    }

}
