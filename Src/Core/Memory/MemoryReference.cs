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
using MemoryType = System.String;

namespace Core.Memory
{
	[Serializable]
	public class MemoryReference : IMemoryReference
	{
		public static readonly int NULL_REFERENCE_ID = -1;
		
		protected int m_id;
		protected string m_name;
		protected MemoryType m_type;

		public int Id {

			get {
			
				return m_id;
			
			}
		
		}

		
		public MemoryType Type { 
		
			get {

				return m_type;
			
			}

			set {

				m_type = value.Replace ("\"", "");
			
			}
		
		}
		
		public string Value { 

			get {

				return m_name;
			
			}
		
			set {

				m_name = value.Replace ("\"", "");
			
			}

		}

		public MemoryReference (int id, 
		                        string name, 
		                        string type)
		{

			m_id = id;
			m_name = name;
			m_type = type;
			
		}
		
		public override bool Equals (Object obj)
		{

			IMemoryReference reference = obj as MemoryReference; 

			if (reference == null) {
			
				return false;

			} else {
			
				return m_id == reference.Id /*&& 
					m_name == reference.Name &&
					m_type == reference.Type*/;

			}

	   }
		
		public override string ToString ()
		{

			return string.Format ("MEMORY [{0}] - TYPE: [{1}] NAME: [{2}] ", m_id,m_type,m_name);
        
		}
		
		public override int GetHashCode ()
		{

			return m_id;
		
		}



		public bool IsNull { 

			get {

				return m_id == NULL_REFERENCE_ID;

			}
		
		}
		
	}

}