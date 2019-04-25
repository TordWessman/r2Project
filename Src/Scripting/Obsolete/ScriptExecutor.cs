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
using R2Core.Device;
using System.Collections.Generic;
using System.Linq;

namespace R2Core.Scripting
{
	/// <summary>
	/// Implementation of an IScriptExecutor. Remotely accissble(which allows communication with scripts created on other instances).
	/// </summary>
	public class ScriptExecutor<T> : DeviceBase, IScriptExecutor, IScriptObserver where T : IScript
	{
		
		private IDictionary<string, IScript> m_scripts;
		private IDeviceManager m_deviceManager;
		private ITaskMonitor m_taskMonitor;
		private IScriptFactory<T>m_scriptFactory;
		private Random m_random;
		private string m_scriptId;
		private string m_currentScriptId;
		
		public ScriptExecutor(string id, 
		                       	string scriptId,
		                      	IDeviceManager deviceManager,
		                      	ITaskMonitor taskMonitor, 
								IScriptFactory<T> scriptFactory) : base(id) {

			m_deviceManager = deviceManager;
			m_taskMonitor = taskMonitor;
			m_scriptFactory = scriptFactory;
			m_scriptId = scriptId;
			
			m_random = new Random();
			m_scripts = new Dictionary<string, IScript>();
			
			CreateScript();
						
		}
		
		private void CreateScript() {

			throw new NotImplementedException("Fix this if/when it's time to figure out what the script-executor thing is good for.");
			/*
			IScriptProcess script = m_scriptFactory.CreateProcess(m_scriptId + "_" + m_random.Next(int.MaxValue),
				m_scriptFactory.GetScriptFilePath(m_scriptId));

			m_scripts.Add(script.Identifier, script);
			m_currentScriptId = script.Identifier;
			*/	
		}
			                                          
			                                          
		
		public override void Start() {
			
			IScript script = m_scripts [m_currentScriptId];
			
			m_deviceManager.Add(script);
			
			script.AddObserver(this);
			
			m_taskMonitor.AddMonitorable(script);
			script.Start();
			
			CreateScript();

		}
		
		private void RemoveScript(string scriptId) {
			Log.t("removing script: " + scriptId);

			if (m_scripts [scriptId].IsRunning) {

				m_scripts [scriptId].Stop();
			
			}
			
			m_deviceManager.Remove(scriptId);

		}
		
		public override void Stop() {
			foreach (IScript process in m_scripts.Values) {

				if (process.IsRunning) {
				
					process.Stop();
				
				}
			
			}
			
			//m_scripts = new Dictionary<string, IScript>();
		
		}

		#region IScriptObserver implementation

		public void OnScriptFinished(IScript script) {
		
			RemoveScript(script.Identifier);
			m_scripts.Remove(script.Identifier);
		
		}

		public void OnScriptErrors(IScript script) {

			OnScriptFinished(script);

		}

		#endregion

		#region IScriptExecutor implementation
	
		public void Set(string handle, dynamic value) {

			foreach (IScript process in m_scripts.Values) {

				if (process.Ready) {
				
					process.Set(handle, value);
			
				}
			
			}

		}

		public dynamic Get(string handle) {

			foreach (IScript process in m_scripts.Values) {

				if (process.Ready) {
				
					return process.Get(handle);
				
				}
			
			}
			
			return null;

		}
		
		public bool Done {
			
			get {
				
				foreach (IScript process in m_scripts.Values) {

					if (process.IsRunning) {
					
						return false;
					
					}
				
				}
				
				return true;
			
			}
		
		}

		#endregion
	
	}

}

