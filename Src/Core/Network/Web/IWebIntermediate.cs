﻿// This file is part of r2Poject.
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
using System.Collections.Generic;

namespace Core.Network.Web
{
	/// <summary>
	/// IHttpIntermediate is responsible for containing data during Http communication. Implementations might i.e. be capable of transcribing data between the server and the sub system parser.
	/// </summary>
	public interface IWebIntermediate
	{
		/// <summary>
		/// Returns all data contained in this object.
		/// </summary>
		/// <value>The data.</value>
		dynamic Data { get; set; }

		/// <summary>
		/// Adds a metadata field to this object.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		void AddMetadata(string key, object value);

		/// <summary>
		/// Returns all metadata fields in this object.
		/// </summary>
		/// <value>The headers.</value>
		IDictionary<string, object> Metadata { get; }

		/// <summary>
		/// Converts data to CLR typed format.
		/// </summary>
		void CLRConvert ();

	}

	public static class IWebIntermediateExtension {
	
		public static bool IsPrimitive(this IWebIntermediate self) {
		
			return self.Data is sbyte
				|| self.Data is byte
				|| self.Data is short
				|| self.Data is ushort
				|| self.Data is int
				|| self.Data is uint
				|| self.Data is long
				|| self.Data is ulong
				|| self.Data is float
				|| self.Data is double
				|| self.Data is decimal
				|| self.Data is string;

		}
	}
}