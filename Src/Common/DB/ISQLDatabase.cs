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
using System.Data;
using R2Core.Device;


namespace R2Core.Common
{

    /// <summary>
    /// Implementations of SQL databases.
    /// </summary>
	public interface ISQLDatabase : IDevice {
		
		/// <summary>
		/// Returns a data set containing query result.
		/// </summary>
		/// <param name="queryString">Query string.</param>
		DataSet Select(string queryString);

		/// <summary>
		/// Execute query and returns the number of rows affected.
		/// </summary>
		/// <param name="queryString">Query string.</param>
		int Query(string queryString);

		/// <summary>
		/// Execute an insert statement and returns the id of the inserted row. Can also be used for create statements.
		/// </summary>
		/// <param name="queryString">Query string.</param>
		long Insert(string queryString);

        /// <summary>
        /// Executes a COUNT statement and return the number of rows.
        /// </summary>
        /// <returns>The count.</returns>
        /// <param name="queryString">Query string.</param>
        int Count(string queryString);

        /// <summary>
        /// Returns an array of the column names.
        /// </summary>
        /// <returns>The columns.</returns>
        /// <param name="tableName">Table name.</param>
        IEnumerable<string> GetColumns(string tableName);

        /// <summary>
        /// Removes a table with name ´tableName´
        /// </summary>
        /// <param name="tableName">Name of table to be removed.</param>
        void Delete(string tableName);
    }

}

