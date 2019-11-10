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
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Collections.Generic;
using R2Core.Device;
using System.Linq;

namespace R2Core.Scripting
{
	public class PythonScriptFactory : ScriptFactoryBase<IronScript> {
		
		//Make sure the IronPython.Library.dll are included during compilation.
		#pragma warning disable 0169
		private static readonly IronPython.DictionaryTypeInfoAttribute INCLUDE_PYTHON_LIBRARY;
		private static readonly IronPython.Modules.SysModule.floatinfo INCLUDE_PYTHON_MODULES_LIBRARY;
		#pragma warning restore 0169

		private ICollection<string> m_paths;
		private ScriptEngine m_engine;
		private IDeviceManager m_deviceManager;

		public PythonScriptFactory(string id,	
			ICollection<string> paths,
			IDeviceManager deviceManager) : base(id) {
			m_deviceManager = deviceManager;
			m_engine = Python.CreateEngine();
		
			m_engine.SetSearchPaths(paths);
			m_paths = paths;

		}

		public override IronScript CreateScript(string name, string id = null) {
		
			IDictionary<string, dynamic> inputParams = new Dictionary<string, dynamic>();

			// Add the factorys source paths to the engines search paths.
			m_engine.SetSearchPaths(m_paths.Concat(ScriptSourcePaths).ToList());

			// Scripts must know about the device manager. It's how they get access to the rest of the system..
			inputParams.Add(m_deviceManager.Identifier, m_deviceManager);

			return new IronScript(id ?? name, GetScriptFilePath(name), m_engine, inputParams);

		}

		public override IScriptInterpreter CreateInterpreter(IronScript script) {

			script.Set(Settings.Identifiers.ObjectInvoker(), new ObjectInvoker());
			return new ScriptInterpreter(script);

		}

		/// <summary>
		/// Must be overridden. Should return the common extension used by the scripts(i.e: ".lua").
		/// </summary>
		/// <value>The file extension.</value>
		protected override string FileExtension { get {return ".py"; } }
	}
}

