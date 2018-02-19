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
using System.Dynamic;
using Core.Device;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Core.Scripting
{

	/// <summary>
	/// In order to remove a lot of anoying debug output. Should not be used.
	/// </summary>
	public class ScriptException: Exception {

		private string m_stacktrace;

		public ScriptException(Exception exception) : base(exception.Message, exception.InnerException) {
			
			m_stacktrace = String.Join(Environment.NewLine, exception.StackTrace.Split (new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToArray ().Where(line => !(new List<string> () { "Microsoft.Scripting.", "System.Dynamic.UpdateDelegates", ".Runtime.Calls.", "Microsoft.Scripting.Hosting", "(wrapper dynamic-method)", "(wrapper delegate-invoke)", "(wrapper remoting-invoke-with-check)" }).Any(l => line.Contains(l))).ToArray().Where(line => line != Environment.NewLine));

		}

		public override string StackTrace  { get { return m_stacktrace; } }

	}
	
	/// <summary>
	/// IScript implementations should inherit from ScriptBase, since it's usingthe DynamicObject features for method and member access.
	/// </summary>
	public abstract class ScriptBase: DynamicObject, IScript 
	{
		/// <summary>
		/// Will be called upon script initialization. This method must be defined in the MainClass. Defined in scriptbase.rb
		/// </summary>
		public const string HANDLE_INIT_FUNCTION = "r2_init";

		/// <summary>
		/// Instance variable of boolean type. If false, the run loop will quit after the current cycle. 
		/// </summary>
		public const string HANDLE_SHOULD_RUN = "should_run";

		/// <summary>
		/// (Optional) Method handle to the scripts 'stop' method (termination).
		/// </summary>
		public const string HANDLE_STOP = "stop";

		/// <summary>
		/// Method handle to the run loop.
		/// </summary>
		public const string HANDLE_LOOP = "loop";

		/// <summary>
		/// (Optional) Method handle for the setup function (which will be executed before each execution).
		/// </summary>
		public const string HANDLE_SETUP = "setup";	

		protected string m_id;
		protected Guid m_guid;
		private IList<IDeviceObserver> m_deviceObservers;
		private IList<IScriptObserver> m_scriptObservers;
		private bool m_isRunning;
		// The task running the loop
		private Task m_processTask;


		public ScriptBase (string id) {
			
			m_id = id;
			m_guid = Guid.NewGuid ();
			m_deviceObservers = new List<IDeviceObserver> ();
			m_scriptObservers = new List<IScriptObserver> ();

		}

		public string Identifier { get { return m_id; } }
		public Guid Guid { get { return m_guid; } }
		public virtual bool Ready { get { return true; } }
		public bool IsRunning { get { return m_isRunning; } }

		public abstract void Set (string handle, dynamic value);
		public abstract dynamic Get (string handle);
		public abstract void Reload();
		public abstract dynamic Invoke (string handle, params dynamic[] args);

		public void AddObserver (IScriptObserver observer) { m_scriptObservers.Add (observer); }
		public void AddObserver (IDeviceObserver observer) { m_deviceObservers.Add (observer); }

		private Task GetProcessTask () {

			return new Task (() => {

				try {

					while (m_isRunning && (false != Get(HANDLE_SHOULD_RUN)) && (true == Invoke(HANDLE_LOOP))) {

						// In the class' loop function

					}

				} catch (Exception ex) {

					Log.x(ex);

				}


				m_isRunning = false;

				foreach (IScriptObserver observer in m_scriptObservers) {

					observer?.OnScriptFinished (this);

				}

				Log.d($"Loop did finish for script {Identifier}.");

			});

		}

		public void Start () {

			if (!Ready) {

				throw new ApplicationException ($"Unable to start process. The script used to run the process ({Identifier} is not ready.");

			}

			try {

				m_isRunning = true;

				m_processTask = GetProcessTask ();

				// Run setup method if present
				if (null != Get (HANDLE_SETUP)) { 

					Invoke (HANDLE_SETUP); 

				} else { 

					Log.w ($"Warning: Script '{Identifier}' is missing setup method."); 
				
				} 

				m_processTask.Start ();

			} catch (Exception ex) {

				m_isRunning = false;

				foreach (IScriptObserver observer in m_scriptObservers) { observer?.OnScriptErrors (this); }
				ScriptException exception = new ScriptException (ex);
				Log.x (exception);

				throw exception;

			}

		}

		public void Stop () {

			if (null != Get (HANDLE_STOP)) { Invoke (HANDLE_STOP); }

			m_isRunning = false;

		}

		public override bool TrySetMember (SetMemberBinder binder, object value) {

			Set (binder.Name, value);
			return true;

		}

		public override bool TryGetMember (GetMemberBinder binder, out object result) {
		
			result = Get (binder.Name);
			return true;

		}

		public override bool TryInvokeMember (InvokeMemberBinder binder, object[] args, out object result)
		{
			
			result = Invoke (binder.Name, args);
			return true;

		}


		public IDictionary<string,Task> GetTasksToObserve () { return new Dictionary<string, Task>() {{"SCRIPT: " + m_id, m_processTask} }; }

	}

}