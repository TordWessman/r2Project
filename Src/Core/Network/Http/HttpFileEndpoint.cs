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
using System.Collections.Specialized;

namespace Core.Network.Http
{
	/// <summary>
	/// Uses as a very primitive file server within an IHttpServer.
	/// </summary>
	public class HttpFileEndpoint : IHttpEndpoint
	{
		private string m_basePath;
		private string m_responsePath;
		private string m_lastRequestedFileExtension;
		private string m_contentType;

		private const string FileMatchRegexp = @"[A-Za-z0-9\.\-\_]+$";
		private const string ExtensionMatchRegexp = @"[A-Za-z]+$";
		private const string DefaultFileType = "text";

		/// <summary>
		/// Initializes a new instance of the <see cref="Core.Network.Http.HttpFileEndpoint"/> class.
		/// image path is the local directory where the served files resides
		/// responsePath is the response resource uri part (ie "/images")
		/// </summary>
		/// <param name="basePath">Path containing file resources.</param>
		/// <param name="responsePath">Response path.</param>
		public HttpFileEndpoint (string basePath, string responsePath, string contentType)
		{
			m_basePath = basePath;
			m_responsePath = responsePath;
			m_contentType = contentType;
			m_lastRequestedFileExtension = "*";
		}

		#region IHttpServerInterpreter implementation

		public byte[] Interpret (string inputData, Uri uri = null, string httpMethod = null, NameValueCollection headers = null)
		{
			if (uri == null) {
			
				throw new InvalidDataException ("Unable to process request. Uri was null");
			}

			Match fileNameMatch = new Regex (FileMatchRegexp).Match (uri.AbsolutePath);
			string imageName = fileNameMatch?.Value;

			if (string.IsNullOrEmpty(imageName)) {

				throw new InvalidDataException ("Unable to retrieve file name. Using regexp: " + FileMatchRegexp + " to match uri: " + uri.AbsolutePath);

			}

			string fileName = m_basePath + Path.DirectorySeparatorChar + imageName;

			if (!File.Exists (fileName)) {
			
				throw new IOException ("File: " + fileName + " not found.");

			}

			m_lastRequestedFileExtension = new Regex (ExtensionMatchRegexp).Match (imageName)?.Value ?? DefaultFileType;

			return File.ReadAllBytes (fileName);

		}

		public bool Accepts(Uri uri) {

			return uri.AbsolutePath.StartsWith(m_responsePath);

		}


		public string HttpContentType {

			get {

				return m_contentType + "/" + m_lastRequestedFileExtension;

			}

		}

		public NameValueCollection ExtraHeaders {

			get {

				return new NameValueCollection ();

			}

		}

		#endregion
	}
}

