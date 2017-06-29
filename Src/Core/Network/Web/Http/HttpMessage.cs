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

	public struct HttpMessage
	{
		public static readonly string DefaultContentType = @"application/json";

		public int? Code;
		public dynamic Payload;
		public string Destination;
		public IDictionary<string, object> Headers;

		public string Method;
		public string ContentType;


		public HttpMessage (string url, string method = "POST", object body = null, IDictionary<string, object> headers = null) {

			ContentType = DefaultContentType;

			Destination = url;
			Method = method;
			Payload = body;
			Headers = headers;

			Code = null;

		}

		public HttpMessage(HttpError error, int code, IDictionary<string, object> headers = null) {

			ContentType = DefaultContentType;

			Destination = Method = null;

			Payload = error;
			Code = code;
			Headers = headers;

		}


	}

	public struct HttpError {
	
		public string Message;

		public HttpError (string message) {
		
			Message = message;

		}

	}

}

