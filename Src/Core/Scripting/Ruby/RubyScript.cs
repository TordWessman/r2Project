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
using IronRuby.Builtins;
using System.Dynamic;
using System.Threading.Tasks;

namespace Core.Scripting
{
	/// <summary>
	/// Represents a runnable ruby script file following the specified template (scripts not conforming to the template will render error).
	/// </summary>
	public class RubyScript: ScriptBase, IScript
	{

		protected ScriptEngine m_engine;
		protected ScriptScope m_scope;
		protected ScriptSource m_source;

		/// <summary>
		/// The required name of the main class in each ruby script
		/// </summary>
		public const string HANDLE_MAIN_CLASS = "main_class";

		// The name of the ruby script file being executed
		protected string m_fileName;

		// Reference to the main class of the script
		private RubyObject m_mainClass;

		// Contains a list of input parameters
		private IDictionary<string, dynamic> m_params;

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

		public override void Reload ()
		{
			
			if (!File.Exists (m_fileName)) {

				throw new IOException ($"Ruby file does not exist: {m_fileName}");
			
			} else {
			
				Log.d ($"Loading script: {m_fileName}");
			
			}

			m_source = m_engine.CreateScriptSourceFromFile (m_fileName);

			try {

				m_source.Execute (m_scope);
			
			} catch (IronRuby.Builtins.SyntaxError ex) { HandleSyntaxException (ex); return; 
			} catch (Microsoft.Scripting.SyntaxErrorException ex) { HandleSyntaxException (ex); return; 
			} catch (System.ArgumentNullException ex) { HandleSyntaxException (ex); return; }

			System.Runtime.Remoting.ObjectHandle tmp;

			if (!m_scope.TryGetVariableHandle (HANDLE_MAIN_CLASS, out tmp)) {

				throw new ArgumentNullException ($"Unable to get main class: '{HANDLE_MAIN_CLASS}' from script: '{m_fileName}'" );

			}

			m_mainClass =  tmp?.Unwrap () as RubyObject;

			foreach (KeyValuePair<string, dynamic> kvp in m_params) {

				Set (kvp.Key, kvp.Value);

			}

			Invoke (HANDLE_INIT_FUNCTION);

		}

		public override bool Ready { get {return m_mainClass != null;} }

		public override void Set (string handle, dynamic value) {
			
			m_engine.Operations.SetMember (m_mainClass, handle, value);
		
		}
		
		public override dynamic Get (string handle) {

			if (!m_engine.Operations.ContainsMember (m_mainClass, handle)) {
			
				return null;

			}

			return m_engine.Operations.GetMember (m_mainClass, handle);

		} 

		public override dynamic Invoke (string handle, params dynamic[] args) {

			if (!m_engine.Operations.ContainsMember (m_mainClass, handle)) {

				throw new ArgumentException ($"Error in script: {m_fileName}. Handle '{handle}' was not declared in {HANDLE_MAIN_CLASS}.");
			
			}
				
			return m_engine.Operations.InvokeMember (m_mainClass, handle, args);

		}
		
		private void HandleSyntaxException (Exception ex) {

			string exception_message = m_engine.GetService<ExceptionOperations>().FormatException(ex);
			m_mainClass = null;
			Log.e ("Script: '{m_fileName}' contains syntax error and will not be executed: ");

			throw new ApplicationException($"Syntax exception.\n{exception_message}");

		}

	}

}