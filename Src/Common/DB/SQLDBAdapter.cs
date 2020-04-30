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
using System.Collections.Generic;
using System.Linq;

namespace R2Core.Common {

    /// <summary>
    /// Base class for ISQLAdapter implementations. Contains template methods
    /// which concrete implementation uses to define the configuration of
    /// the database.
    /// </summary>
    public abstract class SQLDBAdapter : IDBAdapter {

        protected ISQLDatabase Database { get; private set; }

        protected abstract string GetTableName();

        /// <summary>
        /// Return a list of the required collumns in the format (i.e.: "name": "INTEGER NOT NULL").
        /// </summary>
        /// <returns>The columns.</returns>
        protected abstract IDictionary<string, string> GetColumns();

        protected abstract IEnumerable<string> GetPrimaryKeys();

        /// <summary>
        /// Return true if the autoincremented id parameter should be used.
        /// </summary>
        /// <returns><c>true</c>, if incremental identifier was used, <c>false</c> otherwise.</returns>
        protected virtual bool UseIncrementalIdentifier() { return true; }

        /// <summary>
        /// The column name of the auto incremented Id parameter
        /// </summary>
        public static readonly string IdParameterName = "id";

        public SQLDBAdapter(ISQLDatabase database) {

            Database = database;

        }

        public bool Ready => Database.Ready;

        public virtual void SetUp() {

            if (!Database.Ready) { throw new InvalidOperationException("Database not ready!"); }

            CreateTable();
            SynchronizeTable();

        }

        private void CreateTable() {

            Database.Query(CreateSQL);

        }

        private void SynchronizeTable() {

            IDictionary<string, string> newRows = new Dictionary<string, string>();

            IEnumerable<string> existingColumns = Database.GetColumns(GetTableName());

            foreach (string name in existingColumns) {

                if (!GetColumns().Keys.Contains(name) && name != IdParameterName) {

                    Log.w($"Warning: Unmapped column '{name}' in table '{GetTableName()}' in database with identifier: {Database.Identifier}. ");

                }

            }

            foreach (KeyValuePair<string, string> column in GetColumns()) {

                if (!existingColumns.Contains(column.Key)) {

                    newRows[column.Key] = column.Value;

                }

            }

            if (newRows.ContainsKey(IdParameterName)) {

                throw new ApplicationException($"Can't add column name '{IdParameterName}' since it's reserved for the autoincremental primary key. If UseIncrementalIdentifier() returns 'true', this column will be added automatically. ");

            }

            if (newRows.Count > 0) {

                foreach (KeyValuePair<string, string> column in newRows) {

                    string createSql = column.Value;

                    if (createSql.HasNotNull()) {

                        if (createSql.HasIllegalUpdateParameters()) {

                            throw new ApplicationException($"Unable to synchronize table. Collumn with key '{column.Key}' = '{createSql}' contains SQL parameters for ALTER table {GetTableName()}");

                        }

                        createSql = createSql.AddDefaultValueSql();

                    }

                    string sql = $@"ALTER TABLE {GetTableName()} ADD ""{column.Key}"" {createSql}";
                    Database.Query(sql);

                    Log.d($"Adding column {column.Key} to table {GetTableName()}");

                }

            }

        }

        public string CreateSQL {

            get {

                string sql = $@"CREATE TABLE IF NOT EXISTS ""{GetTableName()}"" (";

                if (UseIncrementalIdentifier()) {

                    sql += $@"""{IdParameterName}"" INTEGER PRIMARY KEY AUTOINCREMENT, ";

                }

                foreach (KeyValuePair<string, string> column in GetColumns()) {

                    if (column.Key == IdParameterName) {

                        throw new ApplicationException($"Can't add column name '{IdParameterName}' since it's reserved for the autoincremental primary key. If UseIncrementalIdentifier() returns 'true', this column will be added automatically. ");

                    }

                    sql += $@"""{column.Key}"" {column.Value}, ";

                }

                sql = sql.Substring(0, sql.Length - 2);


                if (GetPrimaryKeys().Any() && UseIncrementalIdentifier()) {

                    throw new ApplicationException("Only one primary key allowed: Primary keys and incremental identifier can't live together.");

                }

                if (GetPrimaryKeys().Any()) {

                    sql += ", PRIMARY KEY (";
                    foreach (string key in GetPrimaryKeys()) { sql += $@"{key}, "; }
                    sql = sql.Substring(0, sql.Length - 2);
                    sql += ")";

                }

                sql += ")";

                return sql;

            }

        }

        public string UpdateSQL(IDictionary<string, dynamic> values, string conditionsSQL) {

            string sql = $@"UPDATE ""{GetTableName()}"" SET ";

            foreach (var nameValue in values) {

                if (!GetColumns().ContainsKey(nameValue.Key)) {

                    throw new ArgumentException($"Invalid key '{nameValue.Key}' not found in Columns.");

                }

                sql += $"\"{nameValue.Key}\" = ";

                if (nameValue.Value is string || nameValue.Value is DateTime) {

                    sql += $@"""{nameValue.Value}"", ";

                } else { sql += $" {nameValue.Value}, "; }

            }
            sql = sql.Substring(0, sql.Length - 2);

            sql += $@" WHERE {conditionsSQL}";
            return sql;

        }

        public string DeleteSQL(string conditionsSQL = null) {

            string sql = $@"DELETE FROM {GetTableName()}";

            if (conditionsSQL != null) {

                sql += $" WHERE {conditionsSQL}";

            }

            return sql;

        }

        public string SelectSQL(string conditionsSQL = null) {

            string sql = $@"SELECT *";

            sql += $@" FROM {GetTableName()}";

            if (conditionsSQL != null) { sql += $" WHERE {conditionsSQL}"; }

            return sql;

        }

        public long Count(string conditionsSQL = null) {

            string sql = $"SELECT COUNT(*) FROM {GetTableName()}";

            if (conditionsSQL != null) {

                sql += $" WHERE {conditionsSQL}";

            }

            return Database.Count(sql);

        }

        /// <summary>
        /// Remove all rows and reset the id sequence.
        /// </summary>
        public void Truncate() {

            Database.Query(DeleteSQL());
            string sql = $"UPDATE SQLITE_SEQUENCE SET SEQ=0 WHERE NAME='{GetTableName()}'";
            Database.Query(sql);

        }

        public string InsertSQL(IEnumerable<dynamic> values) {

            string sql = $@"INSERT INTO ""{GetTableName()}"" (";

            foreach (string column in GetColumns().Keys) { sql += $@"""{column}"", "; }
            sql = sql.Substring(0, sql.Length - 2);

            sql += ") VALUES (";

            foreach (dynamic value in values) {

                if (value is string || value is DateTime) {

                    sql += $@" ""{value}"", ";

                } else { sql += $" {value}, "; }

            }

            sql = sql.Substring(0, sql.Length - 2);

            sql += ")";

            return sql.Replace(@"\", "");

        }

    }

}
