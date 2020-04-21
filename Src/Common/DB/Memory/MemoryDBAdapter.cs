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
using R2Core.Data;
using System.Collections.Generic;
using System.Data;
using MemoryType = System.String;

namespace R2Core.Common
{
	public class MemoryDBAdapter : SQLDBAdapter, IMemoryDBAdapter {

        protected override string GetTableName() => throw new NotImplementedException();
        protected override IDictionary<string, string> GetColumns() => throw new NotImplementedException();
        protected override IEnumerable<string> GetPrimaryKeys() => throw new NotImplementedException();

        private const string MEMORY_TABLE_NAME = "memory";
		private const string ID_COL = "id";
		private const string NAME_COL = "value";
		private const string TYPE_COL = "type";
		
		private const string CREATE_TABLE_SQL = "CREATE  TABLE  IF NOT EXISTS \"{0}\" (\"{1}\" INTEGER PRIMARY KEY  NOT NULL  UNIQUE , \"{2}\" VARCHAR NOT NULL , \"{3}\" VARCHAR NOT NULL )";
		private const string INSERT_ROW_SQL = "INSERT INTO \"{0}\" (\"{1}\",\"{2}\",\"{3}\") VALUES (\"{4}\",\"{5}\",\"{6}\")";
		private const string UPDATE_ROW_SQL = "UPDATE \"{0}\" SET \"{1}\" = \"{2}\", \"{3}\" = \"{4}\" WHERE {5}={6}";
		private const string SELECT = "SELECT * FROM  \"{0}\" WHERE \"{1}\"=\"{2}\"";
		private const string SELECT_MANY = "SELECT * FROM  \"{0}\" WHERE \"{1}\" IN ({2})";
		private const string SELECT_ALL_SQL = "SELECT * FROM  \"{0}\"";
		
		private const string DELETE_SQL = "DELETE FROM  \"{0}\" WHERE {1}={2}";
		
		public MemoryDBAdapter(ISQLDatabase db) : base (db) {

		
		}

		public ICollection<IMemoryReference> Get(string type) {

			ICollection<IMemoryReference> memories = new List<IMemoryReference>();
			string sql = string.Format(SELECT, MEMORY_TABLE_NAME, TYPE_COL, type);

			if (!Database.Ready) {

				throw new InvalidOperationException("Database not ready!");

			}

			DataSet ds = Database.Select(sql);
			DataTable dt = ds.Tables[0];

			foreach (DataRow row in dt.Rows) {

				memories.Add(CreateReference(row));

			}

			return memories;

		}

		public ICollection<IMemoryReference> Get(int[] ids) {

			ICollection<IMemoryReference> memories = new List<IMemoryReference>();
			string sql = string.Format(SELECT_MANY, MEMORY_TABLE_NAME, ID_COL, String.Join(",", ids));

			if (!Database.Ready) {

				throw new InvalidOperationException("Database not ready!");

			}

			DataSet ds = Database.Select(sql);
			DataTable dt = ds.Tables[0];

			foreach (DataRow row in dt.Rows) {

				memories.Add(CreateReference(row));

			}

			return memories;

		}
	
		public bool Update(IMemoryReference reference) {
			string sql = string.Format(
				UPDATE_ROW_SQL,
				MEMORY_TABLE_NAME,
				NAME_COL,
				reference.Value.Replace("\"", ""),
				TYPE_COL,
				reference.Type.Replace("\"", ""),
				ID_COL,
				reference.Id);

			if (!Database.Ready) {

				throw new InvalidOperationException("Database not ready!");

			}

			return Database.Query(sql) > 0;

		}
		
		public override void SetUp() {

			string sql = string.Format(CREATE_TABLE_SQL,
			                           	MEMORY_TABLE_NAME,
			                           	ID_COL,
			                           	NAME_COL,
			                            TYPE_COL);

			if (!Database.Ready) {
			
				throw new InvalidOperationException("Database not ready!");
		
			}
			
			Database.Query(sql);
		
		}
		
		public void Create(IMemoryReference reference) {

			string sql = string.Format(
				INSERT_ROW_SQL,
			    MEMORY_TABLE_NAME,
			    ID_COL,
			    NAME_COL,
			    TYPE_COL,
			    reference.Id,
				reference.Value.Replace("\"", ""),
				reference.Type.Replace("\"", ""));
				
			if (!Database.Ready) {

				throw new InvalidOperationException("Database not ready!");
			
			}
			
			Database.Insert(sql);
		
		}
		
		public ICollection<IMemoryReference> LoadAll() {
			
			ICollection<IMemoryReference> memories = new List<IMemoryReference>();
			string sql = string.Format(SELECT_ALL_SQL, MEMORY_TABLE_NAME);
			
			if (!Database.Ready) {

				throw new InvalidOperationException("Database not ready!");
			
			}
			
			DataSet ds = Database.Select(sql);
			DataTable dt = ds.Tables[0];
			
			foreach (DataRow row in dt.Rows) {
				
				memories.Add(CreateReference(row));
			
			}

			return memories;
		
		}
		
		public bool Delete(int memoryId) {

			string sql = string.Format(
				DELETE_SQL,
			    MEMORY_TABLE_NAME,
			    ID_COL,
				memoryId);
			
			if (!Database.Ready) {
			
				throw new InvalidOperationException("Database not ready!");
			
			}
			
			int result = Database.Query(sql);
			
			if (result > 0) {
			
				return true;
			
			}
			
			return false;
		
		}

		private IMemoryReference CreateReference(DataRow row) {
		
			return new MemoryReference(
				(int)((long)row[ID_COL]),
				(string)row[NAME_COL],
				(string)row[TYPE_COL] 
			);

		}
	
	}

}

