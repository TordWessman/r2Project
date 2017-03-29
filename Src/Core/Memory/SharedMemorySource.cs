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

﻿using System;
using Core.Device;
using System.Collections.Generic;
using Core.Data;
using MemoryType = System.String;
using System.Linq;

namespace Core.Memory
{
	/// <summary>
	/// Shared memory source is a possibly inter connected memory source (can be chained and combined with other sources). Deletions, updates etc will affect linked sources.
	/// </summary>
	public class SharedMemorySource : RemotlyAccessableDeviceBase, IMemorySource, IDeviceManagerObserver
	{

		public const string F_GetAssociations = "GetAssociations";
		public const string F_GetId = "GetId";
		public const string F_GetIds = "GetIds";
		public const string F_GetType = "GetType";

		public const string F_Create = "Create";
		public const string F_AddAssociation = "AddAssociation";
		public const string F_DeleteMemory = "DeleteMemory";
		public const string F_UpdateMemory = "UpdateMemory";

		private MemoryFactory m_memoryFactory;
		private IDeviceManager m_deviceManager;

		private IMemoryDBAdapter m_memDb;
		private IAssociationsDBAdapter m_assDb;
		private IList<IMemorySource> m_otherSources;

		public SharedMemorySource (string id, IDeviceManager deviceManager, IDatabase db) : base (id)
		{

			m_memDb = new MemoryDBAdapter(db);
			m_assDb = new AssociationsDBAdapter(db);
			m_deviceManager = deviceManager;
			m_memoryFactory = new MemoryFactory (m_memDb);

			m_deviceManager.AddObserver (this);
			m_otherSources = new List<IMemorySource> ();

		}

		public void PrintAll () {
		
			foreach (IMemory memory in All()) {
			
				Log.t (memory.Id + ": " + memory.Type + " = " + memory.Value + " " + (memory.IsLocal ? "" : "[remote]"));

			}

		}

		public override void Start ()
		{

			m_memDb.SetUp ();
			m_assDb.SetUp ();

		}

		public IMemory Get (int memoryId) {

			return _Get (new int[]{ memoryId }, true).FirstOrDefault();

		}

		public IMemory Get (MemoryType type) {

			return _Get (type, true);

		}

		public ICollection<IMemory> All (MemoryType type = null) {
		 
			if (type == null) {
			
				type = string.Empty;

			}

			return _All (type, true);

		}

		public ICollection<IMemory> GetAssociations (IMemory memory) {

			return _GetAssociations (memory, true);

		}

		public ICollection<IMemory> Get (int[] memoryIds) {

			return _Get (memoryIds, true);

		}

		public void Associate (IMemory one, IMemory two) {

			_Associate (one, two, true);

		}

		public IMemory Create (string type, string name)
		{

			IMemoryReference reference = m_memoryFactory.StoreMemoryReference(NextMemoryReference, name, type);
			IMemory memory = m_memoryFactory.CreateMemory (reference, this, true);

			return memory;

		}


		public bool Update (IMemory memory)
		{

			return _Update(memory.Reference, true);

		}

		public bool Delete (IMemory memory) {

			return _Delete (memory.Id, true);

		}

		public bool Delete (int memoryId) {
		
			return _Delete (memoryId, true);

		}

		#region private

		private bool _Update(IMemoryReference reference, bool affectOther) {
		
			if (m_memDb.Update (reference)) {
			
				return true;

			}

			foreach (IMemorySource source in m_otherSources) {
			
				if (source.Update (m_memoryFactory.CreateMemory(reference, null, false))) {
				
					return true;

				}

			}

			return false;

		}

		private bool _Delete (int memoryId, bool affectOther)
		{

			m_assDb.RemoveAssociations (memoryId);

			if (!m_memDb.Delete (memoryId) && affectOther) {

				foreach (IMemorySource source in m_otherSources) {

					if (source.Delete (memoryId)) {

						return true;

					}

				}

			} else {

				return true;
			}

			return false;

		}


		private ICollection<IMemory> _Get (int[] memoryIds, bool affectOther = false)
		{

			List<IMemory> memories = new List<IMemory> (m_memDb.Get (memoryIds).Select (r => m_memoryFactory.CreateMemory (r, this, true)));

			if (affectOther) {
			
				foreach (IMemorySource source in m_otherSources) {

					memories.AddRange(source.Get(memoryIds).Select(m => m_memoryFactory.CreateMemory(m.Reference, source, false)));

				}

			}

			return memories;
		
		}

		private IMemory _Get (MemoryType type, bool affectOther) {

			return _All (type, affectOther).FirstOrDefault ();

		}

		private ICollection<IMemory> _GetAssociations (IMemory memory, bool affectOther)
		{

			List<IMemory> memories = new List<IMemory> ();
			memories.AddRange(m_memDb.Get (m_assDb.GetAssociations (memory.Id)).Select (r => m_memoryFactory.CreateMemory (r, this, true)));

			if (affectOther) {

				foreach (IMemorySource source in m_otherSources) {

					memories.AddRange (source.GetAssociations (memory).Select (m => m_memoryFactory.CreateMemory (m.Reference, source, false)));

				}

			}

			return memories;

		}

		private void _Associate (IMemory one, IMemory two, bool affectOther)
		{

			m_assDb.SetAssociation (one.Id, two.Id);

			if (affectOther) {

				foreach (IMemorySource source in m_otherSources) {

					source.Associate (one, two);

				}

			}

		}

		private ICollection<IMemory> _All (MemoryType type, bool affectOther = false)
		{

			IEnumerable<IMemoryReference> references = type == string.Empty ? m_memDb.LoadAll() : m_memDb.Get (type);
			List<IMemory> memories = new List<IMemory> ();

			var refs = references.Select (r => m_memoryFactory.CreateMemory (r, this, true));

			if (refs != null) {
			
				memories.AddRange(refs);

			}

			if (affectOther) {
			
				foreach (IMemorySource source in m_otherSources) {

					if (source != null) {

						references = source.All (type).Select(r => r.Reference);

						if (references != null) {
						
							memories.AddRange(references.Select(r => m_memoryFactory.CreateMemory (r, source, false)));

						}

					}

				}

			}

			return memories;

		}

		private int NextMemoryReference {

			get {

				//TODO: fix this
				Random random = new Random ();
				return random.Next (0, Int32.MaxValue);

			}

		}

		#endregion

		#region RemotlyAccessableDeviceBase

		public override byte[] RemoteRequest (string methodName, byte[] rawData, IRPCManager<System.Net.IPEndPoint> mgr)
		{
			if (IsBaseMethod (methodName)) {

				return ExecuteStandardDeviceMethod (methodName, rawData, mgr);
			
			}

			if (methodName == F_Create) {

				// Not implemented on remode
				IMemoryReference reference = mgr.ParsePackage<IMemoryReference> (rawData);
				IMemory newMemory = Create (reference.Type, reference.Value);

				return mgr.RPCReply<IMemoryReference> (Guid, methodName, newMemory.Reference);;

			} else if (methodName == F_UpdateMemory) {

				// Not implemented on remode
				IMemoryReference reference = mgr.ParsePackage<IMemoryReference> (rawData);
				bool didUpdate = _Update (reference, false);

				return mgr.RPCReply<bool> (Guid, methodName, didUpdate);;

			} else if (methodName == F_AddAssociation) {

				IMemoryReference[] input = mgr.ParsePackage<IMemoryReference[] > (rawData);

				_Associate (
					m_memoryFactory.CreateMemory(input [0], this, false), 
					m_memoryFactory.CreateMemory(input [1], this, false),
					false);

				return null;

			} else if (methodName == F_GetAssociations) {

				int id = mgr.ParsePackage<int> (rawData);
				IMemory memory = _Get (new int[]{id}, false).FirstOrDefault();
				List<IMemoryReference> associations = new List<IMemoryReference> ();

				if (memory != null) {
				
					associations.AddRange (_GetAssociations (memory, false));

				}

				return mgr.RPCReply<ICollection<IMemoryReference>> (Guid, methodName, associations);

			} else if (methodName == F_GetId) {

				int id = mgr.ParsePackage<int> (rawData);
				IMemory memory = _Get (new int[]{id}, false).FirstOrDefault();
				IMemoryReference reference = memory != null ? memory.Reference : m_memoryFactory.CreateNullReference ();

				return mgr.RPCReply<IMemoryReference> (Guid, methodName, reference);;

			} else if (methodName == F_GetIds) {

				int[] ids = mgr.ParsePackage<int[]> (rawData);
				ICollection<IMemory>  memories = _Get (ids, false);

				return mgr.RPCReply<IMemoryReference[]> (Guid, methodName, memories.Select( m => m.Reference).ToArray());;

			} else if (methodName == F_GetType) {

				MemoryType type = mgr.ParsePackage<MemoryType> (rawData);
				ICollection<IMemory>  memories = _All (type, false);

				return mgr.RPCReply<IMemoryReference[]> (Guid, methodName, memories.Select( m => m.Reference).ToArray());;

			}

			throw new System.NotImplementedException ("The method you try to evoke is not implemented: " + methodName);

		}

		public override RemoteDevices GetTypeId ()
		{
			return RemoteDevices.MemorySource;
		}

		#endregion

		#region IDeviceManagerObserver implementation

		public void DeviceAdded (IDevice device)
		{

			if (device is IMemorySource && device.Identifier != Identifier) {

				m_otherSources.Add (device as IMemorySource);

			}

		}

		public void DeviceRemoved (IDevice device)
		{

			if (device is IMemorySource) {

				if (m_otherSources.Contains (device as IMemorySource)) {

					m_otherSources.Remove (device as IMemorySource);

				}

			}

		}

		#endregion

		public void AddSource (IMemorySource source) {
		
			if (source == null) {
			
				throw new ArgumentException ("Source can't be null.");

			}

			if (source.Guid != Guid) {

				m_otherSources.Add (source);

			}

		}

	}

}

