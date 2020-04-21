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

        protected override IEnumerable<string> GetPrimaryKeys() => new List<string>();

        public StatLoggerDBAdapter(ISQLDatabase database) : base(database) {}

        public void LogEntry(StatLogEntry<double> entry) {

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

            foreach(string identifier in identifiers) {

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

        internal static StatLogEntry<T> CreateEntry<T>(this DataRow self) {

            return new StatLogEntry<T> {
                Id = self.GetId(),
                Value = (T)self["value"],
                Timestamp = DateTime.Parse((string)self["timestamp"]),
                Identifier = (string)self["identifier"],
                Description = (string)(self["description"] ?? "")
            };

        }

    }

}
