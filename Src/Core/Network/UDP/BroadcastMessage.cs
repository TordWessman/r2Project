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
	public class BroadcastMessage : INetworkMessage
	{
		public const string BroadcastMessageUniqueIdentifierHeaderKey = "BroadcastMessageUniqueIdentifier";

		public int Code { get; set; }
		public string Destination { get; set; }
		public System.Collections.Generic.IDictionary<string, object> Headers { get; set; }
		public dynamic Payload { get; set; }

		/// <summary>
		/// An unique identifier in the payload allowing messages to be tracked back to it's origin.
		/// </summary>
		/// <value>The identifier.</value>
		public String Identifier { 

			get { return Headers [BroadcastMessageUniqueIdentifierHeaderKey].ToString (); } 

			set {

				if (Headers == null) { Headers = new Dictionary<string, object> (); }
				Headers [BroadcastMessage.BroadcastMessageUniqueIdentifierHeaderKey] = value;

			}

		}

		/// <summary>
		/// The endpoint from where this message was sent. This information is typically set on the receiver side ands hould not be included in the message.
		/// </summary>
		/// <value>The origin.</value>
		public IPEndPoint Origin { get; set; }

		/// <summary>
		/// Constructor used for incomming broadcasts
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="origin">Origin.</param>
		public BroadcastMessage(INetworkMessage message, IPEndPoint origin = null) {
		
			Code = message.Code;
			Destination = message.Destination;
			Headers = message.Headers ?? new System.Collections.Generic.Dictionary<string, object>();
			Payload = message.Payload;
			Origin = origin;

			if (!Headers.ContainsKey (BroadcastMessageUniqueIdentifierHeaderKey)) {
			
				Headers.Add (BroadcastMessageUniqueIdentifierHeaderKey, Guid.NewGuid ().ToString());

			}

		}

	}

	public static class INetworkMessageExtensions {
	
		/// <summary>
		/// Determines if the message is a broadcast message.
		/// </summary>
		/// <returns><c>true</c> if is broadcast message the specified self; otherwise, <c>false</c>.</returns>
		/// <param name="self">Self.</param>
		public static bool IsBroadcastMessage(this INetworkMessage self) {
		
			return self.GetBroadcastMessageKey() != null;
				
		}

		/// <summary>
		/// Returns the unique message id for this message if it's a broadcast message or null if not.
		/// </summary>
		/// <returns>The broadcast message key.</returns>
		/// <param name="self">Self.</param>
		public static MessageIdType GetBroadcastMessageKey(this INetworkMessage self) {
		
			object responseKey = null;

			return self.Headers?.TryGetValue (BroadcastMessage.BroadcastMessageUniqueIdentifierHeaderKey, out responseKey) == true ? responseKey?.ToString() : null;

		}

	}

}

