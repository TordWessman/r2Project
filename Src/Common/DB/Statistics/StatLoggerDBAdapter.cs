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
using System.Data;

namespace R2Core.Common {

    public class StatLoggerDBAdapter : SQLDBAdapter, IStatLoggerDBAdapter {

        protected override string GetTableName() => $"{Database.Identifier}";

        protected override IDictionary<string, string> GetColumns() => new Dictionary<string, string> {
                { "value", "REAL NOT NULL" },
                { "type", "TEXT NOT NULL" },
                { "timestamp", "TEXT NOT NULL" },
                { "identifier", "TEXT NOT NULL" },
                { "description", "TEXT" }
            };

        public StatLoggerDBAdapter(ISQLDatabase database) : base(database) {}

        public void SaveEntry<T>(StatLogEntry<T> entry) {

            IList<dynamic> values = new List<dynamic> {
                entry.Value,
                $"{entry.GetType().GetGenericArguments()[0]}",
                $"{entry.Timestamp.AsSQLiteTimestamp()}",
                $"{entry.Identifier}",
                $"{entry.Description ?? ""}",
            };

            string sql = InsertSQL(values);

            if (!Database.Ready) { throw new InvalidOperationException("Database not ready!"); }

            Database.Insert(sql);

        }

        public void ClearEntries(string identifier) {

            string sql = DeleteSQL($@"identifier = ""{identifier}""");

            if (!Database.Ready) { throw new InvalidOperationException("Database not ready!"); }

            Database.Query(sql);

        }

        public void SetDescription(string identifier, string description) {

            string sql = UpdateSQL(new Dictionary<string, dynamic> { { "description", description} }, $@"identifier = ""{identifier}""");
            Database.Query(sql);

        }

        /// <summary>
        /// Returns as numerical values ordered by their identifiers
        /// </summary>
        /// <returns>The data points.</returns>
        /// <param name="identifiers">Identifier.</param>
        public IDictionary<string, IEnumerable<StatLogEntry<double>>> GetDataPoints(IEnumerable<string> identifiers) {

            IDictionary<string, IEnumerable<StatLogEntry<double>>> points = new Dictionary<string, IEnumerable<StatLogEntry<double>>>();

            foreach (string identifier in identifiers) {

                points[identifier] = GetEntries<double>(identifier);
            
            }

            return points;

        }

        public IEnumerable<StatLogEntry<T>>GetEntries<T>(string identifier) {

            string sql = SelectSQL($@"identifier = ""{identifier}""");

            if (!Database.Ready) { throw new InvalidOperationException("Database not ready!"); }

            DataSet result = Database.Select(sql);

            foreach (DataRow row in result.Tables[0].Rows) {

                yield return row.CreateEntry<T>();

            }

        }

    }

    internal static class DataRowCollectionExtension {

        /// <summary>
        /// Creates a StatLogEntry with the Value parameter of type T from a DataRow.
        /// </summary>
        /// <returns>The entry.</returns>
        /// <param name="self">Self.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        internal static StatLogEntry<T> CreateEntry<T>(this DataRow self) {

            T value = default(T);

            // This is the rather idiosyncratic strategy i came up with to cast System.Single, which is the type of the
            // "value" parameter returned by the DB
            if (typeof(T) == typeof(double)) {

                value = (T)(dynamic)double.Parse(self["value"].ToString());

            } else if (typeof(T) == typeof(int)) {

                value = (T)(dynamic)int.Parse(self["value"].ToString());

            } else if (typeof(T) == typeof(long)) {

                value = (T)(dynamic)long.Parse(self["value"].ToString());

            } else if (typeof(T) == typeof(byte)) {

                value = (T)(dynamic)byte.Parse(self["value"].ToString());

            } else {

                value = (T)self["value"];

            }

            return new StatLogEntry<T> {
                Id = self.GetId(),
                Value = value,
                Timestamp = DateTime.Parse((string)self["timestamp"]),
                Identifier = (string)self["identifier"],
                Description = (string)(self["description"] ?? "")
            };

        }

    }

}
