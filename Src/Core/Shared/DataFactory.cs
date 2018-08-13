using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using R2Core.Device;

namespace R2Core
{
	/// <summary>
	/// Use this factory to create data containers
	/// </summary>
	public class DataFactory: DeviceBase
	{
		IEnumerable<string> m_searchPaths;

		public DataFactory (string id, IEnumerable<string> searchPaths = null) : base(id)
		{
			
			m_searchPaths = searchPaths ?? new List<string> ();

		}

		/// <summary>
		/// Loads a data set from the specified file. The file format should be of a simple CSV type containing x/y coordinates.
		/// </summary>
		/// <returns>The data set.</returns>
		/// <param name="fileName">File name.</param>
		public ILinearDataSet<double> CreateDataSet(string fileName) {

			string filePath = GetFilePath (fileName);
			string[] fileData = File.ReadAllText (filePath).Trim( new char[]{'\n', '\r', ' ', '	'}).Replace(Environment.NewLine, ",").Split(',');

			if (fileData.Length % 2 != 0) {

				throw new InvalidDataException ($"Unable to parse CSV data in file {filePath}. Uneven number of coordinates ({fileData.Length})");

			}


			IDictionary<double, double> points = new Dictionary<double, double> ();
			int i = 0;
			double x = 0;

			foreach (double value in fileData.Select(v => double.Parse(v)).AsEnumerable()) {
				
				if (i++ % 2 == 0) { x = value; }
				else { 
				
					if (points.Keys.Contains(x)) { 

						Log.e ($"Trying to add duplicate values for x = {x}");

					} else {

						points.Add (x, value); 

					}
						
				}

			}

			return new LinearDataSet (points);

		}

		/// <summary>
		/// Tries to evaluate the full path of the file using the search paths if the file was not found.
		/// </summary>
		/// <returns>The file path.</returns>
		/// <param name="fileName">File name.</param>
		public string GetFilePath(string fileName) {
		
			if (!File.Exists (fileName)) {

				foreach (string path in m_searchPaths) {
				
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

