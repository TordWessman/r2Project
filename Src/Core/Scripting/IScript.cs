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
using Core;


namespace Core.Scripting
{
	/// <summary>
	/// Base interface for scripting 
	/// </summary>
	public interface IScript : IDevice
	{

		/// <summary>
		/// Returns the script main class 
		/// </summary>
		/// <value>The main class.</value>
		dynamic MainClass { get; }

		/// <summary>
		/// Set the variable identified by handle to value.
		/// </summary>
		/// <param name="handle">Handle.</param>
		/// <param name="value">Value.</param>
		void Set (string handle, object value);

		/// <summary>
		/// Returns the value of variable identified by handle
		/// </summary>
		/// <param name="handle">Handle.</param>
		object Get (string handle);

		/// <summary>
		/// Returns a variable or method typed as T. (i.e: a method, f(Y) -> X, would be specified as Func<X,Y> 
		/// </summary>
		/// <returns>The typed.</returns>
		/// <param name="methodHandle">Method handle.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		T GetTyped<T> (string methodHandle);

		/// <summary>
		/// Reloads the script from script file.
		/// </summary>
		void Reload();
	}
}

