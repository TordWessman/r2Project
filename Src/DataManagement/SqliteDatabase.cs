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
using Mono.Data.Sqlite;
using Mono.Data;
using System.IO;
using System.Collections.Generic;
using System.Data;
using R2Core.Device;

namespace R2Core.DataManagement
{
	public class SqliteDatabase : DeviceBase, IDatabase
	{
		private string m_fileName;
		private SqliteConnection m_con;

		private readonly object m_queryLock = new object();

		/// <summary>
		/// Initializes a new instance of the <see cref="Core.DB.SqliteDatabase"/> class. If fileName is not found, the database file will be created.
		/// </summary>
		/// <param name="fileName">File name.</param>
		public SqliteDatabase (string id, string fileName) : base(id) 
		{

			m_fileName = fileName;

		}
		
		public override bool Ready { 

			get {

				//TODO: check this
				return m_con != null;// && m_con.State == ConnectionState.Open
			
			}
		
		}
		
		public override void Start ()
		{

			lock (m_queryLock) {
				
				if (!File.Exists (m_fileName)) {

					SqliteConnection.CreateFile (m_fileName);
				
				}

				if (m_con != null) {

					throw new InvalidOperationException ("Unable to sqlite instance - one instance for this file name already started.");
				
				}
			
				try {

					string cs = "URI=file:" + m_fileName;

					m_con = new SqliteConnection (cs);
					m_con.Open ();
					
				} finally {
				}

			}
			
		}
		
		public override void Stop ()
		{
			lock (m_queryLock) {

				if (m_con != null) {
				
					m_con.Close ();
				
				}
				
				m_con = null;
			
			}
		
		}
		
		public DataSet Select (string queryString)
		{

			lock (m_queryLock) {

				if (m_con == null) {

					throw new InvalidOperationException ("Sqlite Database not started.");
				
				}
				
				DataSet ds = new DataSet ();
				
				using (SqliteDataAdapter adapter = new SqliteDataAdapter (queryString, m_con)) {
	
					adapter.Fill (ds);
	
				}
				
				return ds;

			}
			
		}
		
		public int Update (string queryString)
		{

			lock (m_queryLock) {

				if (m_con == null) {

					throw new InvalidOperationException ("Sqlite Database not started.");
				
				}
				
				int rows = 0;
			
				using (SqliteCommand cmd = new SqliteCommand(queryString, m_con)) {
				
					rows = cmd.ExecuteNonQuery ();                                                                   
				
				}
			
				return rows;
			
			}

		}
		
		public Int64 Insert (string queryString)
		{
			lock (m_queryLock) {

				if (m_con == null) {

					throw new InvalidOperationException ("Sqlite Database not started.");
				
				}
			
				using (SqliteCommand cmd = new SqliteCommand(queryString, m_con)) {

					cmd.ExecuteNonQuery ();                                                                   
				
				}
			
				using (SqliteCommand cmd = new SqliteCommand(@"select last_insert_rowid()", m_con)) {
				
					return (Int64)cmd.ExecuteScalar ();                                                         
				
				}

			}

		}

	}

}