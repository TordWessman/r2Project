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
using System.Collections.Generic;
using System.Dynamic;
using Core.Device;
using Core.Data;

namespace Core.Network.Web
{
	public class TCPPackageFactory: DeviceBase
	{
		private IR2Serialization m_serialization;
		private INetworkSecurity m_security;

		public TCPPackageFactory (string id, IR2Serialization serialization, INetworkSecurity security = null): base(id) {
		
			m_security = security;
			m_serialization = serialization;

		}

		public byte[] CreateRaw(TCPPackage package) {
		
			byte[] headerData = m_serialization.Serialize (package.Headers);
			byte[] bodyData = m_serialization.Serialize (package.Payload);
			byte[] path = m_serialization.Serialize (package.Path);

			throw new NotImplementedException ();
		
		}

		public TCPPackage CreatePackage(byte [] rawData) {
		
			throw new NotImplementedException ();

		}
	}

}

