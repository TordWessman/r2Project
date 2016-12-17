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
using System.Security.Cryptography;
using Core.Device;

namespace Core.Network
{

	public class SimpleNetworkSecurity : DeviceBase, INetworkSecurity
	{

		private string m_passwordHash;

		public SimpleNetworkSecurity (string id, string password) : base (id)
		{

			if (!String.IsNullOrEmpty (password)) {
			
				using (MD5 m = MD5.Create ()) {

					byte[] byteArray = m.ComputeHash (password.ToByteArray ());

					m_passwordHash = BitConverter.ToString (byteArray).Replace("-" ,"");

				}

			}

		}

		#region INetworkSecurity implementation

		public bool IsValid (string token)
		{

			if (token == null && m_passwordHash != null) {
			
				return false;

			}

			using (MD5 m = MD5.Create ()) {

				byte[] byteArray = m.ComputeHash (token.ToByteArray ());

				return m_passwordHash == BitConverter.ToString (byteArray).Replace("-" ,"");

			}

		}

		#endregion

	}

}

