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

namespace Core.Scripting
{
	public interface IScriptObserver
	{
		/// <summary>
		/// Called whenever a process ran to end without errors.
		/// </summary>
		/// <param name="script">Script.</param>
		void OnScriptFinished (IScript script);

		/// <summary>
		/// Called upon exception.
		/// </summary>
		/// <param name="script">Script.</param>
		void OnScriptErrors (IScript script);

	}
}

