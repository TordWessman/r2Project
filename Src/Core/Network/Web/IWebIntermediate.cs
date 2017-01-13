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
using System.Dynamic;
using System.Collections.Specialized;

namespace Core.Network.Web
{
	/// <summary>
	/// IHttpIntermediate is responsible for containing data during Http communication. Implementations might i.e. be capable of transcribing data between the server and the sub system parser.
	/// </summary>
	public interface IWebIntermediate
	{
		/// <summary>
		/// Adds a header field to this object.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		void AddHeader(string key, string value);

		/// <summary>
		/// Sets a data field of this object.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		void SetValue(string key, dynamic value);

		/// <summary>
		/// Returns a new IHttpIntermediate object.
		/// </summary>
		/// <value>The new.</value>
		IWebIntermediate New { get; }

		/// <summary>
		/// Returns all data contained in this object.
		/// </summary>
		/// <value>The data.</value>
		dynamic Data { get; set; }

		/// <summary>
		/// Returns all header fields in this object.
		/// </summary>
		/// <value>The headers.</value>
		NameValueCollection Headers { get; }
	}
}

