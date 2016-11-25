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
using Microsoft.Scripting.Hosting;
using System.Collections.Generic;
using IronRuby;
using System.IO;
using System.Linq;

namespace Core.Scripting
{
	/// <summary>
	/// Represents a runnable ruby script file following the specified template (scripts not conforming to the template will render error).
	/// </summary>
	public abstract class RubyScript : DeviceBase, IScript
	{
		public const string HANDLE_MAIN_CLASS = "main_class";
		public const string HANDLE_SETUP = "main_class.setup";	
		public const string HANDLE_ROBOT = "robot";
		public const string HANDLE_ARGS = "args";

		protected string m_fileName;
		protected ScriptEngine m_engine;
		protected ScriptScope m_scope;
		protected ScriptSource m_source;
		protected IDeviceManager m_deviceManager;
		protected dynamic m_mainClass;
		protected bool m_hasSyntaxErrors;
		
		protected ICollection<IScriptObserver> m_observers;
		
		public RubyScript (string id, 
		                    string fileName, 
		                    ICollection<string> searchPaths, 
		                    IDeviceManager deviceManager) : base (id)
		{

			m_fileName = fileName;
			m_engine = Ruby.CreateEngine ();
			m_scope = m_engine.CreateScope ();
			m_engine.SetSearchPaths (searchPaths);
			m_deviceManager = deviceManager;
			m_observers = new List<IScriptObserver> ();

			Log.t (fileName);

			Init ();

		}
		
		protected void Init ()
		{
			m_hasSyntaxErrors = false;
			
			if (!File.Exists (m_fileName)) {

				throw new IOException ("Ruby file does not exist: " + m_fileName);
			
			} else {
			
				Log.d ("Loading script: " + m_fileName);
			
			}

			m_scope.SetVariable (HANDLE_ROBOT, m_deviceManager);
			m_source = m_engine.CreateScriptSourceFromFile (m_fileName);

			try {

				m_source.Execute (m_scope);
			
			} catch (IronRuby.Builtins.SyntaxError ex) {
			
				HandleSyntaxException (ex);
				return;
			
			} catch (Microsoft.Scripting.SyntaxErrorException ex) {
			
				HandleSyntaxException (ex);
				return;
			
			}
			
			System.Runtime.Remoting.ObjectHandle tmp;
			
			if (!m_scope.TryGetVariableHandle (HANDLE_MAIN_CLASS, out tmp)) {

				throw new ApplicationException ("ERROR: no " + HANDLE_MAIN_CLASS + " defined for Ruby process: " + m_fileName);	

			}
				
			m_mainClass = m_scope.GetVariable (HANDLE_MAIN_CLASS);

		}
		
		public bool HasSyntaxErrors { get { return m_hasSyntaxErrors; } }
		
		public override bool Ready { get {return m_mainClass != null;} }
		
		public void SetArgs (object[] args)
		{

			Set (HANDLE_ARGS, args);
		
		}
		
		public void Set (string handle, object value)
		{
			if (m_mainClass == null) {

				throw new InvalidOperationException ("Unable to set variable for script: " + m_fileName + " with id: " + m_id + " since it has not been executed.");
			
			}
			
			m_scope.SetVariable (handle, value);
		
		}
		
		public object Get (string handle)
		{
			if (m_mainClass == null) {

				throw new InvalidOperationException ("Unable to get variable for script: " + m_fileName + " with id: " + m_id + " since it has not been executed.");
			
			}
			
			System.Runtime.Remoting.ObjectHandle tmp;
			
			if (!m_scope.TryGetVariableHandle (handle, out tmp)) {

				Log.e ("Unable to get handle: " + handle + " from script: " + Identifier);
			
			}
			
			return tmp.Unwrap();
		}
		
		private void HandleSyntaxException (Exception ex)
		{
			
			m_hasSyntaxErrors = true;
			m_mainClass = null;
			Log.e ("Script: " + m_fileName + " contains syntax error and will not be started: ");
			Log.x (ex);

		}
	
		#region IScript implementation
		

		public void AddObserver (IScriptObserver observer)
		{

			m_observers.Add (observer);

		}
		#endregion
		
		public T GetTyped<T> (string methodHandle)
		{	

			return m_scope.GetVariable<T> (methodHandle);

		
		}
	}

}

