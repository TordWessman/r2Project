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
using System.Net;
using System.Collections.Generic;
using MemoryType = System.String;

namespace R2Core.Common
{
	/// <summary>
	/// The IMemoryBus deals with low-level memories(IMemoryReferences).
	/// This includes the memory associations of each memory.
	/// The IMemoryBus should be shared among RobotInstances, and
	/// there must  be only one non-remote implementation.
	/// </summary>
	public interface IMemoryBus : IDevice
	{
		/// <summary>
		/// Adds a collection of IMemoryReferences to the local(temporary - RAM) memory.
		/// Will omit duplicates.
		/// </summary>
		/// <param name='memories'>
		/// A ICollection<IMemoryReference> to add.
		/// </param>
		void AddMemories(ICollection<IMemoryReference> memories);
		
		/// <summary>
		/// Creates an association between two IMemoryReferences
		/// </summary>
		/// <param name='first'>
		/// First.
		/// </param>
		/// <param name='second'>
		/// Second.
		/// </param>
		void AddAssociation(IMemoryReference first, IMemoryReference second);
		
		/// <summary>
		/// Gets the associations of memory with a specified id
		/// </summary>
		/// <returns>
		/// A list of associations or a empty list if 
		/// no association was found
		/// </returns>
		/// <param name='memoryId'>
		/// Memory identifier.
		/// </param>
		ICollection<IMemoryReference> GetAssociations(int memoryId);
		
		/// <summary>
		/// Gets the IMemoryReference of a specific memory
		/// </summary>
		/// <returns>
		/// An IMemoryReference
		/// </returns>
		/// <param name='memoryId'>
		/// Memory identifier.
		/// </param>
		IMemoryReference GetReference(int memoryId);
		
		/// <summary>
		/// Gets the IMemoryReferences of a specific type
		/// </summary>
		/// <returns>
		/// An ICollection<IMemoryReference> containing all
		/// memories of the specified type found or an empty
		/// list if no reference was found.
		/// </returns>
		/// <param name='type'>
		/// The memory type requested.
		/// </param>
		ICollection<IMemoryReference> GetReferences(MemoryType type);
		
		/// <summary>
		/// Removes a memory references and its associations from RAM and disk.
		/// </summary>
		/// <param name='memory'>
		/// The memory reference no remove.
		/// </param>
		void RemoveMemory(IMemoryReference memory);
		
		/// <summary>
		/// Gets the next unique(LOL) memory reference.
		/// </summary>
		/// <value>
		/// The next memory reference.
		/// </value>
		int NextMemoryReference {get;}
		
		/// <summary>
		/// Sets a short term memory
		/// </summary>
		/// <param name='key'>
		/// The key used as a reference to the memory
		/// </param>
		/// <param name='value'>
		/// The object stored in the short term memory
		/// </param>
		void SetShort(string key, object value);
		
		/// <summary>
		/// Gets the short term memory id
		/// </summary>
		/// <returns>
		/// The id of the IMemory or IMemoryReference associated
		/// </returns>
		/// <param name='key'>
		/// Key.
		/// </param>
		T GetShort<T>(string key);
		
		object GetShort(string key);
		
		/// <summary>
		/// Determines whether this instance has the specified short term memory.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance has the specified key for the short term memory; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='key'>
		/// If set to <c>true</c> key.
		/// </param>
		bool HasShort(string key);
		//IMemoryReference GetReferenceByName(MemoryType type, string name);
		
		//IMemoryReference GetReference(long memoryId);
		//void RemoveMemories(long[]memoryIds);
	}
}

