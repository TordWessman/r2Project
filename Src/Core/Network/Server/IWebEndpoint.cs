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

namespace R2Core.Network
{
	/// <summary>
	/// Interprets data input and returns a decoded response.
	/// sent back to the client
	/// </summary>
	public interface IWebEndpoint {
		
		/// <summary>
		/// Interprets inputData and returns an intermediate object. This object should not have a serialized payload, since serialization is performed by the server.
		/// </summary>
		INetworkMessage Interpret(INetworkMessage request, System.Net.IPEndPoint source);

		/// <summary>
		/// The uri path on which this interprener is listening.
		/// </summary>
		/// <value>The response path.</value>
		string UriPath { get; }

	}
}

