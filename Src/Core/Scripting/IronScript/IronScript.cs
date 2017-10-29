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
using System.IO;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Dynamic;
using System.Collections.Generic;
using IronPython.Runtime;
using System.Linq;

namespace Core.Scripting
{
	/// <summary>
	/// Default script implementation for IronRuby & IronPython
	/// </summary>
	public class IronScript: ScriptBase
	{
		protected ScriptEngine m_engine;
		protected ScriptScope m_scope;
		protected ScriptSource m_source;

		/// <summary>
		/// The required name of the main class in each ruby script
		/// </summary>
		public const string HANDLE_MAIN_CLASS = "main_class";

		protected string m_fileName;

		// Reference to the main class of the script
		private dynamic m_mainClass;

		// Contains a list of input parameters
		private IDictionary<string, dynamic> m_params;

		public IronScript(string id, string fileName, ScriptEngine engine, IDictionary<string, dynamic> parameters) : base (id)
		{

			m_engine = engine;
			m_scope =  m_engine.CreateScope();
			m_fileName = fileName;
			m_params = parameters ?? new Dictionary<string,dynamic>();
		
			Reload ();
		}

		public void AddSearchPath(string searchPath) {
		
			m_engine.SetSearchPaths (m_engine.GetSearchPaths ().Concat (new List<string> () { searchPath }).ToList());
		
		}

		public override void Reload ()
		{

			if (!File.Exists (m_fileName)) {

				throw new IOException ($"Python file does not exist: {m_fileName}");

			} else {

				Log.d ($"Loading script: {m_fileName}");

			}

			m_source = m_engine.CreateScriptSourceFromFile (m_fileName);

			try {

				m_source.Execute (m_scope);

			} catch (Exception ex) {
			
				Log.x (ex);

				throw ex;

			}

			System.Runtime.Remoting.ObjectHandle tmp;

			if (!m_scope.TryGetVariableHandle (HANDLE_MAIN_CLASS, out tmp)) {
				
				throw new ArgumentNullException ($"Unable to get main class: '{HANDLE_MAIN_CLASS}' from script: '{m_fileName}'" );

			}

			m_mainClass =  tmp?.Unwrap();

			foreach (KeyValuePair<string, dynamic> kvp in m_params) {

				Set (kvp.Key, kvp.Value);

			}

			Invoke (HANDLE_INIT_FUNCTION);

		}

		public override bool Ready { get { return base.Ready && m_mainClass != null;} }

		public override void Set (string handle, dynamic value) {

			m_engine.Operations.SetMember (m_mainClass, handle, value);

		}

		public override dynamic Get (string handle) {

			if (!Ready || !m_engine.Operations.ContainsMember (m_mainClass, handle)) {

				return null;

			}

			return m_engine.Operations.GetMember (m_mainClass, handle);

		} 

		public override dynamic Invoke (string handle, params dynamic[] args) {
			
			if (!m_engine.Operations.ContainsMember (m_mainClass, handle)) {

				throw new ArgumentException ($"Error in script: {m_fileName}. Handle '{handle}' was not declared in {HANDLE_MAIN_CLASS}.");

			}

			dynamic member = m_engine.Operations.GetMember (m_mainClass, handle);

			if (member is IronPython.Runtime.Method || member is IronRuby.Runtime.Calls.RubyMethodInfo || member is IronRuby.Builtins.RubyMethod) {
			
				// Invoke as method
				return m_engine.Operations.InvokeMember (m_mainClass, handle, args);

			}

			// Treat as property
			return member;

		}

	}

}
