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

namespace Core.Network.Web
{
	/// <summary>
	/// Uses as a very primitive file server within an IHttpServer.
	/// </summary>
	public class WebFileEndpoint : IWebEndpoint
	{
		private string m_basePath;
		private string m_responsePath;
		private string m_lastRequestedFileExtension;
		private string m_contentType;

		private const string FileMatchRegexp = @"[A-Za-z0-9\.\-\_]+$";
		private const string ExtensionMatchRegexp = @"[A-Za-z]+$";
		private const string DefaultFileType = "text";

		/// <summary>
		/// image path is the local directory where the served files resides
		/// responsePath is the response resource uri part (ie "/images")
		/// </summary>
		/// <param name="basePath">Path containing file resources.</param>
		/// <param name="responsePath">Response path.</param>
		public WebFileEndpoint (string basePath, string responsePath, string contentType)
		{
			m_basePath = basePath;
			m_responsePath = responsePath;
			m_contentType = contentType ?? "file";
			m_lastRequestedFileExtension = "*";

			//m_extraHeaders.Add ("Content-Type", @"application/json");
		}

		#region IHttpServerInterpreter implementation

		public byte[] Interpret (byte[] input, IDictionary<string, object> metaData = null)
		{
			Uri uri = metaData.ContainsKey (HttpServer.URI_KEY) && metaData [HttpServer.URI_KEY] is Uri  ? metaData [HttpServer.URI_KEY] as Uri : null;

			if (uri == null) {
			
				throw new MissingFieldException ("Unable to process request. Uri was null");
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

			using (FileStream file = new FileStream (fileName, FileMode.Open)) {
			
				//file.
				using (StreamReader reader = new StreamReader (file)) {
			
					//reader.r
				}
			
			}
			return File.ReadAllBytes (fileName);

		}

		public string UriPath {

			get {

				return m_responsePath;

			}

		}

		public IDictionary<string, object> Metadata {

			get {

				var headers = new Dictionary<string, object> ();

				headers.Add ("Content-Type", m_contentType + "/" + m_lastRequestedFileExtension);

				return headers;
			}

		}

		#endregion
	}
}

