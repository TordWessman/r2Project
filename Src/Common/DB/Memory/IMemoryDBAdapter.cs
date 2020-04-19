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
using R2Core.Data;

namespace R2Core.Common
{
	public interface IMemoryDBAdapter : IDBAdapter
	{
		/// <summary>
		/// Creates a new memory using reference.
		/// </summary>
		/// <param name="reference">Reference.</param>
		void Create(IMemoryReference reference);

		/// <summary>
		/// Returns a list of memories.
		/// </summary>
		/// <param name="type">Type.</param>
		System.Collections.Generic.ICollection<IMemoryReference> Get(string type);

		/// <summary>
		/// Returns the memories with reference ids.
		/// </summary>
		/// <param name="id">Identifier.</param>
		System.Collections.Generic.ICollection<IMemoryReference> Get(int[] ids);

		/// <summary>
		/// Updates the reference's name and type. Returns true if the reference was updated.
		/// </summary>
		/// <param name="reference">Reference.</param>
		bool Update(IMemoryReference reference);

		/// <summary>
		/// Returns everything...
		/// </summary>
		/// <returns>The all.</returns>
		System.Collections.Generic.ICollection<IMemoryReference> LoadAll();

		/// <summary>
		/// Deletes a memory using reference. Returns true if a memory was deleted.
		/// </summary>
		/// <param name="reference">Reference.</param>
		bool Delete(int memoryId);

	}
}

