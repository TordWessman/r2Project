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
//
namespace R2Core
{
	/// <summary>
	/// Implementations allows member invocations using method/property handles as strings.
	/// </summary>
	public interface IInvokable {

		/// <summary>
		/// Set the variable identified by handle to value.
		/// </summary>
		/// <param name="handle">Handle.</param>
		/// <param name="value">Value.</param>
		void Set(string handle, dynamic value);

		/// <summary>
		/// Returns the value of variable identified by handle
		/// </summary>
		/// <param name="handle">Handle.</param>
		dynamic Get(string handle);

		/// <summary>
		/// Invokes a method using provided arguments(args) and returning result(or null if a void function invoced)
		/// </summary>
		/// <param name="args">Arguments.</param>
		dynamic Invoke(string handle, params dynamic[] args);

	}
}

