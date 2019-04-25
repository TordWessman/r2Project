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

namespace R2Core.Network
{

	/// <summary>
	/// Ping message. Internally used.
	/// </summary>
	internal class PingMessage : INetworkMessage {

		public int Code { get; set; }

		public string Destination { get; set; }

		public  System.Collections.Generic.IDictionary<string, object> Headers { get; set; }

		public dynamic Payload { get; set; }

		public PingMessage() {
			
			Destination = Settings.Consts.PingDestination();
			Code = WebStatusCode.Ping.Raw();
		
		}
	
	}

	/// <summary>
	/// Pong message. Internally used.
	/// </summary>
	internal class PongMessage : PingMessage {

		public PongMessage() {
			
			Destination = Settings.Consts.PingDestination();
			Code = WebStatusCode.Pong.Raw();

		}

	}

}

