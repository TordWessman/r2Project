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
	public class RubyScript : DeviceBase, IScript
	{
		/// <summary>
		/// The required name of the main class in each ruby script
		/// </summary>
		public const string HANDLE_MAIN_CLASS = "main_class";

		/// <summary>
		/// The object which will contain the device manager. Should be set before execution.
		/// </summary>
		public const string HANDLE_DEVICES = "robot";

		protected ScriptEngine m_engine;
		protected ScriptScope m_scope;
		protected ScriptSource m_source;

		// The name of the ruby script file being executed
		protected string m_fileName;

		// Reference to the main class of the script
		private dynamic m_mainClass;

		// Contains a list of input parameters
		private IDictionary<string, dynamic> m_params;

		// If syntax error exception occurs, this will be true;
		private bool m_hasSyntaxErrors;

		//public dynamic MainClass { get { return m_mainClass; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="Core.Scripting.RubyScript"/> class. id is the Device Identifier of the script. fileName is the absolute path to the file being executed, engine is the RubyEngine and parameters are a list of input parameters being set before execution.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="fileName">File name.</param>
		/// <param name="engine">Engine.</param>
		/// <param name="parameters">Parameters.</param>
		public RubyScript (string id, string fileName, ScriptEngine engine, IDictionary<string, dynamic> parameters) : base (id)
		{

			m_fileName = fileName;
			m_engine = engine;
			m_scope = m_engine.CreateScope ();
			m_params = parameters;

			Reload ();

		}

		public void Reload ()
		{
			m_hasSyntaxErrors = false;
			
			if (!File.Exists (m_fileName)) {

				throw new IOException ($"Ruby file does not exist: {m_fileName}");
			
			} else {
			
				Log.d ($"Loading script: {m_fileName}");
			
			}

			m_source = m_engine.CreateScriptSourceFromFile (m_fileName);

			foreach (KeyValuePair<string, dynamic> kvp in m_params) {
			
				Set (kvp.Key, kvp.Value);

			}

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

		public void Set (string handle, dynamic value) {
			
			m_scope.SetVariable (handle, value);
		
		}
		
		public dynamic Get (string handle) {

			System.Runtime.Remoting.ObjectHandle tmp;
			
			if (!m_scope.TryGetVariableHandle (handle, out tmp)) {

				Log.e ($"Unable to get handle: {handle} from script: {Identifier}" );
			
			}
			
			return tmp.Unwrap();

		} 

		public dynamic Invoke (string handle, params dynamic[] args) {

			return m_engine.Operations.InvokeMember (m_mainClass, handle, args);

		}
		
		private void HandleSyntaxException (Exception ex) {
			
			m_hasSyntaxErrors = true;
			m_mainClass = null;
			Log.e ("Script: '{m_fileName}' contains syntax error and will not be executed: ");
			Log.x (ex);

		}

	}

}