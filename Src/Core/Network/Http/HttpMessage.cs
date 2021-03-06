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
using System.Collections.Generic;
using System.Net;

namespace R2Core.Network
{

	/// <summary>
	/// Default ´INetworkMessage´ for Http requests
	/// </summary>
	public struct HttpMessage : INetworkMessage {

		public const string DefaultHttpMethod = "POST";
		public const string DefaultContentType = "application/json";

		public int Code { get; set; }
		public dynamic Payload { get; set; }
		public string Destination { get; set; }
		public IDictionary<string, object> Headers { get; set; }

		public string Method { get; set; }
		public string ContentType { get; set; }

        public HttpMessage(INetworkMessage message) {

			Code = message.Code;
			Payload = message.Payload;
			Destination = message.Destination;
			Headers = message.Headers;

			if (message is HttpMessage) {
				
				Method = ((HttpMessage) message).Method;
				ContentType = ((HttpMessage) message).ContentType;
			
			} else {
			
				Method = DefaultHttpMethod;
				ContentType = DefaultContentType;

			}

		}

		public override string ToString() {

			return string.Format("[HttpMessage: Code={0}, Destination={1}, Payload={2}, Method={3}]", Code, Destination, Payload, Method
            );

		}

	}

}

