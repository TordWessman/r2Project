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

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using R2Core.Device;


namespace R2Core
{
	public class SimpleTaskMonitor : DeviceBase, ITaskMonitor {
		
		private IDictionary<string, Task> m_tasks;

        public Task MonitorTask { get; private set; }
        private bool m_shouldRun;
		private static readonly object m_assignLock = new object();
		
		public SimpleTaskMonitor(string deviceId) : base(deviceId) {
			m_shouldRun = true;
			m_tasks = new Dictionary<string,Task>();
			MonitorTask = new Task(() => {
				
				while(m_shouldRun) {
					
					lock(m_assignLock) {
						
						List<string> done = new List<string>();
						List<string> faulted = new List<string>();
						
						foreach (string id in m_tasks.Keys) {
							
							if (m_tasks[id] != null) {
								if (m_tasks[id].Status == TaskStatus.RanToCompletion) {
									done.Add(id);
								} else if (m_tasks[id].Status == TaskStatus.Faulted) {
									faulted.Add(id);
								}
								                                                        
								
							}
							
						}
						
						foreach (string id in done) {
							m_tasks.Remove(id);
						}
						
						foreach (string id in faulted) {
							Log.x(m_tasks[id].Exception);
							m_tasks.Remove(id);
						}
				
					}
					
					Thread.Sleep(2000);
				}
			});
		}

		#region ITaskMonitor implementation
		public void AddTask(string id, System.Threading.Tasks.Task task) {
			lock(m_assignLock) {
				
				if (m_tasks.ContainsKey(id)) {
					id = id + task.GetHashCode().ToString();
				}

				if (m_tasks.ContainsKey(id)) {
				
					m_tasks.Remove(id);

					Log.w("AddTask encountered duplicate. Removing previous instance of task: " + id);

					m_tasks.Add(id, task);
				}
				
			}

		}

		public void AddMonitorable(ITaskMonitored observer) {

			foreach (string id in observer.GetTasksToObserve().Keys) {
				AddTask(id, observer.GetTasksToObserve()[id]);
			}

		}

		public override void Start() {
			m_shouldRun = true;
            MonitorTask.Start();
		}

		public override void Stop() {
			m_shouldRun = false;
			PrintTasks();
		}

		public void RemoveMonitorable(ITaskMonitored observer) {
			lock(m_assignLock) {
				List<string> ids = new List<string>();
				
				foreach (string id in observer.GetTasksToObserve().Keys) {
					ids.Add(id);
					
				}
				
				foreach (string id in ids) {
					if (m_tasks.ContainsKey(id)) {
						m_tasks.Remove(id);
					}
				}
				
			}
		}

		public void PrintTasks() {
			int workerThread = 0, other = 0;
			ThreadPool.GetAvailableThreads(out workerThread, out other);
			Log.d("Number of worker threads: " + workerThread + " completionPortThreads: " + other + " ");
			
			lock(m_assignLock) {
				foreach (string id in m_tasks.Keys) {
					Log.d("[" + id + "]" + (m_tasks[id] != null ? m_tasks[id].Status.ToString() + " (" + m_tasks[id].GetHashCode()  + ") " : "(null)"));
				}
				
			}
		}

        #endregion
    }
}

