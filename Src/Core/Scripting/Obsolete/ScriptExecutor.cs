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
using System.Linq;

namespace Core.Scripting
{
	/// <summary>
	/// Implementation of an IScriptExecutor. Remotely accissble (which allows communication with scripts created on other instances).
	/// </summary>
	public class ScriptExecutor<T> : RemotlyAccessibleDeviceBase, IScriptExecutor, IScriptObserver where T: IScript
	{
		
		private IDictionary<string, IScript> m_scripts;
		private IDeviceManager m_deviceManager;
		private ITaskMonitor m_taskMonitor;
		private IScriptFactory<T> m_scriptFactory;
		private Random m_random;
		private string m_scriptId;
		private string m_currentScriptId;
		
		public ScriptExecutor (	string id, 
		                       	string scriptId,
		                      	IDeviceManager deviceManager,
		                      	ITaskMonitor taskMonitor, 
								IScriptFactory<T> scriptFactory) : base(id)
		{

			m_deviceManager = deviceManager;
			m_taskMonitor = taskMonitor;
			m_scriptFactory = scriptFactory;
			m_scriptId = scriptId;
			
			m_random = new Random ();
			m_scripts = new Dictionary<string, IScript> ();
			
			CreateScript ();
						
		}
		
		private void CreateScript ()
		{

			throw new NotImplementedException ("Fix this if/when it's time to figure out what the script-executor thing is good for.");
			/*
			IScriptProcess script = m_scriptFactory.CreateProcess (m_scriptId + "_" + m_random.Next (int.MaxValue),
				m_scriptFactory.GetScriptFilePath (m_scriptId));

			m_scripts.Add (script.Identifier, script);
			m_currentScriptId = script.Identifier;
			*/	
		}
			                                          
			                                          
		
		public override void Start ()
		{
			
			IScript script = m_scripts [m_currentScriptId];
			
			m_deviceManager.Add (script);
			
			script.AddObserver (this);
			
			m_taskMonitor.AddMonitorable (script);
			script.Start ();
			
			CreateScript ();

		}
		
		private void RemoveScript (string scriptId)
		{
			Log.t ("removing script: " + scriptId);

			if (m_scripts [scriptId].IsRunning) {

				m_scripts [scriptId].Stop ();
			
			}
			
			m_deviceManager.Remove (scriptId);

		}
		
		public override void Stop ()
		{
			foreach (IScript process in m_scripts.Values) {

				if (process.IsRunning) {
				
					process.Stop ();
				
				}
			
			}
			
			//m_scripts = new Dictionary<string, IScript> ();
		
		}
		
		#region implemented abstract members of Core.Device.RemotlyAccessibleDeviceBase

		public override byte[] RemoteRequest (string methodName, byte[] rawData, IRPCManager<System.Net.IPEndPoint> mgr)
		{
			if (IsBaseMethod (methodName)) {
				
				return ExecuteStandardDeviceMethod (methodName, rawData, mgr);
				
			} else if (methodName.Equals (RemoteScriptExecutor.GET_VALUE_METHOD_NAME)) {
				
				string handle = mgr.ParsePackage<string> (rawData);
				object returnObject = Get (handle);
				
				if (returnObject == null) {

					Log.e ("Unable to get object for handle: " + handle + 
					       " in ScriptExecutor: " + Identifier); 

					returnObject = new object ();
				
				}

				return mgr.RPCReply<object> (Guid, methodName, returnObject);
				
			} else if (methodName.Equals (RemoteScriptExecutor.SET_VALUE_METHOD_NAME)) {
				
				KeyValuePair<string,object> input = mgr.ParsePackage<KeyValuePair<string,object>> (rawData);
				Set (input.Key, input.Value);
				return null;
				
			} else if (methodName.Equals (RemoteScriptExecutor.DONE_METHOD_NAME)) {
				
				return mgr.RPCReply<bool> (Guid, methodName, Done);
				
			} 
			
			throw new NotImplementedException ("Method name: " + methodName + " is not implemented for ScriptExecutor.");
		
		}

		public override RemoteDevices GetTypeId ()
		{
		
			return RemoteDevices.ScriptExecutor;
		
		}

		#endregion

		#region IScriptObserver implementation

		public void OnScriptFinished (IScript script)
		{
		
			RemoveScript (script.Identifier);
			m_scripts.Remove (script.Identifier);
		
		}

		public void OnScriptErrors (IScript script) {

			OnScriptFinished (script);

		}

		#endregion

		#region IScriptExecutor implementation
	
		public void Set (string handle, dynamic value)
		{

			foreach (IScript process in m_scripts.Values) {

				if (process.Ready) {
				
					process.Set (handle, value);
			
				}
			
			}

		}

		public dynamic Get (string handle)
		{

			foreach (IScript process in m_scripts.Values) {

				if (process.Ready) {
				
					return process.Get (handle);
				
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
