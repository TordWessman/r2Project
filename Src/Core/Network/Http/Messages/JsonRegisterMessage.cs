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

namespace Core.Network.Http
{
	public class JsonRegisterMessage : JsonBaseMessage
	{
		public JsonRegisterMessage ()
		{
		}

		public string Login { get; set;}
		public string Password { get; set;}
		public string Token  { get; set;}
		public bool Success {get; set;}
		public string ClientPort {get; set;}

		public override string ToString ()
		{
			return string.Format ("[JsonRegisterMessage: Login={0}, Password={1}, Token={2}, Success={3}]", Login, Password, Token, Success);
		}
	}
}

