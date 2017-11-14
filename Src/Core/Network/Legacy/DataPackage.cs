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
using Core.Device;
using System.Runtime.Serialization;

namespace Core.Network.Data
{
	
	
	public class DataPackage : IDataPackage {
		private IDataPackageHeader m_header;
		private DataPackageType m_type;
		
		public DataPackage (IDataPackageHeader header, DataPackageType type)
		{
			m_header = header;
			m_type = type;
		}


		public new DataPackageType GetType ()
		{
			return m_type;
		}

		public IDataPackageHeader GetHeader ()
		{
			return m_header;
		}

	}
	
	public class DataPackage<K> : IDataPackage<K> /*where K : ISerializable*/
	{
	
		private IDataPackageHeader m_header;
		private DataPackageType m_type;
		private K m_data;
		
		public DataPackage (IDataPackageHeader header, K data, DataPackageType type)
		{
			m_header = header;
			m_data = data;
			m_type = type;
		}

		#region IDataPackage implementation
		public K Value  { 
			get
		{
			return m_data;
		}
		}
		
		public new DataPackageType GetType ()
		{
			return m_type;
		}

		public IDataPackageHeader GetHeader ()
		{
			return m_header;
		}

		#endregion
	}
}

