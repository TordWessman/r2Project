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

namespace R2Core.Common
{
	public interface IDBAdapter {

		bool Ready { get; }

		void SetUp();
	
    }

    public abstract class DBAdapter : IDBAdapter {

        protected ISQLDatabase Database { get; private set; }

        protected abstract string GetTableName();

        protected abstract IDictionary<string, string> GetColumns();

        protected abstract IEnumerable<string> GetPrimaryKeys();

        public DBAdapter(ISQLDatabase database) {

            Database = database;
        
        }

        public bool Ready => Database.Ready;

        public virtual void SetUp() {

            if (!Database.Ready) { throw new InvalidOperationException("Database not ready!"); }

            Database.Update(CreateSQL);

        }

        protected string CreateSQL { get {

            string sql = $"CREATE TABLE IF NOT EXIST \"{GetTableName()}\" (";

            foreach (var column in GetColumns()) { sql += $"\"{column.Key}\" {column.Value}, "; }
            sql = sql.Substring(0, sql.Length - 3);

            if (GetPrimaryKeys().Count() > 0) {

                sql += ", PRIMARY KEY (";
                foreach(string key in GetPrimaryKeys()) {  sql += $"{key}, "; }
                sql = sql.Substring(0, sql.Length - 3);
                sql += ")";

            }

            sql += ")";

            return sql;

       } }

        public string UpdateSQL(IDictionary<string, string> values, string conditionsSQL) {

            string sql = $"UPDATE \"{GetTableName()}\" SET ";

            foreach (var value in values) {

                if (!GetColumns().ContainsKey(value.Key)) {

                    throw new ArgumentException($"Invalid key '{value.Key}' not found in Columns.");

                }

                sql += $"\"{value.Key}\" = \"{value.Value}\", ";

            }
            sql = sql.Substring(0, sql.Length - 3);

            sql += $" WHERE {conditionsSQL}";
            return sql;
        }

        protected string DeleteSQL(string conditionsSQL) {

            return $"DELETE FROM {GetTableName()} WHERE {conditionsSQL}";

        }

        protected string SelectSQL(string conditionsSQL) {

            string sql = $"SELECT ";
            foreach (string column in GetColumns().Keys) { sql += $"{column}, "; }
            sql = sql.Substring(0, sql.Length - 3);

            sql += $" FROM {GetTableName()} WHERE {conditionsSQL}";
            return sql;

        }

        protected string InsertSQL(IEnumerable<string> values) {

            string sql = $"INSERT INTO \"{GetTableName()}\" (";

            if (values.Count() != GetColumns().Count) {

                throw new ArgumentException($"Invalid parameter count. Required {GetColumns().Count}. Got {values.Count()}");

            }

            foreach (string column in GetColumns().Keys) { sql += $"\"{column}\", "; }
            sql = sql.Substring(0, sql.Length - 3);

            sql += ") VALUES (";
            foreach (string value in values) { sql += $" \"{value}\", "; }

            sql = sql.Substring(0, sql.Length - 3);

            sql += ")";

            return sql;
        }


    }
}

