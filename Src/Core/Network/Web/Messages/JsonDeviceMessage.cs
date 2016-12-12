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

namespace Core.Network.Web {

	public class JsonDeviceMessage : JsonBaseMessage
{
		public enum Statuses : int
		{
			OK = 200,			// Message delivered successfully
			BadToken = 401,		// Wrot token
			NoDevice = 404,		// The Device was not found
			GeneralError = 500,	// Anything else...
			ServerNotRegisteredAToken = 503	// Server had not yet registered a connection token
			
		}
				
	public string HttpMethod { get; set;}

	public string Device { get; set;}
	public string Function { get; set;}
	public string Params  { get; set;}
	public string Data { get; set;}
		public string Token { get; set;}

	}



}

