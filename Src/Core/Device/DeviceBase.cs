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

using System;
using System.Runtime.Serialization;

namespace Core.Device
{
	public abstract class DeviceBase : IDevice
	{
		protected string m_id;
		protected Guid m_guid;

		public DeviceBase (string id)
		{
			m_id = id;
			m_guid = Guid.NewGuid ();
		}
		
		public string Identifier { get {
				return m_id;}}

		public Guid Guid { get { return m_guid; } }
		
		public virtual void Start () {}
		
		public virtual void Stop () {}
		
		public virtual bool Ready { get { return true; } }


		
	}
}

