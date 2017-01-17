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
using System.Collections.Generic;
using System.IO;
using Core.Device;
using System.Threading.Tasks;


namespace Core.Scripting
{
	/// <summary>
	/// Implementation of a script factory. Used to create ruby scripts.
	/// </summary>
	public class RubyScriptFactory : DeviceBase, IScriptFactory<RubyScript>
	{
		private string m_scriptSourcePath;
		private IDeviceManager m_deviceManager;
		private ICollection<string> m_paths;
		private const string RUBY_FILE_EXTENSION = ".rb";
		private const string RUBY_COMMAND_SCRIPT_ID_POSTFIX = "_in_command_script";
		private ITaskMonitor m_taskMonitor;
		
		public RubyScriptFactory (string id, string scriptSourcePath, 
		                      ICollection<string> paths,
		                      IDeviceManager deviceManager,
		                       ITaskMonitor taskMonitor) : base (id)
		{

			if (scriptSourcePath.EndsWith (Path.DirectorySeparatorChar.ToString ())) {

				scriptSourcePath = scriptSourcePath.Substring (0, scriptSourcePath.Length - 1);
			
			}

			m_taskMonitor = taskMonitor;
			m_paths = paths;
			m_deviceManager = deviceManager;
			m_scriptSourcePath = scriptSourcePath;
		
		}
		
		public ICommandScript CreateCommand (string id, string sourceFile = null)
		{
			
			IScript script = CreateScript (id + RUBY_COMMAND_SCRIPT_ID_POSTFIX, sourceFile);
			
			return new RubyCommandScript (id, script);
		}

		public IScriptProcess CreateProcess (string id, string sourceFile = null) {
		
			IScript script = CreateScript (id, sourceFile);

			IScriptProcess process = new RubyProc (id, script);

			foreach (Task task in process.GetTasksToObserve().Values) {

				m_taskMonitor.AddTask(script.Identifier, task);

			}

			return process;

		}

		public IScriptProcess CreateProcess (string id, RubyScript script) {

			IScriptProcess process = new RubyProc (id, script);

			foreach (Task task in process.GetTasksToObserve().Values) {

				m_taskMonitor.AddTask(script.Identifier, task);

			}

			return process;

		}
			
		public RubyScript CreateScript (string id, string sourceFile = null) {
		
			return new RubyScript (id,
				GetSourceFilePath(id,sourceFile),
				m_paths,
				m_deviceManager);
		}
		
		public string GetSourceFilePath (string id, string sourceFile = null)
		{

			if (sourceFile == null) {

				sourceFile = id + RUBY_FILE_EXTENSION;

			}

			//Check if the file is in the script base folder. If not, use the provided path as an absolute path.
			string sourceFilePath = File.Exists (m_scriptSourcePath + Path.DirectorySeparatorChar + sourceFile) ? 
				m_scriptSourcePath + Path.DirectorySeparatorChar + sourceFile : sourceFile;

			if (!File.Exists (sourceFilePath)) {

				throw new ArgumentException ("Ruby file with path '" + sourceFilePath + "' does not exist.");

			}

			return sourceFilePath;
		
		}

		public IScriptInterpreter CreateInterpreter(RubyScript script) {

			return new RubySrlptInterpreter (script);

		}

	}

}

