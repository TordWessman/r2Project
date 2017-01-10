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

namespace GPIO
{
	/// <summary>
	/// A servo controller represents an entity capable of controlling a number of IServo
	/// </summary>
	public interface IServoController: IDevice
	{
		/// <summary>
		/// Set the value of servo <item>channel</item> to <para>value</para>.
		/// </summary>
		/// <param name="channel">Channel.</param>
		/// <param name="value">Value.</param>
		void Set (int channel, int value);
		
		IServo CreateServo (string id, int channel);
	}
}
