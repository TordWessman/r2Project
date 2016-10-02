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
using System.Net;

namespace Core.Device
{
	/// <summary>
	/// Used as a data baerer containing enugh information to create the local representation of a remote device.
	/// </summary>
	public class RemoteDeviceReference
	{

		protected string m_id;
		private Guid m_guid;
		protected IRPCManager<IPEndPoint> m_networkManager;
		protected IPEndPoint m_host;

		public string Id { get { return m_id; } }
		public Guid Guid { get { return m_guid; } }
		public IRPCManager<IPEndPoint> NetworkManager { get { return m_networkManager; } }
		public IPEndPoint Host { get { return m_host; } }

		public RemoteDeviceReference(string id, string guid, IRPCManager<IPEndPoint> networkManager, IPEndPoint host)
		{

			if (id == null) {

				throw new ArgumentNullException ("RemoteDeviceBase id must not be null.");

			} else if (guid == null) {

				throw new ArgumentNullException ("RemoteDeviceBase checksum must not be null.");
			}

			m_guid = new Guid(guid);
			m_id = id;
			m_networkManager = networkManager;
			m_host = host;

		}
	
	}

}