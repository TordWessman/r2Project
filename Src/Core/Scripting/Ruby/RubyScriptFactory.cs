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
using Microsoft.Scripting.Hosting;
using IronRuby;


namespace Core.Scripting
{
	/// <summary>
	/// Implementation of a script factory. Used to create ruby scripts.
	/// </summary>
	public class RubyScriptFactory : ScriptFactoryBase<RubyScript>
	{

		private const string RUBY_FILE_EXTENSION = ".rb";
		private const string RUBY_COMMAND_SCRIPT_ID_POSTFIX = "_in_command_script";

		//Make sure the IronRuby.Library.dll are included during compilation.
		#pragma warning disable 0169
		private static readonly IronRuby.StandardLibrary.BigDecimal.BigDecimal INCLUDE_IRONRUBY_LIBRARY_ON_COMPILE_TIME;
		#pragma warning restore 0169

		private IDeviceManager m_deviceManager;
		private ScriptEngine m_engine;

		/// <summary>
		/// Initializes a new instance of the <see cref="Core.Scripting.RubyScriptFactory"/> class.
		/// scriptSourcePaths contains search paths for the directories conaining script. paths is an array of ruby search paths used by the script and the engine.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="paths">Paths.</param>
		/// <param name="deviceManager">Device manager.</param>
		/// <param name="taskMonitor">Task monitor.</param>
		public RubyScriptFactory (string id, 
		                      	ICollection<string> paths,
		                      	IDeviceManager deviceManager) : base (id)
		{

			m_deviceManager = deviceManager;
			m_engine = Ruby.CreateEngine ();
			m_engine.SetSearchPaths (paths);
		
		}

		protected override string FileExtension { get { return RUBY_FILE_EXTENSION; } }

		public override ICommandScript CreateCommand (string id)
		{
			
			IScript script = CreateScript (id);
			
			return new RubyCommandScript (id, script);

		}
			
		public override RubyScript CreateScript (string id) {
		
			IDictionary<string, dynamic> inputParams = new Dictionary<string, dynamic> ();

			// Scripts must know about the device manager. It's how they get access to the rest of the system..
			inputParams.Add(m_deviceManager.Identifier, m_deviceManager);

			RubyScript script = new RubyScript (id,
				GetScriptFilePath(id),
				m_engine, inputParams);
			
			return script;

		}
	
		public override IScriptInterpreter CreateInterpreter(RubyScript script) {

			return new RubySrlptInterpreter (script);

		}

	}

}

