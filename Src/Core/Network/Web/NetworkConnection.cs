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
using Core.Device;
using System.Collections.Generic;

namespace Core.Network
{
	/// <summary>
	/// Default implementation of an INetworkConnection.
	/// </summary>
	public class NetworkConnection : DeviceBase, INetworkConnection
	{
		private IMessageClient m_connection;
		private INetworkMessage m_message;

		/// <summary>
		/// `destination` is the path used when sending messages. `host` is the connection used for transfers.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="destination">Destination.</param>
		/// <param name="host">Host.</param>
		public NetworkConnection (string id, string destination, IMessageClient connection) : base(id) {
			
			m_connection = connection;
			m_message = new NetworkMessage() { Headers = new Dictionary<string, object> (), Destination = destination };

		}

		public IDictionary<string, object> Headers  { get { return m_message.Headers; } }

		public override bool Ready { get { return m_connection.Ready; } }

		public override void Start () { m_connection.Start (); }

		public override void Stop () { m_connection.Stop (); }

		public dynamic Send(dynamic payload) {

			m_message.Payload = payload;
			INetworkMessage response = m_connection.Send (m_message);

			if (WebStatusCode.Ok.Is(response.Code)) {
			
				return response.Payload;

			}

			throw new ApplicationException ($"Request to {m_message.Destination} for connection `m_connection.ToString ()` failed with code `{response.Code}`. Response body: `{response.Payload}`.");

		}

	}
}

