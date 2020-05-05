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
	/// Default implementation of ´INetworkMessage´ containing error information.
	/// </summary>
	public class NetworkErrorMessage : INetworkMessage {
		
		public int Code { get; set; }

		public string Destination { get; set; }

		public  System.Collections.Generic.IDictionary<string, object> Headers { get; set; }

		public dynamic Payload { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="R2Core.Network.NetworkErrorMessage"/> class.
		/// </summary>
		/// <param name="ex">Exception information used in the ´Payload´.</param>
		/// <param name="requestMessage">Request message sent when the error occured. (if any).</param>
		public NetworkErrorMessage(Exception ex, INetworkMessage requestMessage = null) {
			
			Code = NetworkStatusCode.ServerError.Raw();
			Destination = requestMessage?.Destination;
			Payload = new NetworkErrorDescription($"Error: {ex.ToString()}. Request: {requestMessage}.");

		}

		/// <summary>
		/// Instantiate using an error code (´code´) and a payload of type ´NetworkErrorDescription´ with the
		/// Message ´ḿessage´.
		/// </summary>
		/// <param name="code">Code.</param>
		/// <param name="message">Message.</param>
		public NetworkErrorMessage(NetworkStatusCode code, string message, INetworkMessage originalRequest = null) {
		
			Code = code.Raw();
			Payload = new NetworkErrorDescription(message);
			Destination = originalRequest?.Destination;
			Headers = originalRequest?.Headers;
		
		}

		public override string ToString() { return $"NetworkErrorMessage: [Code: {Code}, Destination: {Destination}, Message: {Payload}]"; }

	}

	/// <summary>
	/// Represents an error object.
	/// </summary>
	public struct NetworkErrorDescription {

		public string Message;

		public NetworkErrorDescription(string message) {

			Message = message;

		}

        public override string ToString() { return Message; }

    }

}

