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
using Core.Data;
using System.Data;

namespace Core.Memory
{
	public class AssociationsDBAdapter : IAssociationsDBAdapter
	{
		private IDatabase m_db;
		
		private const string ASSOCIATIONS_TABLE_NAME = "associations";
		private const string ID_COL_ONE = "id_1";
		private const string ID_COL_TWO = "id_2";
		
		private const string CREATE_TABLE_SQL = "CREATE  TABLE  IF NOT EXISTS \"{0}\" (\"{1}\" INTEGER NOT NULL, \"{2}\" INTEGER NOT NULL, PRIMARY KEY ( \"{1}\",  \"{2}\") )";
		private const string INSERT_ROW_SQL = "INSERT INTO \"{0}\" (\"{1}\",\"{2}\") VALUES (\"{3}\",\"{4}\")";

		private const string SELECT_SQL = "SELECT {0} FROM  \"{1}\" WHERE {2}={3}";
		
		private const string DELETE_SQL = "DELETE FROM {0} WHERE {1}={3} OR {2}={3}";
		
		public AssociationsDBAdapter (IDatabase db)
		{
			m_db = db;
		}

		#region IDBAdapter implementation
		public void SetUp ()
		{
			string sql = string.Format (CREATE_TABLE_SQL,
			                           	ASSOCIATIONS_TABLE_NAME,
			                           	ID_COL_ONE,
			                           	ID_COL_TWO);
			if (!m_db.Ready) {
				throw new InvalidOperationException ("Database not ready!");
			}
			
			m_db.Update (sql);
		}

		public bool Ready {
			get {
				return m_db.Ready;
			}
		}
		#endregion

		#region IAssociationsDBAdapter implementation
		public void SetAssociation (int memoryId1, int memoryId2)
		{
			
			if (memoryId1 == memoryId2) {
				throw new ArgumentException ("Unable to create Association - memory1 == memory2: " + memoryId1);
			}
			
			foreach (int assocId in GetAssociations(memoryId1)) {
				if (assocId == memoryId2) {
					//Log.w ($"Will not add association for {memoryId1} and {memoryId2} since they already are associated.");
					return;
				}
			}
			
			//lalala make sure memoryId1 is the lowest...
			if (memoryId1 < memoryId2) {
				int temp = memoryId1;
				memoryId1 = memoryId2;
				memoryId2 = temp;
			}
			
			string sql = string.Format (INSERT_ROW_SQL,
			                           	ASSOCIATIONS_TABLE_NAME,
			                           	ID_COL_ONE,
			                           	ID_COL_TWO,
			                            memoryId1,
			                            memoryId2);
			if (!m_db.Ready) {
				throw new InvalidOperationException ("Database not ready!");
			}
			
			m_db.Insert (sql);
			
		}

		public int[] GetAssociations (int memoryId)
		{
			//ICollection<int> memories = new List<int>();
			
			if (!m_db.Ready) {
				throw new InvalidOperationException ("Database not ready!");
			}
			
			string sql = string.Format (SELECT_SQL,
			                           	ID_COL_ONE,
			                            ASSOCIATIONS_TABLE_NAME,
			                           	ID_COL_TWO,
			                            memoryId);
			
			DataSet dsId1 = m_db.Select (sql);
			
			sql = string.Format (SELECT_SQL,
			                           	ID_COL_TWO,
			                            ASSOCIATIONS_TABLE_NAME,
			                           	ID_COL_ONE,
			                            memoryId);
			
			DataSet dsId2 = m_db.Select (sql);
			
			
			DataTable dt1 = dsId1.Tables [0];
			DataTable dt2 = dsId2.Tables [0];
			
			int [] result = new int[dt1.Rows.Count + dt2.Rows.Count];
			
			int i = 0;
			
			foreach (DataRow row in dt1.Rows) {
				result [i++] = (int)(long)row [ID_COL_ONE];
			}

			foreach (DataRow row in dt2.Rows) {
				result [i++] = (int)(long)row [ID_COL_TWO];
			}
			
			
			return result;
			
		}
		
		public void RemoveAssociations (int memoryId)
		{
			string sql = string.Format (DELETE_SQL,
			                           	ASSOCIATIONS_TABLE_NAME,
			                           	ID_COL_ONE,
			                           	ID_COL_TWO,
			                            memoryId);
			
			if (!m_db.Ready) {
				throw new InvalidOperationException ("Database not ready!");
			}
			
			m_db.Update (sql);
			
		}
		
		
		#endregion
	}
}

