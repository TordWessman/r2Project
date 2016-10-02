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

namespace Core.Network.Http
{
	/// <summary>
	/// Interprets data input and returns a decoded response.
	/// sent back to the client
	/// </summary>
	public interface IHttpServerInterpreter
	{
		/// <summary>
		/// Interprets inputData and returns byte array of response data.
		/// </summary>
		/// <param name="inputData">Input data.</param>
		/// <param name="httpMethod">Http method.</param>
		byte[] Interpret(string inputData, string uri = null, string httpMethod = null);

		/// <summary>
		/// The uri path on which this interprener is listening 
		/// </summary>
		/// <value>The response path.</value>
		bool Accepts (string uri);

		/// <summary>
		/// Thee response content type
		/// </summary>
		/// <value>The type of the http content.</value>
		string HttpContentType {get; }

		/// <summary>
		/// Gets the extra headers.used for the response
		/// </summary>
		/// <value>The extra headers.</value>
		IDictionary<string,string> ExtraHeaders {get;}

	}
}

