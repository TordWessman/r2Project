﻿// This file is part of r2Poject.
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
using R2Core.Device;
using System.IO;
using System.Collections.Generic;
using R2Core.Network;

namespace R2Core.Scripting
{
	
	public abstract class ScriptFactoryBase<T> : DeviceBase, IScriptFactory<T> where T : IScript {

		protected IList<string> ScriptSourcePaths { get; private set; }

        protected ScriptFactoryBase(string id) : base(id) {

            ScriptSourcePaths = new List<string>();

		}


		public void AddSourcePath(string path) {

            ScriptSourcePaths.Add(path);

		}

		public string GetScriptFilePath(string id) {

			string fileName = id + FileExtension;

			if (!File.Exists(fileName)) {

				foreach (string path in ScriptSourcePaths) {

					string evaluatedPath = path.EndsWith(Path.DirectorySeparatorChar.ToString(), System.StringComparison.Ordinal) ? path + fileName : path + Path.DirectorySeparatorChar + fileName;

					if (File.Exists(evaluatedPath)) {

						return evaluatedPath;

					}

				}

				throw new FileNotFoundException($"Unable to locate '{fileName}'. Is there a search path missing?");

			}

			return fileName;

		}

		public abstract T CreateScript(string name, string id = null);

		public abstract IScriptInterpreter CreateInterpreter(T script);

		public IWebEndpoint CreateEndpoint(T script, string path) {

			return new ScriptEndpoint(script, path);
		
		}

		/// <summary>
		/// Must be overridden. Should return the common extension used by the scripts(i.e: ".lua").
		/// </summary>
		/// <value>The file extension.</value>
		protected abstract string FileExtension { get; }

		/// <summary>
		/// Overridden implementation will be called upon new source path.
		/// </summary>
		/// <param name="path">Path.</param>
		protected virtual void DoAddSearchPath(string path) {}

	}

}

