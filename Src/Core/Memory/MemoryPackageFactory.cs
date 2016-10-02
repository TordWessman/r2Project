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
using Core.Network.Data;
using System.Collections.Generic;
using Core.Network;

namespace Core.Memory
{
	public class MemoryPackageFactory : DataPackageFactory
	{
		public MemoryPackageFactory  (INetworkSecurity security) : base (security)
		{
		}
		
		public IDataPackage CreateGetMemoryReferencePackage (int memoryId)
		{
			IDictionary<string, string> headerValues = new Dictionary<string, string> ();
			headerValues.Add (HeaderFields.MemoryId.ToString (), memoryId.ToString ());
			headerValues.Add (HeaderFields.Checksum.ToString (), m_security.Token);

			return CreateEmptyPackage (DataPackageType.MemoryRequest, headerValues);
		}
		
		public IDataPackage<IMemoryReference> CreateMemoryReferencePackage (IMemoryReference reference)
		{
			IDictionary<string, string> headerValues = new Dictionary<string, string> ();
			headerValues.Add (HeaderFields.Checksum.ToString (), m_security.Token);

			IDataPackageHeader header = new DataPackageHeader (headerValues);

			return new DataPackage<IMemoryReference>(header,reference,DataPackageType.MemoryReply);

		}
	}
}

