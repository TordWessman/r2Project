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
//
using System;
using System.Collections.Generic;
using System.Net;

namespace Core.Network.Web
{
	public struct HttpRequest
	{
		public static readonly string DefaultContentType = @"application/json";

		public dynamic Body;
		public string Method;
		public string Url;
		public IDictionary<string, object> Headers;
		public string ContentType;

		public HttpRequest (string url, string method = "POST", dynamic body = null, IDictionary<string, object> headers = null) {

			ContentType = DefaultContentType;

			Url = url;
			Method = method;
			Body = body;
			Headers = headers;

		}

	}

	public struct HttpError {
	
		public string Message;

	}

	public struct HttpResponse {
	
		public HttpStatusCode? Code;
		public dynamic Body;
		public HttpError? Error;
		public IDictionary<string, object> Headers;

	}

}

