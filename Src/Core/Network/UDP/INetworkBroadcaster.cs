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
using R2Core.Device;
using System.Threading.Tasks;
using MessageIdType = System.String;

namespace R2Core.Network
{
	/// <summary>
	/// Represents a broadcast client, capable of sending INetworkMessages
	/// </summary>
	public interface INetworkBroadcaster : IDevice {
		
		/// <summary>
		/// Will broadcast `message` through the implentation specific medium(i.e. UDP). 
		/// Waits `timeout` for any response. Upon response, `responseDelegate` is called.
		/// Returns an unique identifier for the broadcast message sent.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="timeout">Timeout.</param>
		/// <param name="responseDelegate">Response delegate.</param>
		MessageIdType Broadcast(INetworkMessage message, Action<INetworkMessage, Exception> responseDelegate = null, int timeout = 2000);

	}

}

