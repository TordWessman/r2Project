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

namespace R2Core.Network
{

	public struct TCPMessage : INetworkMessage {
		
		public int Code { get; set; }
		public string Destination { get; set; }
		public IDictionary<string, object> Headers { get; set; }
		public dynamic Payload { get; set; }

		/// <summary>
		/// Determined data type of the payload
		/// </summary>
		public TCPPackageFactory.PayloadType PayloadType;

		public TCPMessage(INetworkMessage message) {
		
			Code = message.Code != 0 ? message.Code : WebStatusCode.Ok.Raw();
			Destination = message.Destination;
			Headers = message.Headers;
			Payload = message.Payload;

			PayloadType = TCPPackageFactory.GetPayloadType(message);

		}

		public override string ToString() {

			return string.Format("[TCPMessage: Code={0}, Destination={1}, Payload={2}]", Code, Destination, Payload);

		}

	}

}