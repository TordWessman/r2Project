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


namespace Core.Device
{
	public interface IInputPort : IDevice
	{
		bool Value { get;}

		/// <summary>
		/// Triggers the func on every interval read when set. Use StepTrigger to quit
		/// if pullUp is true, the event will be triggered when the input is low
		/// </summary>
		/// <param name="triggerFunc">Trigger func.</param>
		void SetTrigger(Func<object> triggerFunc, bool pullUp = false, int interval = 100);

		/// <summary>
		/// Stop the clock from triggering new calls
		/// </summary>
		void StopTrigger();

	}
}

