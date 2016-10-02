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
using System.Collections.Generic;
using System.IO;


namespace Core.Scripting
{
	/// <summary>
	/// Remotly accessible implementation of a IScriptExecutorFactory. Use the Create on a remote ScriptExecutorFactory in order to create a (remotly accessible) script on a remote machine.
	/// </summary>
	public class ScriptExecutorFactory : RemotlyAccessableDeviceBase, IScriptExecutorFactory
	{
		private IDeviceManager m_deviceManager;
		private ITaskMonitor m_taskMonitor;
		private IScriptFactory m_scriptFactory;
		
		public ScriptExecutorFactory (string id,
		                      IDeviceManager deviceManager,
		                      ITaskMonitor taskMonitor, 
		                      IScriptFactory scriptFactory) : base (id)
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
				
			IScriptExecutor executor = new ScriptExecutor (id, 
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

		#region implemented abstract members of Core.Device.RemotlyAccessableDeviceBase
		public override byte[] RemoteRequest (string methodName, byte[] rawData, IRPCManager<System.Net.IPEndPoint> mgr)
		{
			if (IsBaseMethod (methodName)) {

				return ExecuteStandardDeviceMethod (methodName, rawData, mgr);
			
			} else if (methodName.Equals (RemoteScriptExecutorFactory.CREATE_METHOD_ID)) {
				
				string[] ids = mgr.ParsePackage<string[]> (rawData);
				Create (ids[0], ids[1]);
				
				return null;
			
			}
			
			throw new NotImplementedException ("Method name: " + methodName + " is not implemented...");
		
		}

		public override RemoteDevices GetTypeId ()
		{

			return RemoteDevices.ScriptExecutorFactory;
		
		}

		#endregion
	
	}

}

