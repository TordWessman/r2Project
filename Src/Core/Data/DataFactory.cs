using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Device;

namespace Core.Data
{
	/// <summary>
	/// Use this factory to create data containers
	/// </summary>
	public class DataFactory: DeviceBase
	{
		IEnumerable<string> m_searhPaths;

		public DataFactory (string id, IEnumerable<string> searchPaths = null) : base(id)
		{
			
			m_searhPaths = searchPaths ?? new List<string> ();

		}


		/// <summary>
		/// Loads a data set from the specified file. The file format should be of a simple CSV type containing x/y coordinates.
		/// </summary>
		/// <returns>The data set.</returns>
		/// <param name="fileName">File name.</param>
		public ILinearDataSet<double> CreateDataSet(string fileName) {

			string filePath = GetFilePath (fileName);
			string[] fileData = File.ReadAllText (filePath).Replace(Environment.NewLine, ",").Split(',');

			if (fileData.Length % 2 != 0) {

				throw new InvalidDataException ($"Unable to parse CSV data in file {filePath}. Uneven number of coordinates.");

			}

			int i = 0;
			double[] x = new double[fileData.Length / 2];
			double[] y = new double[fileData.Length / 2];

			foreach (double value in fileData.Select(v => double.Parse(v)).AsEnumerable()) {

				if (i % 2 == 0) { x [i / 2] = value; }
				else { y [i / 2] = value; }

			}

			return new LinearDataSet (x, y);

		}

		public IDatabase CreateSqlDatabase(string id, string fileName) {
		
			return new SqliteDatabase (id, fileName);

		}

		public T CreateDatabaseAdapter<T>(IDatabase database) where T: IDBAdapter {
		
			throw new NotImplementedException ();

		}

		/// <summary>
		/// Tries to evaluate the full path of the file using the search paths if the file was not found.
		/// </summary>
		/// <returns>The file path.</returns>
		/// <param name="fileName">File name.</param>
		private string GetFilePath(string fileName) {
		
			if (!File.Exists (fileName)) {

				foreach (string path in m_searhPaths) {
				
					string evaluatedPath = path.EndsWith (Path.DirectorySeparatorChar.ToString ()) ? path + fileName : path + Path.DirectorySeparatorChar + fileName;

					if (File.Exists (evaluatedPath)) {
					
						return evaluatedPath;

					}

				}

				throw new FileNotFoundException ($"Unable to locate {fileName}. Is there a search path missing?");

			}

			return fileName;

		}
	}
}

