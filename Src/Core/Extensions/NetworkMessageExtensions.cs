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
using MessageIdType = System.String;

namespace R2Core.Network
{
	public static class NetworkMessageExtensions {

		/// <summary>
		/// Returns true if a message is a PingMessage or PongMessage.
		/// </summary>
		/// <returns><c>true</c> if is ping or pong the specified self; otherwise, <c>false</c>.</returns>
		public static bool IsPingOrPong(this INetworkMessage self) {

			return self.IsPing() || self.IsPong();

		}

		/// <summary>
		/// Returns true if a message is a PingMessage
		/// </summary>
		/// <returns><c>true</c> if is ping the specified self; otherwise, <c>false</c>.</returns>
		public static bool IsPing(this INetworkMessage self) {

			NetworkStatusCode.Ping.Is(null);

			return(self.HasCode(NetworkStatusCode.Ping) &&
				self.Destination == Settings.Consts.PingDestination());

		}

		/// <summary>
		/// Returns true if a message is a PongMessage
		/// </summary>
		/// <returns><c>true</c> if is ping the specified self; otherwise, <c>false</c>.</returns>
		public static bool IsPong(this INetworkMessage self) {

			return self.HasCode(NetworkStatusCode.Pong) &&
				self.Destination == Settings.Consts.PingDestination();

		}

		/// <summary>
		/// Returns true if the error message NetworkStatusCode is not null.
		/// </summary>
		/// <returns><c>true</c> if is error the specified self; otherwise, <c>false</c>.</returns>
		public static bool IsError(this INetworkMessage self) {

			return self.Code != (int)NetworkStatusCode.Ok;

		}

		/// <summary>
		/// Returns true if the ´INetworkMessage´ has no properties set.
		/// </summary>
		/// <returns><c>true</c> if is empty the specified self; otherwise, <c>false</c>.</returns>
		public static bool IsEmpty(this INetworkMessage self) {

			return self.Code == 0 && self.Destination == null && self.Payload == null;

		}

		/// <summary>
		/// Returns a string describing the error message, including the payload.
		/// </summary>
		/// <returns>The description.</returns>
		public static string ErrorDescription(this INetworkMessage self) {

			if (!self.IsError()) { return ""; }

			return $"Error: {self.Code}. Description: {self}";
			
		}

		/// <summary>
		/// Returns true if the ´Payload´ property is of a known primitive type. 
		/// </summary>
		/// <returns><c>true</c> if is primitive the specified self; otherwise, <c>false</c>.</returns>
		public static bool IsPrimitive(this INetworkMessage self) {

			return self.Payload is sbyte
				|| self.Payload is byte
				|| self.Payload is short
				|| self.Payload is ushort
				|| self.Payload is int
				|| self.Payload is uint
				|| self.Payload is long
				|| self.Payload is ulong
				|| self.Payload is float
				|| self.Payload is double
				|| self.Payload is decimal
				|| self.Payload is string;

		}

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
		public static MessageIdType GetBroadcastMessageKey(this INetworkMessage self) {

			object responseKey = null;

			return self.Headers?.TryGetValue(BroadcastMessage.BroadcastMessageUniqueIdentifierHeaderKey, out responseKey) == true ? responseKey?.ToString() : null;

		}

		/// <summary>
		/// Returns the Address representation as string if ´this´ is a ´BroadcastMessage´, otherwise null. 
		/// </summary>
		/// <returns>The broadcast address.</returns>
		public static string GetBroadcastAddress(this INetworkMessage self) {

			return self.IsBroadcastMessage() ? (self as BroadcastMessage).OriginAddress : null;

		}

		/// <summary>
		/// Returns the Port if ´this´ is a ´BroadcastMessage´, otherwise null. 
		/// </summary>
		/// <returns>The broadcast address.</returns>
		public static int GetBroadcastPort(this INetworkMessage self) {

			return self.IsBroadcastMessage() ? (self as BroadcastMessage).OriginPort : 0;

		}

	}
}

