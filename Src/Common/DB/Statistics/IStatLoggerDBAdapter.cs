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

namespace R2Core.Common {

    /// <summary>
    /// The interface for the StatLogger to the database.
    /// </summary>
    public interface IStatLoggerDBAdapter : IDBAdapter {

        /// <summary>
        /// Adds an entry row to the database.
        /// </summary>
        /// <param name="entry">Entry.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        void SaveEntry<T>(StatLogEntry<T> entry);

        /// <summary>
        /// Clear all entries using device identifier ´identifier´
        /// </summary>
        /// <param name="identifier">Identifier.</param>
        void ClearEntries(string identifier);

        /// <summary>
        /// Return all entries using device identifier ´identifier´
        /// </summary>
        /// <returns>The entries.</returns>
        /// <param name="identifier">Identifier.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        IEnumerable<StatLogEntry<T>> GetEntries<T>(string identifier);

        /// <summary>
        ///  Set the ´description´ property of all entries using device identifier ´identifier´.
        /// </summary>
        /// <param name="identifier">Identifier.</param>
        /// <param name="description">Description.</param>
        void SetDescription(string identifier, string description);

    }

}
