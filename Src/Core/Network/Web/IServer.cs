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

﻿using System;
using Core.Device;

namespace Core.Network
{
	/// <summary>
	/// Basic interface for a web server
	/// </summary>
	public interface IServer: IDevice
	{

		/// <summary>
		/// Returns the address on which the server is configured to listen to.
		/// </summary>
		/// <value>The port.</value>
		string Address {get;}

		/// <summary>
		/// Returns the port on which the server is configured to listen to.
		/// </summary>
		/// <value>The port.</value>
		int Port {get;}

		/// <summary>
		/// Adds a new IWebEndpoint to the interpreters list in order for the server to allow other, different requests.
		/// </summary>
		/// <param name="interpreter">Interpreter.</param>
		void AddEndpoint(IWebEndpoint interpreter);

	}
}

