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
//
using System;

namespace R2Core.Network
{

	/// <summary>
	/// Pre-defined message codes for INetworkMessage
	/// </summary>
	public enum NetworkStatusCode : int {
		
		/// <summary>
		/// The code has not yet been set.
		/// </summary>
		NotDefined = 0,

		/// <summary>
		/// Indicatates that the message request had the same origin as the server and will therefore not be handled. 
		/// </summary>
		SameOrigin = 103,

		/// <summary>
		/// Message handle without issues.
		/// </summary>
		Ok = 200,

		/// <summary>
		/// Generic client side error.
		/// </summary>
		BadRequest = 400,

		/// <summary>
		/// Client did not provide correct credentials.
		/// </summary>
		Unauthenticated = 401,

		/// <summary>
		/// Client does not have access to the resource.
		/// </summary>
		Forbidden = 403,

		/// <summary>
		/// Resource not found.
		/// </summary>
		NotFound = 404,

		/// <summary>
		/// The server was unable to process the request.
		/// </summary>
		UnableToProcess = 422,

		/// <summary>
		/// Generic error during server side execution.
		/// </summary>
		ServerError = 500,

		/// <summary>
		/// Server did not contain the requested resource.
		/// </summary>
		ResourceUnavailable = 503,

		/// <summary>
		/// Communication error on the client side.
		/// </summary>
		NetworkError = 600,

		/// <summary>
		/// Ping message
		/// </summary>
		Ping = 4444,

		/// <summary>
		/// Pong message
		/// </summary>
		Pong = 4445

	}

}
