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
using System.Data;
using System.Linq;

namespace R2Core.Common {

    internal static class SqlExtensions {

        /// <summary>
        /// Return true if a DEFAULT parameter is required.
        /// </summary>
        internal static bool HasNotNull(this string self) {

            return self.ToUpper().Contains("NOT NULL") && !self.ToUpper().Contains("DEFAULT");

        }

        /// <summary>
        /// Returns true sql contains keywords that is not allowed for alteration of table.
        /// </summary>
        /// <returns><c>true</c>, if illegal update parameters was hased, <c>false</c> otherwise.</returns>
        /// <param name="self">Self.</param>
        internal static bool HasIllegalUpdateParameters(this string self) {

            return  self.ToUpper().Contains("PRIMARY KEY") ||
                    self.ToUpper().Contains("UNIQUE");

        }

        /// <summary>
        /// Add the DEFAULT value parameter to a sql string specifying the properties of a column.
        /// </summary>
        /// <returns>The default value sql.</returns>
        /// <param name="self">Self.</param>
        internal static string AddDefaultValueSql(this string self) {

            string sql = self.ToUpper();

            string datatype = sql.Split(' ').FirstOrDefault();

            sql += @" DEFAULT ";

            if (datatype == "TEXT" || datatype == "BLOB") {

                sql += @"('')";

            } else {

                sql += @"(0)";

            }

            return sql;

        }

        /// <summary>
        /// The format which Sqlite uses for string conversions of DateTime.
        /// </summary>
        /// <returns>The date format.</returns>
        public static string SqliteDateFormat() { return "yyyy-MM-dd HH:mm:ss"; }

        /// <summary>
        /// Return a timestamp formatted string.
        /// </summary>
        /// <returns>The SQL ite timestamp.</returns>
        /// <param name="self">Self.</param>
        internal static string AsSQLiteTimestamp(this DateTime self) {

            return self.ToString(SqliteDateFormat());

        }

    }

    public static class SqlDataRowExtensions {

        /// <summary>
        /// Parse a column with name ´columName´ as datetime.
        /// </summary>
        public static DateTime GetDateTime(this DataRow self, string columnName) {

            return DateTime.Parse((string)self[columnName]);

        }

        public static long GetId(this DataRow self) {

            return (long)self[SQLDBAdapter.IdParameterName];

        }

    }

}
