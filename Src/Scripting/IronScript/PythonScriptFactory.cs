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
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Collections.Generic;
using R2Core.Device;
using System.IO;

namespace R2Core.Scripting
{

    public class PythonScriptFactory : ScriptFactoryBase<IronScript> {

        /// Make sure the IronPython StdLib is included in the compiled binary.
        private class MakeSureIronPythonModulesAreIncluded : IronPython.Modules.EncodingMap { }

        private ScriptEngine m_engine;
        private IDeviceManager m_deviceManager;

        /// <summary>
        /// If set to `true`, the "standard import headers defined by `ScriptR2ImportFile` will be prepended
        /// to all scripts. Set this flag to `false` if not all project are included in the binary.
        /// </summary>
        public bool PrependR2Imports = false;

        public PythonScriptFactory(string id,    
            ICollection<string> paths,
            IDeviceManager deviceManager) : base(id) {

            m_deviceManager = deviceManager;

            foreach (string path in paths) { AddSourcePath(path); }

            Reload();
        
        }

        public void Reload() {

            m_engine = Python.CreateEngine();
            ClearHooks();
            m_engine.SetSearchPaths(ScriptSourcePaths);

        }

        /// <summary>
        /// Needed in order to bypass the "Not a ZIP file" exception
        /// </summary>
        private void ClearHooks() {

            var pc = Microsoft.Scripting.Hosting.Providers.HostingHelpers.GetLanguageContext(m_engine) as IronPython.Runtime.PythonContext;
            var hooks = pc.SystemState.Get__dict__()["path_hooks"] as IronPython.Runtime.List;
            hooks.Clear();

        }

        public override IronScript CreateScript(string name, string id = null) {
        
            // Add the factorys source paths to the engines search paths.
            m_engine.SetSearchPaths(ScriptSourcePaths);

            // Scripts must know about the device manager. It's how they get access to the rest of the system..
            IronScript script = new IronScript(id ?? name, GetScriptFilePath(name), 
                m_engine, 
                new Dictionary<string, dynamic> { { m_deviceManager.Identifier, m_deviceManager } });

            if (PrependR2Imports) {

                script.PrependedCode.Add(File.ReadAllText(GetScriptFilePath(Settings.Consts.ScriptR2ImportFile())));

            }

            script.Reload();

            return script;

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

