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
using System;
using Core.Device;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Core.Scripting
{
	public class RubyProc: DeviceBase, IScriptProcess
	{
		public const string HANDLE_SHOULD_RUN = "main_class.should_run";		
		public const string HANDLE_STOP = "main_class.stop";	

		private bool m_isRunning;

		private ICollection<IScriptObserver> m_observers;

		private Task m_processTask;

		private IScript m_script;

		public RubyProc (string id, IScript script) : base (id)
		{
			m_observers = new List<IScriptObserver> ();
			m_script = script;

			//TODO: Check all required functions

			m_processTask = GetProcessTask ();
		}

		public void Reload() {
		
			Stop ();
			m_script.Reload ();

		}

		private Task GetProcessTask () {
			
			return new Task (() => {

				m_script?.MainClass?.@enable();

				while (false != m_script.MainClass?.@should_run()) {

					m_script.MainClass.@loop ();

				}

				m_isRunning = false;

				foreach (IScriptObserver observer in m_observers) {

					observer?.ProcessDidFinish (m_id);
				
				}

			});

		}

		public override void Start () {
		
			if (!m_script.Ready) {

				Log.e ("Unable to start process. The script used to run the process (" + m_script.Identifier +  ") is not ready.");

				return;

			}

			try {
				
				m_isRunning = true;
				m_script.MainClass.@setup ();
				m_processTask.Start ();

			} catch (Exception ex) {

				m_isRunning = false;

				Log.x (ex);

				foreach (IScriptObserver observer in m_observers) {

					observer?.ProcessHadErrors (m_id);

				}

			}

		}

		public override void Stop () {
		
			m_script?.MainClass?.@disable();
			m_processTask = GetProcessTask ();

		}

		public IScript Script { get { return m_script; } }

		public override bool Ready { get { return m_script.Ready; } }

		public bool IsRunning { get { return m_isRunning; } }

		public IDictionary<string,Task> GetTasksToObserve () { return new Dictionary<string, Task>() {{"RUBY PROCESS: " + m_id, m_processTask} }; }

		public void AddObserver (IScriptObserver observer) { m_observers.Add (observer); }

	}
}

