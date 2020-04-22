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
using System.IO;
using System.Data;
using R2Core.Device;
using System.Collections.Generic;

namespace R2Core.Common
{

	public class SqliteDatabase : DeviceBase, ISQLDatabase {

		private readonly string m_fileName;
		private SqliteConnection m_connection;

		private readonly object m_queryLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:R2Core.Common.SqliteDatabase"/> class.
        /// By setting ´fileName´ parameter to ´null´, the database will live in memory only.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="fileName">File name.</param>
		public SqliteDatabase(string id, string fileName) : base(id) {

            m_fileName = fileName;

            if (fileName == null) { Log.i($"Creating an in-memory database only for database with identifier: '{id}'"); }

		}
		
		public override bool Ready { 

			get {

				//TODO: check this
				return m_connection != null;// && m_con.State == ConnectionState.Open
			
			}
		
		}
		
		public override void Start() {

			lock(m_queryLock) {
				
                if (m_fileName == null) {

                    m_connection = new SqliteConnection("Data Source=:memory:");

                } else {

                    if (!File.Exists(m_fileName)) {

                        SqliteConnection.CreateFile(m_fileName);

                    }

                    if (m_connection != null) {

                        throw new InvalidOperationException("Unable to sqlite instance - one instance for this file name already started.");

                    }

                    m_connection = new SqliteConnection($"URI=file:{m_fileName}");
                   
                }

                m_connection.Open();

            }
			
		}
		
		public override void Stop() {

			lock(m_queryLock) {

				if (m_connection != null) {
				
					m_connection.Close();
				
				}
				
				m_connection = null;
			
			}
		
		}
		
		public DataSet Select(string queryString) {

			lock(m_queryLock) {

				if (m_connection == null) {

					throw new InvalidOperationException("Sqlite Database not started.");
				
				}
				
				DataSet ds = new DataSet();
				
				using(SqliteDataAdapter adapter = new SqliteDataAdapter(queryString, m_connection)) {
	
					adapter.Fill(ds);
	
				}
				
				return ds;

			}
			
		}

        public int Count(string queryString) {

            using (SqliteCommand cmd = new SqliteCommand(queryString, m_connection)) {

               return Convert.ToInt32(cmd.ExecuteScalar());

            }

        }

        public int Query(string queryString) {

			lock(m_queryLock) {

				if (m_connection == null) {

					throw new InvalidOperationException("Sqlite Database not started.");
				
				}
				
				int rows = 0;
			
				using(SqliteCommand cmd = new SqliteCommand(queryString, m_connection)) {
				
					rows = cmd.ExecuteNonQuery();                                                                   
				
				}
			
				return rows;
			
			}

		}
		
		public long Insert(string queryString) {

			lock(m_queryLock) {

				if (m_connection == null) {

					throw new InvalidOperationException("Sqlite Database not started.");
				
				}
			
				using(SqliteCommand cmd = new SqliteCommand(queryString, m_connection)) {

					cmd.ExecuteNonQuery();                                                                   
				
				}
			
				using(SqliteCommand cmd = new SqliteCommand(@"select last_insert_rowid()", m_connection)) {
				
					return (long)cmd.ExecuteScalar();                                                         
				
				}

			}

		}

        public IEnumerable<string> GetColumns(string tableName) {

            IList<string> names = new List<string>();
            DataSet columns = Select($"PRAGMA table_info({tableName})");

            foreach (DataRow row in columns.Tables[0].Rows) {

                yield return (string)row["name"];
            
            }
        
        }

        public void Delete(string tableName) {

            Query($"DROP TABLE IF EXISTS {tableName}");

        }

    }

}