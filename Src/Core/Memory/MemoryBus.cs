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
using System.Net;
using System.Collections.Generic;
using Core.Network.Data;
using Core.Network;
using MemoryType = System.String;
using Core.Data;
using Core.Device;

namespace Core.Memory
{
	public class MemoryBus : RemotlyAccessibleDeviceBase, IMemoryBus
	{
		public const string F_AddMemories = "AddMemories";
		public const string F_AddAssociation = "AddAssociation";
		public const string F_GetAssociations = "GetAssociations";
		public const string F_GetReference = "GetReference";
		public const string F_GetReferences = "GetReferences";
		public const string F_NextMemoryReference = "NextMemoryReference";
		public const string F_RemoveMemory = "RemoveMemory";
		public const string F_GetReferenceByName = "GetReferenceByName";

		public const string F_GetShort = "GetShort";
		public const string F_SetShort = "SetShort";
		public const string F_HasShort = "HasShort";
		
		private IDictionary<string,object> m_shortTermMemories;
		
		private IDictionary<MemoryType, ICollection<IMemoryReference>> m_memories;
		
		private IDictionary<int, WeakReference> m_idPointers;

		private readonly object m_associationsLock = new object();
		private readonly object m_shortTermMemoryLock = new object();
		
		private IHostManager<IPEndPoint> m_hostManager; 
		private IAssociationsDBAdapter m_dbAdapter;
		private INetworkPackageFactory m_networkPackageFactory;
		
		public int NextMemoryReference {
			get {
				
				//TODO: fix this
				Random random = new Random ();
				int val = random.Next (0, Int32.MaxValue);
				while (m_idPointers.ContainsKey(val)) {
					val = random.Next (0, Int32.MaxValue);
					Log.t ("Random!");
				}
				
				return val;
			}}
		
		
		public MemoryBus (string id, IDatabase db, IHostManager<IPEndPoint> hostManager, INetworkPackageFactory networkPackageFactory) : base (id)
		{
			m_memories = new Dictionary<MemoryType, ICollection<IMemoryReference>> ();
			m_idPointers = new Dictionary<int, WeakReference> ();
			m_dbAdapter = new AssociationsDBAdapter (db);
			m_hostManager = hostManager;
			m_shortTermMemories = new Dictionary<string, object> ();
			m_networkPackageFactory = networkPackageFactory;
		}
		
		public void AddMemories (ICollection<IMemoryReference> memories)
		{
			foreach (IMemoryReference memory in memories) {
				AddMemory (memory);
			}
		}

		
		
		private void AddMemory (IMemoryReference memory)
		{
			
			if (!m_memories.ContainsKey (memory.Type)) {
				m_memories.Add (memory.Type, new List<IMemoryReference> ());
			}
			
			if (!m_memories [memory.Type].Contains (memory)) {
				m_memories [memory.Type].Add (memory);
			}
			
			if (!m_idPointers.ContainsKey (memory.Id)) {
				m_idPointers.Add (memory.Id, new WeakReference (memory));
			}

		}
		
		private void RemoveMemoryInternal (IMemoryReference memory)
		{
				
			if (m_memories.ContainsKey (memory.Type)) {
				if (m_memories [memory.Type].Contains (memory)) {
					m_memories [memory.Type].Remove (memory);
				}
			}

			if (m_idPointers.ContainsKey (memory.Id)) {
				m_idPointers.Remove(memory.Id);
			}
		}
		
		public void RemoveMemory (IMemoryReference memory)
		{
			lock (m_associationsLock) {
				m_dbAdapter.RemoveAssociations (memory.Id);
			}
			
			RemoveMemoryInternal (memory);
		}
		
		public void AddAssociation (IMemoryReference first, IMemoryReference second)
		{
			
			lock (m_associationsLock) {
				m_dbAdapter.SetAssociation (first.Id, second.Id);
			}

		}
		
		public ICollection<IMemoryReference> GetAssociations (int memoryId)
		{
			lock (m_associationsLock) {
				
				ICollection<IMemoryReference> result = new List<IMemoryReference> ();
				
				int [] associations = m_dbAdapter.GetAssociations (memoryId);
			
				foreach (int association in associations) {
					if (m_idPointers.ContainsKey (association)) {
						IMemoryReference reference = m_idPointers [association].Target as IMemoryReference;
						result.Add (reference);

					} //If no association is found, it should be the consequence of a host (containing memories) that is not connected
//					else {
//						throw new ApplicationException ("Host missing? The association id " + association
//						                                + " was not found in the list of references"
//						                                + " for memory " + memoryId);
//					}
					
					//Log.t ("HAHA: " + reference.Name);
					
				
				}
			
				return result;
			}
		
		}
		
		public IMemoryReference GetReference (int memoryId)
		{
			if (m_idPointers.ContainsKey (memoryId)) {
				return m_idPointers [memoryId].Target as IMemoryReference;
			} else {
				throw new IndexOutOfRangeException ("No IMemoryReference found with id :" + memoryId);
			}
		}
		
		public ICollection<IMemoryReference> GetReferences (MemoryType type)
		{
			if (!m_memories.ContainsKey (type)) {
				Log.w ("No memory of type: " + type + " found.");
				return new List<IMemoryReference> ();
			}
			
			
			
			return m_memories[type];
		}
		
		public IMemoryReference GetReferenceByName (MemoryType type, string name)
		{
			if (!m_memories.ContainsKey (type)) {
				throw new ApplicationException ("No memory of type: " + type);
			}
			
			
			foreach (IMemoryReference reference in m_memories[type]) {
				if (reference.Value.ToLower ().Equals (name.ToLower ())) {
					return reference;
				}
			}
			
			throw new ApplicationException ("No memory with name: " + name + " found.");
		}
		
		#region IDeviceBase implementation
		public override void Start ()
		{
			m_dbAdapter.SetUp ();
			
			m_hostManager.SendToAll (
				m_networkPackageFactory.CreateMemoryBusOnlinePackage ());
			
		}

		public override bool Ready {
			get {
				return m_dbAdapter.Ready;
			}
		}
		#endregion

		#region IRemotlyAccessable implementation
		public override byte[] RemoteRequest (string methodName, byte[] rawData, Core.Device.IRPCManager<IPEndPoint> mgr)
		{
			if (IsBaseMethod (methodName)) {
				return ExecuteStandardDeviceMethod (methodName, rawData, mgr);
			}
			
			if (methodName == F_AddMemories) {
				ICollection<IMemoryReference> input = mgr.ParsePackage<ICollection<IMemoryReference>> (rawData);
				AddMemories (input);
				
				return null;

			} else if (methodName == F_AddAssociation) {
				IMemoryReference[] input = mgr.ParsePackage< IMemoryReference[] > (rawData);
				AddAssociation (input [0], input [1]);
				
				return null;
				
			} else if (methodName == F_GetAssociations) {
				int id = mgr.ParsePackage<int> (rawData);
			
				return mgr.RPCReply<ICollection<IMemoryReference>> (Guid, methodName, GetAssociations (id));
				
			} else if (methodName == F_GetReference) {
				int id = mgr.ParsePackage<int> (rawData);
			
				return mgr.RPCReply<IMemoryReference> (Guid, methodName, GetReference (id));
				
			} else if (methodName == F_GetReferences) {
				MemoryType type = mgr.ParsePackage<MemoryType> (rawData);
			
				return mgr.RPCReply<ICollection<IMemoryReference>> (Guid, methodName, GetReferences (type));
				
			} else if (methodName == F_NextMemoryReference) {
			
				return mgr.RPCReply<int> (Guid, methodName, NextMemoryReference);
			} else if (methodName == F_RemoveMemory) {
				IMemoryReference reference = mgr.ParsePackage<IMemoryReference> (rawData);
				RemoveMemory (reference);
				return null;
			} else if (methodName == F_SetShort) {
				KeyValuePair<string,object> input = mgr.ParsePackage<KeyValuePair<string,object>> (rawData);
				SetShort (input.Key, input.Value);
				return null;
			} else if (methodName == F_GetShort) {
				string key = mgr.ParsePackage<string> (rawData);
				
				return mgr.RPCReply<object> (Guid, methodName, GetShort (key));
				
			} else if (methodName == F_HasShort) {
				string key = mgr.ParsePackage<string> (rawData);
				
				return mgr.RPCReply<bool> (Guid, methodName, HasShort (key));
			}
		
			throw new System.NotImplementedException ("The method you try to evoke is not implemented: " + methodName);
		}

		public override Core.Device.RemoteDevices GetTypeId ()
		{
			return Core.Device.RemoteDevices.MemoryBus;

		}
		#endregion

		public void SetShort (string key, object value)
		{
			lock (m_shortTermMemoryLock) {
				if (m_shortTermMemories.ContainsKey (key)) {
					m_shortTermMemories [key] = value;
				} else {
					m_shortTermMemories.Add (key, value);
				}				
			}

		}
		
		public bool HasShort (string key)
		{
			lock (m_shortTermMemoryLock) {
				return m_shortTermMemories.ContainsKey (key);
			}
		}
		
		public T GetShort<T> (string key)
		{
			lock (m_shortTermMemoryLock) {
				if (!m_shortTermMemories.ContainsKey (key)) {
					throw new ArgumentException ("The key: " + key + " did not exist in my short term memory.");
				} else {
					return (T)m_shortTermMemories [key];
				}
			}

		}
		
		public object GetShort (string key)
		{
			
			return GetShort<object> (key);
		}

	}
}

