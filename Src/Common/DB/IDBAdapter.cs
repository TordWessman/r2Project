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

namespace R2Core.Common
{
    /// <summary>
    /// An object acting as an semi-complete abstraction layer over a database.
    /// </summary>
	public interface IDBAdapter {

        /// <summary>
        /// Returns ´true´ if the adapter is ready for commands.
        /// </summary>
        /// <value><c>true</c> if ready; otherwise, <c>false</c>.</value>
		bool Ready { get; }

        /// <summary>
        /// Configure the database. This includes creating the tables if necessary. 
        /// </summary>
		void SetUp();
	
    }

}