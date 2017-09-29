﻿// This file is part of r2Poject.
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

namespace Core.Network.Web
{
	public interface IHttpClient
	{

		/// <summary>
		/// Synchronous http communication.
		/// </summary>
		/// <param name="message">Message.</param>
		HttpMessage Send (HttpMessage message);

		/// <summary>
		/// Asynchronous communication. The action delegate will have to take a Core.Network.Web.HttpResponse parameter. 
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="responseDelegate">Response delegate.</param>
		void SendAsync(HttpMessage message, Action<HttpMessage> responseDelegate);

	}

}