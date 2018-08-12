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
using System.Threading.Tasks;
using R2Core.Device;


namespace R2Core
{
	/// <summary>
	/// The task monitor is a primitive tool used to monitor tasks and to 
	/// display messages when they crashes
	/// </summary>
	public interface ITaskMonitor : IDevice
	{
		/// <summary>
		/// Adds a task to be monitored assiciated with an id
		/// </summary>
		/// <param name='task'>
		/// Task to be monitored
		/// </param>
		void AddTask (string id, Task task);
		
		/// <summary>
		/// Adds a ITaskMonitored instance (possibly containing several tasks to monitor)
		/// </summary>
		/// <param name='observer'>
		/// Observer.
		/// </param>
		void AddMonitorable (ITaskMonitored observer);

		Task MonitorTask {get;}
		/// <summary>
		/// Removes a ITaskMonitored and it's task.
		/// </summary>
		/// <param name='observer'>
		/// Observer.
		/// </param>
		void RemoveMonitorable (ITaskMonitored observer);

		void PrintTasks();
	}
}

