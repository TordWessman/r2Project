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
using System.Net;
using System.Threading.Tasks;
using MemoryType = System.String;
using System.Collections.Generic;

namespace Core.Memory
{
	public class RemoteMemoryBus : RemoteDeviceBase, IMemoryBus
	{

		public RemoteMemoryBus (RemoteDeviceReference reference) : base (reference) {}

		#region IMemoryBus implementation
		public void AddMemories (System.Collections.Generic.ICollection<IMemoryReference> memories)
		{

			Execute<ICollection<IMemoryReference>> (
				MemoryBus.F_AddMemories, memories);
		
		}

		public void AddAssociation (IMemoryReference first, IMemoryReference second)
		{

			Execute<IMemoryReference[]> (
				MemoryBus.F_AddAssociation, new IMemoryReference[] {first, second});
		
		}

		public ICollection<IMemoryReference> GetAssociations (int memoryId)
		{

			return Execute<ICollection<IMemoryReference>, int> (
				MemoryBus.F_GetAssociations, memoryId);
		
		}

		public IMemoryReference GetReference (int memoryId)
		{

			return Execute<IMemoryReference, int> (
				MemoryBus.F_GetReference, memoryId);
		
		}

		public ICollection<IMemoryReference> GetReferences (MemoryType type)
		{
			
			return Execute<ICollection<IMemoryReference>, MemoryType> (
				MemoryBus.F_GetReferences, type);
			
		}
		
		public void RemoveMemory (IMemoryReference memory)
		{

			Execute<IMemoryReference> (
				MemoryBus.F_RemoveMemory, memory);
		
		}

		public int NextMemoryReference {

			get {
				
				return Execute<int> (MemoryBus.F_NextMemoryReference);
			
			}
		
		}

		#endregion
		#region IMemoryBus implementation
		
		public void SetShort (string key, object value)
		{

			Execute<KeyValuePair<string,object>> (MemoryBus.F_SetShort,
			                                 new KeyValuePair<string,object> (key, value));  
			
		}

		
		public T GetShort<T> (string key)
		{

			return Execute<T,string> (MemoryBus.F_GetShort, key);
		
		}
		
		public object GetShort (string key)
		{

			return Execute<object,string> (MemoryBus.F_GetShort, key);
		
		}
		
		public bool HasShort (string key)
		{

			return Execute<bool,string> (MemoryBus.F_HasShort, key);

		}

		#endregion
	
	}

}

