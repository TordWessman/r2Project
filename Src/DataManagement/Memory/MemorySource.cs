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
using System.IO;
using R2Core.Network;
using System.Net;
using System.Collections.Generic;
using R2Core.Data;
using MemoryType = System.String;
using System.Linq;

namespace R2Core.DataManagement.Memory
{
	public class MemorySource : DeviceBase, IMemorySource, IDeviceManagerObserver
	{
		private MemoryFactory m_memoryFactory;
		private IDeviceManager m_deviceManager;
		private string m_memoryBusId;
		
		private IMemoryDBAdapter m_dbAdapter;

		private IList<IMemorySource> m_remoteMemorySources;

		public override bool Ready { get {
				return Bus.Ready && m_dbAdapter.Ready;
			}}
		
		public MemorySource (string id,
		                     IDeviceManager deviceManager,
		                     IDatabase db,
		                     string memoryBusId) : base (id)
		{
	
			m_dbAdapter = new MemoryDBAdapter (db);
			m_memoryFactory = new MemoryFactory (m_dbAdapter);
			m_deviceManager = deviceManager;
			m_memoryBusId = memoryBusId;
			m_remoteMemorySources = new List<IMemorySource> ();

			m_deviceManager.AddObserver (this);
			//m_server = server;
			//server.AddObserver (DataPackageType.MemoryRequest, this);
		}


		#region IMemorySource implementation
		public bool Delete (int memoryId)
		{
			IMemory mem = Get (memoryId);
			
			return Delete (ref mem);

		}

		public bool Delete (IMemory memory)
		{

			return Delete (ref memory);

		}
		
		public bool Delete (ref IMemory memory)
		{
			AssertBus ();
			
			if (m_dbAdapter.Delete (memory.Id)) {
				
				Bus.RemoveMemory (memory.Reference);
				
				memory = null;

				return true;

			} 

			Log.w ("Unable to delete: " + memory.ToString () + ". Can only delete localy stored memories.");

			return false;

		}
		
		public IMemory Get (int memoryId)
		{

			IMemoryReference reference = Bus.GetReference (memoryId);
			
			if (reference.Id == MemoryReference.NULL_REFERENCE_ID) {
			
				return null;
			
			}
			
			return m_memoryFactory.CreateMemory (reference, this, true);

		}

		public IMemory Get (string MemoryType) {

			throw new NotImplementedException();

		}

		
		public void Associate (IMemory one, IMemory two)
		{
			Bus.AddAssociation (one.Reference, two.Reference);
		}
		
		
		public override void Start ()
		{
			
			//if (!Bus.Ready) {
			//	Log.w ("Memory Bus is not ready. Will not send memories.");
			//}

			m_dbAdapter.SetUp ();
			
			ICollection<IMemoryReference> refs = m_dbAdapter.LoadAll ();
			Bus.AddMemories (refs);
		}
		
		public IMemoryReference Fetch (int memoryId)
		{
			
			return Bus.GetReference (memoryId);
			//return LoadMemory (memoryId);
			
		}
		
		private void AssertBus ()
		{
			if (!m_deviceManager.Has (m_memoryBusId)) {

				throw new InvalidOperationException ("Unable to fetch memory reference. Bus named: " +
				
					m_memoryBusId + " not identified. You must add a MemoryBus to the network."
				
				);
			
			}
		
		}

		public IMemoryBus Bus {
		
			get {
			
				AssertBus ();
				return m_deviceManager.Get<IMemoryBus> (m_memoryBusId);
			
			}

		}
		
		
		public IMemory Create (MemoryType type, string name)
		{
			int newId = Bus.NextMemoryReference;
			
			IMemoryReference reference = m_memoryFactory.StoreMemoryReference (
				newId, name, type);
			
			Bus.AddMemories (new List<IMemoryReference> () {reference});
			
			return m_memoryFactory.CreateMemory (reference, this, true);
		}
		
		public ICollection<IMemory> GetAssociations (IMemory memory)
		{
			ICollection<IMemory> memories = new List<IMemory>();
			foreach (IMemoryReference association in Bus.GetAssociations (memory.Reference.Id)) 
			{
				memories.Add (m_memoryFactory.CreateMemory (association, this, true));
			}
				              
			return memories;
			
		}
		
		public IMemory Get (MemoryType type, string name)
		{
			foreach (IMemoryReference reference in Bus.GetReferences(type)) {

				if (reference.Value == name) {
				
					return m_memoryFactory.CreateMemory (reference, this, true);
				
				}
			
			}
			
			return null;

		}
		
		public ICollection<IMemory> All (MemoryType type)
		{
			ICollection<IMemory> memories = new List<IMemory> ();

			foreach (IMemoryReference association in Bus.GetReferences(type)) {

				memories.Add (m_memoryFactory.CreateMemory (association, this, true));

			}
				              
			return memories;

		}
		
		public bool Update (IMemory memory) {

			Log.e ("Update not implemented. " + memory.Type + " " + memory.Value + " will not be stored.");

			return false;
		
		}
		#endregion

		#region IDeviceManagerObserver implementation

		public void DeviceAdded (IDevice device)
		{

			if (device is IMemorySource) {
		
				m_remoteMemorySources.Add (device as IMemorySource);

			}
		
		}

		public void DeviceRemoved (IDevice device)
		{

			if (device is IMemorySource) {

				if (m_remoteMemorySources.Contains (device as IMemorySource)) {
				
					m_remoteMemorySources.Remove (device as IMemorySource);

				}

			}

		}

		#endregion

		/*
		private IMemoryReference LoadMemory (int memoryId)
		{
			//TODO: check cache
			// load memory
			string name = null, type = null;
	
			//return m_memoryFactory.Load (memoryId, name,type, this);
			
		}
		*/

		public ICollection<IMemory> Get (int[] memoryIds)
		{
			throw new NotImplementedException ();
		}
	}
}

