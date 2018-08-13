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
using System.Collections.Generic;
using MemoryType = System.String;

namespace R2Core.DataManagement.Memory
{
	/// <summary>
	/// The IMemory is a "less abstract embodiment"  of
	/// the IMemoryReference.
	/// 
	/// </summary>
	public interface IMemory : IMemoryReference
	{

		/// <summary>
		/// Returns true if this memory is local to this instance.
		/// </summary>
		/// <value><c>true</c> if this instance is local; otherwise, <c>false</c>.</value>
		bool IsLocal { get; }

		/// <summary>
		/// Gets the reference (id) of the memory 
		/// </summary>
		/// <value>
		/// The reference.
		/// </value>
		IMemoryReference Reference {get;}
		
		/// <summary>
		/// Returns a collection of all associations of the memory.
		/// This method will call the MemoryBus which contains the
		/// shared association and a call to this method is thus, expensive.
		/// </summary>
		/// <value>
		/// The associations.
		/// </value>
		ICollection<IMemory> Associations {get;}
		
		/// <summary>
		/// Creates an association between this memory and
		/// the provided IMemory.
		/// This method will call the MemoryBus which contains the
		/// shared association and a call to this method is thus, expensive.
		/// </summary>
		/// <param name='other'>
		/// Other.
		/// </param>
		void Associate (IMemory other);
		
		/// <summary>
		/// Gets the first occuring association of a specific type or 
		/// Null if no memory association of this type is found.
		/// This method will call the MemoryBus which contains the
		/// shared association and a call to this method is thus, expensive.
		/// </summary>
		/// <returns>
		/// The association.
		/// </returns>
		/// <param name='type'>
		/// Type.
		/// </param>
		IMemory GetAssociation (MemoryType type);
		
		/// <summary>
		/// Gets the associations of a specific type
		/// </summary>
		/// <returns>
		/// The associations.
		/// </returns>
		/// <param name='type'>
		/// Type.
		/// </param>
		ICollection<IMemory> GetAssociations (MemoryType type);

	}
}

