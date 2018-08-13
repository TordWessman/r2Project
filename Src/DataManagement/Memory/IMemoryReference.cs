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
using System.Net;
using MemoryType = System.String;

namespace R2Core.DataManagement.Memory
{

	/// <summary>
	/// A basic representation of a memory. The implementation
	/// should be serializable and thus, sharable among RobotInstances
	/// </summary>
	public interface IMemoryReference
	{

		/// <summary>
		/// Gets the identifier of the memory.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		int Id {get;}
		
		/// <summary>
		/// Gets the type of the memory.
		/// </summary>
		/// <value>
		/// The type.
		/// </value>
		MemoryType Type { get; set;}
		
		/// <summary>
		/// Gets the value of this memory.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		string Value { get; set;}

		/// <summary>
		/// Returns true if this reference is not pointing to any memory value.
		/// </summary>
		/// <value><c>true</c> if this instance is null; otherwise, <c>false</c>.</value>
		bool IsNull { get; }

	}

}

