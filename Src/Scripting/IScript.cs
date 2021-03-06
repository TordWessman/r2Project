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
using R2Core;


namespace R2Core.Scripting
{
	/// <summary>
	/// A script is a dynamically invocable instance of some sort of script (i.e. Python). The script should follow some standard rules regarding
	/// construction / run loop etc.
	/// </summary>
	public interface IScript : IDevice, IInvokable, ITaskMonitored {
		
		/// <summary>
		/// Reloads the script from script file.
		/// </summary>
		void Reload();

		/// <summary>
		/// Adds the observe which will be notified upon script changes.
		/// </summary>
		/// <param name="observer">Observer.</param>
		void AddObserver(IScriptObserver observer);

		/// <summary>
		/// Returns true if the process is in it's main loop.
		/// </summary>
		/// <value><c>true</c> if this instance is running; otherwise, <c>false</c>.</value>
		bool IsRunning { get; } 

	}

}