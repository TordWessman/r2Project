using System;
using Core.Device;
using MemoryType = System.String;
using System.Collections.Generic;
using System.Linq;

namespace Core.Memory
{

	public class TemporaryMemoryReference: IMemoryReference {

		private int m_id;

		public TemporaryMemoryReference(int id) {
		
			m_id = id;
		}

		public int Id { get { return m_id; } }

		public MemoryType Type {get; set;}

		public string Value {get; set;}

		public bool IsNull { get { return false; } }
	
	}

	/// <summary>
	/// Does not persist any data.
	/// </summary>
	public class TemporaryMemorySource: DeviceBase, IMemorySource
	{
		private IDictionary<IMemory, IList<IMemory>> m_memories;
		private int m_refCount;


		public TemporaryMemorySource (string id): base(id)
		{
			m_memories = new Dictionary<IMemory, IList<IMemory>> ();
			m_refCount = 0;

		}

		public IMemory Get (MemoryType type) {

			return m_memories.Keys.Where (m => m.Type == type).FirstOrDefault ();

		}

		public IMemory Get (int memoryId) {

			return m_memories.Keys.Where (m => m.Id == memoryId).FirstOrDefault ();

		}

		public ICollection<IMemory> Get (int[] memoryIds) {
		
			IList<IMemory> memories = new List<IMemory> ();

			foreach (int memoryId in memoryIds) {
			
				memories.Add( Get (memoryId));

			}

			return memories;

		}

		public ICollection<IMemory> All (MemoryType type = null) {

			if (type == null) {
			
				return m_memories.Keys;

			}

			return m_memories.Keys.Where (m => m.Type == type).ToList();

		}

		public ICollection<IMemory> GetAssociations (IMemory memory) {
	
			return m_memories [memory];

		}

		public IMemory Create (MemoryType type, string name) {
		
			IMemoryReference reference = new TemporaryMemoryReference (m_refCount++);
			reference.Type = type;
			reference.Value = name;
			IMemory memory = new Memory (reference, this);
			m_memories.Add (memory, new List<IMemory> ());

			return memory;

		}

		public bool Delete (int memoryId) {
		
			IMemory memory = m_memories.Keys.Where (m => m.Id == memoryId).FirstOrDefault ();

			if (memory == null) {
			
				return false;

			}

			IDictionary<IMemory,IList<IMemory>> removeFromUs = new Dictionary<IMemory,IList<IMemory>>();

			foreach (KeyValuePair<IMemory, IList<IMemory>> kvp in m_memories) {
			
				if (kvp.Value.Contains (memory)) {
				
					IList<IMemory> associations = kvp.Value;
					associations.Remove (memory);
					removeFromUs [kvp.Key] = associations;

				}

			}

			foreach (KeyValuePair<IMemory, IList<IMemory>> kvp in removeFromUs) {
			
				m_memories [kvp.Key] = kvp.Value;
			}

			return m_memories.Remove (memory);

		}

		public bool Delete (IMemory memory) {
		
			return Delete (memory.Id);

		}

		public bool Update (IMemory memory) {
		
			return true;

		}

		public void Associate (IMemory one, IMemory two) {
		
			m_memories [one]?.Add (two);
			m_memories [two]?.Add (one);
		}

	}

}

