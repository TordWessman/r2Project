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
using System.IO;


namespace R2Core.Scripting
{
	/// <summary>
	/// Remotly accessible implementation of a IScriptExecutorFactory. Use the Create on a remote ScriptExecutorFactory in order to create a (remotly accessible) script on a remote machine.
	/// </summary>
	public class ScriptExecutorFactory<T> : DeviceBase, IScriptExecutorFactory where T : IScript
	{
		private IDeviceManager m_deviceManager;
		private ITaskMonitor m_taskMonitor;
		private IScriptFactory<T> m_scriptFactory;
		
		public ScriptExecutorFactory (string id,
		                      IDeviceManager deviceManager,
		                      ITaskMonitor taskMonitor, 
			IScriptFactory<T> scriptFactory) : base (id)
		{
		
			m_deviceManager = deviceManager;
			m_taskMonitor = taskMonitor;
			m_scriptFactory = scriptFactory;
		
		}
		


		#region INetScriptFactory implementation
		public void Create (string id, string scriptId = null)
		{

			if (scriptId == null) {
			
				scriptId = id;
			
			}
				
			IScriptExecutor executor = new ScriptExecutor<T> (id, 
			                                               scriptId,
			                                               m_deviceManager,
			                                               m_taskMonitor,
			                                               m_scriptFactory);
			if (m_deviceManager.Has (id)) {

				m_deviceManager.Remove (id);
			
			}
			
			m_deviceManager.Add (executor);
			
		}
		#endregion

	}

}

