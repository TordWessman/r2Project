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
using System.Dynamic;
using R2Core.Device;

namespace R2Core.Network
{
	/// <summary>
	/// Capable of listening to IWebSocketSender messages.
	/// </summary>
	public interface IWebSocketSenderDelegate {
	
		void OnSend(byte[] data);

	}

	/// <summary>
	/// Implementations are used for transmitting data to web socket clients.
	/// </summary>
	public interface IWebSocketSender : IDevice {
		
		IWebSocketSenderDelegate Delegate { get; set; }
		void Send(dynamic outputObject);

		/// <summary>
		/// The path on which this sender will be sending data to.
		/// </summary>
		/// <value>The URI path.</value>
		string UriPath { get; }
	
	}

}