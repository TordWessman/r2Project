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
using System.Collections.Specialized;
using System.Dynamic;
using System.Collections.Generic;

namespace Core.Network.Http
{
	/// <summary>
	/// Represents an implementation capable of handdling objects through HTTP requests
	/// </summary>
	public interface IHttpObjectReceiver<T> where T: IDictionary<string,Object>
	{
		/// <summary>
		/// Handles the receival of the input object using httpMethod and including the optional header fields. Returns an IHttpIntermediate intermediation object capable of storing data and headers.
		/// </summary>
		/// <param name="input">Input.</param>
		/// <param name="httpMethod">Http method.</param>
		/// <param name="headers">Headers.</param>
		IHttpIntermediate onReceive (JsonExportObject<T> input, string httpMethod, NameValueCollection headers = null);

	}
}