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

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Core.Network.Http
{
	/// <summary>
	/// Uses as an image server within a IHttpServer.
	/// </summary>
	public class ImageInterpreter : IHttpServerInterpreter
	{
		private string m_imagePath;
		private string m_responsePath;
		private string m_lastRequestedImageType;

		/// <summary>
		/// Initializes a new instance of the <see cref="Core.Network.Http.ImageInterpreter"/> class.
		/// image path is the local directory where the served images resides
		/// resourcePath is the response resource uri part (ie "/images")
		/// </summary>
		/// <param name="imagePath">Image path.</param>
		/// <param name="responsePath">Response path.</param>
		public ImageInterpreter (string imagePath, string responsePath)
		{
			m_imagePath = imagePath;
			m_responsePath = responsePath;
			m_lastRequestedImageType = "jpeg";
		}

		#region IHttpServerInterpreter implementation

		public byte[] Interpret (string inputData, string uri = null, string httpMethod = null)
		{
			Match fileNameMatch = new Regex (@"[A-Za-z0-9\.]+$").Match (uri);
			string imageName = fileNameMatch != null ? fileNameMatch.Value : null;

			if (string.IsNullOrEmpty(imageName)) {

				throw new InvalidDataException ("Unable to retrieve image name.");

			}

			string fileName = m_imagePath + Path.DirectorySeparatorChar + imageName;

			if (!File.Exists (fileName)) {
			
				throw new IOException ("Image file: " + imageName + " not found.");

			}

			m_lastRequestedImageType = new Regex (@"[A-Za-z]+$").Match (imageName).Value;

			return File.ReadAllBytes (fileName);

		}

		public bool Accepts(string uri) {

			return uri.StartsWith(m_responsePath);

		}


		public string HttpContentType {

			get {

				return "image/" + m_lastRequestedImageType;

			}

		}

		public IDictionary<string, string> ExtraHeaders {

			get {

				return new Dictionary<string,string> ();

			}

		}

		#endregion
	}
}

