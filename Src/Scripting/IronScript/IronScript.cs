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
using Microsoft.Scripting.Hosting;
using System.Collections.Generic;
using System.Linq;

namespace R2Core.Scripting
{
	/// <summary>
	/// Default script implementation for IronPython
	/// </summary>
	public class IronScript : ScriptBase {
		
		protected ScriptEngine m_engine;
		protected ScriptScope m_scope;
		protected ScriptSource m_source;

        /// <summary>
        /// Contains code segments to be prepended to the actual script source.
        /// </summary>
        /// <value>The prepended code.</value>
        public IList<string> PrependedCode { get; private set; }

        /// <summary>
        /// Contains code segments to be appended to the actual script source.
        /// </summary>
        /// <value>The prepended code.</value>
        public IList<string> AppendedCode { get; private set; }

        /// <summary>
        /// The required name of the main class in each python script
        /// </summary>
        public const string HANDLE_MAIN_CLASS = "main_class";

		protected string m_fileName;

		// Reference to the main class of the script
		private dynamic m_mainClass;

		// Contains a list of input parameters
		private IDictionary<string, dynamic> m_params;

		public IronScript(string id, string fileName, ScriptEngine engine, IDictionary<string, dynamic> parameters) : base(id) {

			m_engine = engine;
            m_scope =  m_engine.CreateScope();
			m_fileName = fileName;
			m_params = parameters ?? new Dictionary<string,dynamic>();
            PrependedCode = new List<string>();
            AppendedCode = new List<string>();

        }

		public void AddSearchPath(string searchPath) {

			m_engine.SetSearchPaths(m_engine.GetSearchPaths().Concat(new List<string> { searchPath }).ToList());
		
		}

		public override void Reload() {

			Stop();

			if (!File.Exists(m_fileName)) {

				throw new ScriptException($"Python file does not exist: {m_fileName}");

			}

            Log.i($"Loading script: {m_fileName}");

            string scriptString = string.Join(Environment.NewLine, new string[] {
                string.Join(Environment.NewLine, PrependedCode),
                File.ReadAllText(m_fileName),
                string.Join(Environment.NewLine, PrependedCode) });

            m_source = m_engine.CreateScriptSourceFromString(scriptString);

            try {

				m_source.Execute(m_scope);

			} catch (Exception ex) {

                Log.e(m_engine.GetService<ExceptionOperations>().FormatException(ex).Replace("File \"\"", $"File \"{m_fileName}\""));
                Log.x(ex);

				throw ex;

			}

            if (!m_scope.TryGetVariableHandle(HANDLE_MAIN_CLASS, out System.Runtime.Remoting.ObjectHandle mainClassHandle)) {

                throw new ScriptException($"Unable to get main class: '{HANDLE_MAIN_CLASS}' from script: '{m_fileName}'");

            }

            m_mainClass =  mainClassHandle?.Unwrap();

			foreach (KeyValuePair<string, dynamic> kvp in m_params) {

				Set(kvp.Key, kvp.Value);

			}

            if (m_engine.Operations.ContainsMember(m_mainClass, HANDLE_INIT_FUNCTION)) {

                Invoke(HANDLE_INIT_FUNCTION);

            }

		}

		public override bool Ready { get { return base.Ready && m_mainClass != null;} }

		public override void Set(string handle, dynamic value) {

            try {

                m_engine.Operations.SetMember(m_mainClass, handle, value);

            } catch (Exception ex) {

                Log.e(GetExceptionInfoString(ex));
                throw ex;

            }

		}

		public override dynamic Get(string handle) {

			if (!Ready || !m_engine.Operations.ContainsMember(m_mainClass, handle)) {

				return null;

			}

            try {

                return m_engine.Operations.GetMember(m_mainClass, handle);

            } catch (Exception ex) {

                Log.e(GetExceptionInfoString(ex));
                throw ex;

            }

		} 

		public override dynamic Invoke(string handle, params dynamic[] args) {
			
			if (!m_engine.Operations.ContainsMember(m_mainClass, handle)) {

				throw new ScriptException($"Error in script: {m_fileName}. Handle '{handle}' was not declared in {HANDLE_MAIN_CLASS}.");

			}

			dynamic member = m_engine.Operations.GetMember(m_mainClass, handle);

			if (member is IronPython.Runtime.Method) {
			
				// Invoke as method
                try {

                    return m_engine.Operations.InvokeMember(m_mainClass, handle, args);

                } catch (Exception ex) {

                    Log.e(GetExceptionInfoString(ex));
                    throw ex;
                        
                }

			}

			// Treat as property
			return member;

		}

        private string GetExceptionInfoString(Exception ex) {

            return m_engine.GetService<ExceptionOperations>().FormatException(ex).Replace("File \"\"", $"File \"{m_fileName}\"").Replace("File \"<string>\"", $"File \"{m_fileName}\"");
        
        }

    }

}
