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
using R2Core.Network;
using System.Net;
using System.Collections.Generic;
using MemoryType = System.String;

namespace R2Core.DataManagement.Memory
{
	/// <summary>
	/// Interface providing methods for interacting with the shared memory. 
	/// Every RobotInstance should have their own local IMemorySource. 
	/// Every memory source stores their memory localy on each RobotInstance,
	/// but they share the same IMemoryBus.
	/// </summary>
	public interface IMemorySource : IDevice
	{

		/// <summary>
		/// Returns the first occurrence of the memory of type.
		/// </summary>
		/// <param name="type">Type.</param>
		IMemory Get(MemoryType type);

		/// <summary>
		/// Get the specified IMemory by providing the id.
		/// </summary>
		/// <param name='memoryId'>
		/// Memory identifier.
		/// </param>
		IMemory Get(int memoryId);

		/// <summary>
		/// Returns a collection of memories matching the provided id's
		/// </summary>
		/// <param name="memoryIds">Memory identifiers.</param>
		ICollection<IMemory> Get(int[] memoryIds);

		/// <summary>
		/// Returns a collection of memories of the specified type or
		/// an empty list if no associations found.
		/// This method will retrieve all memories from all instances and might thus be expensive.
		/// </summary>
		/// <returns>
		/// The memories of type.
		/// </returns>
		/// <param name='type'>
		/// Type.
		/// </param>
		ICollection<IMemory> All(MemoryType type = null);

		/// <summary>
		/// Returns the associations of a specified memory reference or
		/// an empty list if no associations found.
		/// </summary>
		/// <returns>
		/// The associations.
		/// </returns>
		/// <param name='memory'>
		/// Reference.
		/// </param>
		ICollection<IMemory> GetAssociations(IMemory memory);
		
		/// <summary>
		/// Creates a new memory and returns it. Saves it in the local
		/// RobotInstance database.
		/// Broadcasts it's existance via
		/// the IMemoryBus.
		/// This method will call the MemoryBus which contains the
		/// shared association and a call to this method is thus, expensive.
		/// </summary>
		/// <param name='type'>
		/// Type.
		/// </param>
		/// <param name='name'>
		/// Name.
		/// </param>
		IMemory Create(MemoryType type, string name);

		/// <summary>
		/// Delete the specified memory locally and it's
		/// references. Returns true if memory was deleted.
		/// </summary>
		/// <param name='memory'>
		/// Memory.
		/// </param>
		bool Delete(int memoryId);
		bool Delete(IMemory memory);

		/// <summary>
		/// Update the specified memory.
		/// Returns true if memory was updated successfully.
		/// </summary>
		/// <param name='memory'>
		/// Memory.
		/// </param>
		bool Update(IMemory memory);

		/// <summary>
		/// Associate the two memories.
		/// This method will call the MemoryBus which contains the
		/// shared association and a call to this method is thus, expensive.
		/// </summary>
		/// <param name='one'>
		/// One.
		/// </param>
		/// <param name='two'>
		/// Two.
		/// </param>
		void Associate(IMemory one, IMemory two);

	}

}

