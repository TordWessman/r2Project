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
using R2Core.Device;


namespace R2Core.Memory
{
	public class MemoryFactory
	{
		IMemoryDBAdapter m_adapter;
		
		public MemoryFactory (IMemoryDBAdapter adapter)
		{
			m_adapter = adapter;
		}
		
		/// <summary>
		/// Creates a null reference (a serializable object not
		/// refering to anything).
		/// </summary>
		/// <returns>
		/// A reference regarded as a null reference
		/// </returns>
		public IMemoryReference CreateNullReference ()
		{
			return new MemoryReference (MemoryReference.NULL_REFERENCE_ID, "", "");
		}
		
		/// <summary>
		/// Creates a new IMemoryReference implementation.
		/// </summary>
		/// <returns>
		/// The memory reference.
		/// </returns>
		/// <param name='id'>
		/// Identifier.
		/// </param>
		/// <param name='name'>
		/// Name.
		/// </param>
		/// <param name='type'>
		/// Type.
		/// </param>
		public IMemoryReference StoreMemoryReference (int id, string name, string type)
		{
			IMemoryReference reference = new MemoryReference (id, name, type);
			m_adapter.Create (reference);
			
			return reference;
		}
		
		/// <summary>
		/// Creates a new IMemory implementation by providing a localized source 
		/// (IMemorySource) and a memory reference.
		/// </summary>
		/// <returns>
		/// The memory.
		/// </returns>
		/// <param name='reference'>
		/// Reference.
		/// </param>
		/// <param name='source'>
		/// Source.
		/// </param>
		public IMemory CreateMemory (IMemoryReference reference, IMemorySource source, bool isLocal)
		{

			return new Memory (reference, source, isLocal);

		}
	
	}

}

