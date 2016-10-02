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
using System.Net;
using Core.Memory;
using MemoryType = System.String;
using System.Collections.Generic;
using System.Linq;

namespace Core.Device
{
	/// <summary>
	/// Allows memory source to be accessed remotely
	/// </summary>
	public class RemoteMemorySource : RemoteDeviceBase, IMemorySource
	{

		public RemoteMemorySource (RemoteDeviceReference reference) : base (reference)
		{

			// The id must be unique for the remote instance
			m_id = m_id + "_" + m_host.Address.ToString() + "_" + m_host.Port.ToString ();

		}
	
		public Core.Memory.IMemory Get (int memoryId)
		{
		
			IMemoryReference reference = Execute<IMemoryReference, int> (SharedMemorySource.F_GetId, memoryId);

			return reference.IsNull ? null : new Core.Memory.Memory (reference, this, false);
		
		}

		public System.Collections.Generic.ICollection<IMemory> Get (int[] memoryIds) {

			ICollection<IMemoryReference> references = Execute<ICollection<IMemoryReference>, int[]> (SharedMemorySource.F_GetIds, memoryIds);

			return new List<IMemory>(references.Where (r => !r.IsNull).Select (m => new Core.Memory.Memory (m, this, false)));

		}

		public IMemory Get (MemoryType type) {

			IMemoryReference reference = Execute<IMemoryReference, MemoryType> (SharedMemorySource.F_GetType, type);

			return reference.IsNull ? null : new Core.Memory.Memory (reference, this, false);

		}

		public System.Collections.Generic.ICollection<Core.Memory.IMemory> GetAssociations (Core.Memory.IMemory reference)
		{

			ICollection<IMemoryReference> references = Execute<ICollection<IMemoryReference>, int> (SharedMemorySource.F_GetAssociations, reference.Id);

			return new List<IMemory>(references.Where (r => !r.IsNull).Select (m => new Core.Memory.Memory (m, this, false)).ToList());

		}

		public System.Collections.Generic.ICollection<Core.Memory.IMemory> All (MemoryType type)
		{

			ICollection<IMemoryReference> references = Execute<ICollection<IMemoryReference>, MemoryType> (SharedMemorySource.F_GetType, type);

			return new List<IMemory>(references.Where (r => !r.IsNull).Select (m => new Core.Memory.Memory (m, this, false)).ToList());

		}

		public Core.Memory.IMemory Create (string type, string name)
		{
			throw new NotImplementedException ("Cannot remotly create memories.");
		}

		public void Associate (Core.Memory.IMemory one, Core.Memory.IMemory two)
		{

			IMemoryReference[] references = new IMemoryReference[] { one.Reference, two.Reference };

			Execute<IMemoryReference[]> (SharedMemorySource.F_AddAssociation, references);

		}

		public bool Delete (int memoryId)
		{

			return Execute<bool, int> (SharedMemorySource.F_DeleteMemory, memoryId);

		}

		public bool Delete (IMemory memory)
		{

			return Execute<bool, int> (SharedMemorySource.F_DeleteMemory, memory.Id);

		}

		public bool Update (Core.Memory.IMemory memory)
		{

			return Execute<bool, IMemoryReference> (SharedMemorySource.F_DeleteMemory, memory.Reference);
		
		}

	}

}