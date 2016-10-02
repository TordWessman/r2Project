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

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using System.Collections.Generic;
using IronRuby;
using Microsoft.CSharp.RuntimeBinder;
using Core.Device;
using Core;
using System.Threading.Tasks;

namespace Core.Scripting
{
	/// <summary>
	/// Long running ruby script confirming to ruby templates for such..
	/// </summary>
	public class RubyProcess : RubyScript, IScriptProcess
	{
		public const string HANDLE_SHOULD_RUN = "main_class.should_run";		
		public const string HANDLE_STOP = "main_class.stop";	
		
		private bool m_hasStarted;
		private bool m_hasEnded;
		
		
		private Task m_processTask;

		public RubyProcess (string id, 
		                    string fileName, 
		                    ICollection<string> searchPaths, 
		                    IDeviceManager deviceManager): base (id, 
		                    fileName, 
		                    searchPaths, 
		                    deviceManager)
		{

			m_processTask = GetProcessTask ();

		}
		
		public override bool Ready { get {return IsRunning && m_mainClass != null;} }
		public Task Task { get { return m_processTask;}}
		
		private Task GetProcessTask ()
		{
			return new Task (() => {

				while (false != m_mainClass.@should_run()) {

					m_mainClass.@loop ();

				}

				Log.d ("Ruby process: " + m_id + " is done.");
				m_hasEnded = true;

				m_scope = m_engine.CreateScope ();
				
				foreach (IScriptObserver observer in m_observers) {

					if (observer != null) {

						observer.Finished (m_id);

					}

				}

			});
		
		}
		
		public bool HasStarted { get {
				return m_hasStarted;
			}
		}
		
		public bool HasEnded { get {
				return m_hasEnded;
			}
		}
		
		public override void Start ()
		{

			if (m_hasStarted) {

				throw new InvalidOperationException ("Cannot start ruby engine for file: " + m_fileName + ". Script already running.");
			
			}
			
			if (m_hasEnded) {

				Log.w ("Cannot start ruby engine for file: " + m_fileName + ". Script is done.");
				return;
			
			}
			
			m_hasStarted = true;
			try {
			
				m_mainClass.@setup ();
				m_processTask.Start ();

			} catch (Exception ex) {

				Log.x (ex);
			
			}

		}
		
		public override void Stop ()
		{
			
			if (m_mainClass != null) {

				m_mainClass.@stop();

			}

			m_hasStarted = false;
		}
	
		public bool IsRunning {

			get {

				if (m_mainClass == null) {

					return false;
				}

				return  m_mainClass.@should_run() == true && m_hasStarted;

			}

		}
	
		public IDictionary<string,Task> GetTasksToObserve ()
		{

			//Console.WriteLine ("RUBY PROCESS: " + m_id  + " status: " + m_processTask.Status.ToString() + " fault: " + m_processTask.IsFaulted.ToString() + " completed: " + m_processTask.IsCompleted );

			return new Dictionary<string, Task>() {{"RUBY PROCESS: " + m_id, m_processTask} };
		}


	}
}

