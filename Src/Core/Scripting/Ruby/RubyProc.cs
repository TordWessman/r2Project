﻿// This file is part of r2Poject.
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
using System.Dynamic;

namespace Core.Scripting
{
	public class RubyProc: DeviceBase, IScriptProcess
	{
		/// <summary>
		/// Instance variable of boolean type. If false, the run loop will quit after the current cycle. 
		/// </summary>
		public const string HANDLE_SHOULD_RUN = "should_run";

		/// <summary>
		/// Method handle to the scripts 'stop' method (termination).
		/// </summary>
		public const string HANDLE_STOP = "stop";

		/// <summary>
		/// Method handle to the run loop.
		/// </summary>
		public const string HANDLE_LOOP = "loop";

		/// <summary>
		/// Method handle for the setup function (which will be executed before each execution).
		/// </summary>
		public const string HANDLE_SETUP = "setup";	

		private bool m_isRunning;

		private ICollection<IScriptObserver> m_observers;

		private Task m_processTask;

		private IScript m_script;
		//private dynamic m_mainClass;

		public RubyProc (string id, IScript script) : base (id)
		{
			m_observers = new List<IScriptObserver> ();
			m_script = script;
			//m_mainClass = script.Get (RubyScript.HANDLE_MAIN_CLASS);

			m_processTask = GetProcessTask ();
		}

		public void Reload() {
		
			Stop ();
			m_script.Reload ();

		}

		private Task GetProcessTask () {
			
			return new Task (() => {

				try {

					while (false != m_script.Get(HANDLE_SHOULD_RUN) && true == m_script.Invoke(HANDLE_LOOP)) {

						// In the class' loop function

					}

				} catch (Exception ex) {
				
					Log.x(ex);

				}


				m_isRunning = false;

				foreach (IScriptObserver observer in m_observers) {

					observer?.ProcessDidFinish (m_id);
				
				}

				Log.d($"Loop did finnish for script {m_script.Identifier}.");

				Reload ();

			});

		}

		public override void Start () {
		
			if (!m_script.Ready) {

				Log.e ($"Unable to start process. The script used to run the process ({m_script.Identifier} is not ready.");

				return;

			}

			try {
				
				m_isRunning = true;
				m_script.Invoke(HANDLE_SETUP);
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
		
			m_script.Invoke (HANDLE_STOP);
			m_processTask = GetProcessTask ();

		}

		public IScript Script { get { return m_script; } }

		public override bool Ready { get { return m_script.Ready; } }

		public bool IsRunning { get { return m_isRunning; } }

		public IDictionary<string,Task> GetTasksToObserve () { return new Dictionary<string, Task>() {{"RUBY PROCESS: " + m_id, m_processTask} }; }

		public void AddObserver (IScriptObserver observer) { m_observers.Add (observer); }

	}
}

