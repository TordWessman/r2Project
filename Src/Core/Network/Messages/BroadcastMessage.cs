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
using System.Net;
using System.Collections.Generic;
using MessageIdType = System.String;

namespace R2Core.Network
{
	/// <summary>
	/// Broadcast message containing Identifier in order for receiver to identify messages. 
	/// </summary>
	public class BroadcastMessage : INetworkMessage {
		
		/// <summary>
		/// Messages with this header key is considered to be broadcast messages... eh. 
		/// </summary>
		public const string BroadcastMessageUniqueIdentifierHeaderKey = "BroadcastMessageUniqueIdentifier";

		public const string BroadcastMessagePortHeaderKey = "BroadcastMessagePortHeaderKey";

		public const string BroadcastMessageAddressHeaderKey = "BroadcastMessageAddressHeaderKey";

		public int Code { get; set; }
		public string Destination { get; set; }
		public System.Collections.Generic.IDictionary<string, object> Headers { get; set; }
		public dynamic Payload { get; set; }

		/// <summary>
		/// An unique identifier in the payload allowing messages to be tracked back to it's origin.
		/// </summary>
		/// <value>The identifier.</value>
		public String Identifier { 

			get { return Headers [BroadcastMessageUniqueIdentifierHeaderKey].ToString(); } 

			set {

				if (Headers == null) { Headers = new Dictionary<string, object>(); }
				Headers [BroadcastMessage.BroadcastMessageUniqueIdentifierHeaderKey] = value;

			}

		}

		/// <summary>
		/// The endpoint from where this message was sent.
		/// </summary>
		public string OriginAddress { get { return(string)Headers[BroadcastMessageAddressHeaderKey]; } }

		/// <summary>
		/// The port from where this message was sent.
		/// </summary>
		/// <value>The origin port.</value>
		public int OriginPort { get { return(int)Headers[BroadcastMessagePortHeaderKey]; } }

		/// <summary>
		/// Constructor used for incomming broadcasts
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="address">Sender address.</param>
		/// <param name="port">Sender port.</param>
		public BroadcastMessage(INetworkMessage message, string address, int port) {
		
			Code = message.Code;
			Destination = message.Destination;
			Headers = message.Headers ?? new System.Collections.Generic.Dictionary<string, object>();
			Payload = message.Payload;

			if (!Headers.ContainsKey(BroadcastMessageUniqueIdentifierHeaderKey)) {
			
				Headers.Add(BroadcastMessageUniqueIdentifierHeaderKey, Guid.NewGuid().ToString());

			}

			Headers[BroadcastMessagePortHeaderKey] = port;
			Headers[BroadcastMessageAddressHeaderKey] = address;

		}

		public override string ToString() {

			return string.Format("[BroadcastMessage: Code={0}, Destination={1}, Payload={2}]", Code, Destination, Payload);

		}

	}

}

