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

using System;
using System.Data;

/// <summary>
/// Irrational implementation.
/// </summary>
using R2Core.Device;


namespace R2Core.Data
{
	public interface IDatabase : IDevice
	{
		/// <summary>
		/// Returns a data set containing query result.
		/// </summary>
		/// <param name="queryString">Query string.</param>
		DataSet Select (string queryString);

		/// <summary>
		/// Returns the number of rows affected
		/// </summary>
		/// <param name="queryString">Query string.</param>
		int Update(string queryString);

		/// <summary>
		/// Execute an insert statement and returns the id of the inserted row. Can also be used for create statements.
		/// </summary>
		/// <param name="queryString">Query string.</param>
		Int64 Insert (string queryString);

	}

}

