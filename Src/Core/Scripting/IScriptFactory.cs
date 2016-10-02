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
using Core.Device;


namespace Core.Scripting
{
	public interface IScriptFactory : IDevice
	{
		/// <summary>
		/// Creates a looping process conforming to structural requirements (i.e. naming, composition) of the implemented language. The surceFile is the file containing the source code. It may be a relative or an absolute path.
		/// </summary>
		/// <returns>The process.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="sourceFile">Source file.</param>
		/// <param name="args">Arguments.</param>
		IScriptProcess CreateProcess (string id, string sourceFile = null, object [] args = null);

		/// <summary>
		/// Executes a script once.
		/// </summary>
		/// <returns>The command.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="sourceFile">Source file.</param>
		ICommandScript CreateCommand (string id, string sourceFile = null);
		
		/// <summary>
		/// Gets the default name of the script source file based upon the id.
		/// </summary>
		/// <returns>
		/// A script file name
		/// </returns>
		/// <param name='id'>
		/// Identifier. The id of the script to evaluate the name of
		/// </param>
		string GetSourceFileName (string id);
	}
}

