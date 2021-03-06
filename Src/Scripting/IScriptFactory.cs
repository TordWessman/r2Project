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
using R2Core.Device;
using R2Core.Network;


namespace R2Core.Scripting
{
	public interface IScriptFactory<T>: IDevice where T: IScript {
		
		/// <summary>
		/// Creates an initialized ´IronScript´ object of type ´T´. 
		/// ´name´ will be used to determine the file name of the script. The optional
		/// ´id´ will be the device Identifier of the script. ´name´ will be use as Identifier
		/// if ´id´ is not set.
		/// </summary>
		/// <returns>The script.</returns>
		/// <param name="name">Name component of the script file.</param>
		/// <param name="id">Optional Identifier.</param>
		T CreateScript(string name, string id = null);

		/// <summary>
		/// Creates a script interpreter capable of evaluating string expressions. The requirements of the implementations of the interpreted script is dependent on the IScriptInterpreter implementation.
		/// </summary>
		/// <returns>The interpreter.</returns>
		/// <param name="script">Script.</param>
		IScriptInterpreter CreateInterpreter(T script);

		/// <summary>
		/// Creates an ´IWebEndpoint´ wrapping a script and calling the scripts on_receive method.
		/// </summary>
		/// <returns>The endpoint.</returns>
		/// <param name="script">Script.</param>
		/// <param name="path">Path.</param>
		IWebEndpoint CreateEndpoint(T script, string path);

		/// <summary>
		/// Adds a search path for script source files.
		/// </summary>
		/// <param name="path">Path.</param>
		void AddSourcePath(string path);

		/// <summary>
		/// Gets the default name of the script source file based upon the id.
		/// </summary>
		/// <returns>
		/// A script file name
		/// </returns>
		/// <param name='id'>
		/// Identifier. The id of the script to evaluate the name of
		/// </param>
		string GetScriptFilePath(string id);

	}

}