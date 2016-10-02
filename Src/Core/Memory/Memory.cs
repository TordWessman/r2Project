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
using System.Collections.Generic;
using System.Linq;

using MemoryType = System.String;
using Core.Device;

namespace Core.Memory
{
	public class Memory : IMemory
	{
		private IMemoryReference m_reference;
		private IMemorySource m_source;
		private bool m_isLocal;

		public IMemoryReference Reference { get { return m_reference; } }
		
		public int GetId ()
		{

			return Id;
		
		}
		
		public int Id {

			get {

				return m_reference.Id;

			}

		}

		public bool IsLocal { get { return m_isLocal; } }
		
		public MemoryType Type 
		{ 

			get {

				return m_reference.Type;

			}

			set {

				m_reference.Type = value;
				m_source.Update (this);

			}

		}
		
		public string Value {

			get {

				return m_reference.Value;
			
			}

			set {

				m_reference.Value = value;
				m_source.Update (this);

			}

		}

		public Memory (IMemoryReference reference, 
		               IMemorySource source,
						bool isLocal)
		{
			m_isLocal = isLocal;
			m_reference = reference;
			m_source = source;

		}
		
		
		public ICollection<IMemory> Associations {

			get {
			
				return m_source.GetAssociations (this);

			}
		}
		
		public void Associate (IMemory other)
		{
			m_source.Associate (this, other);
		}
		
		public override bool Equals (Object obj)
		{
			IMemory memory = obj as Memory;
			
			if (memory == null)
				return false;
			else 
				return m_reference.Equals (memory.Reference);
		}
		
		public override string ToString ()
		{
			return m_reference.ToString ();
		}
		
		public override int GetHashCode ()
		{
			return m_reference.GetHashCode ();
		}
		
		public IMemory GetAssociation (MemoryType type)
		{
			return (from m in Associations where 
				m.Type == type.ToLower ()
			                       select m).FirstOrDefault ();
		}
		
		public ICollection<IMemory> GetAssociations (MemoryType type)
		{
			return (from m in Associations where 
				m.Type == type.ToLower ()
			        select m).ToList<IMemory>();
		}

		public bool IsNull {
		
			get {
			
				return m_reference.IsNull;
			
			}

		}
		
	}

}