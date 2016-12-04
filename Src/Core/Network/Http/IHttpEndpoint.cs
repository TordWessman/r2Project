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
using System.Collections.Specialized;

namespace Core.Network.Http
{
	/// <summary>
	/// Interprets data input and returns a decoded response.
	/// sent back to the client
	/// </summary>
	public interface IHttpEndpoint
	{
		/// <summary>
		/// Interprets inputData and returns byte array of response data.
		/// </summary>
		/// <param name="inputData">Input data.</param>
		/// <param name="inputData">uri.</param>
		/// <param name="httpMethod">Http method.</param>
		/// <param name="inputData">headers</param>
		byte[] Interpret(byte[] inputData, Uri uri = null, string httpMethod = null, NameValueCollection headers = null);

		/// <summary>
		/// The uri path on which this interprener is listening.
		/// </summary>
		/// <value>The response path.</value>
		string UriPath {get;}

		/// <summary>
		/// Extra response headers.
		/// </summary>
		/// <value>The extra headers.</value>
		NameValueCollection ExtraHeaders {get;}

	}
}

