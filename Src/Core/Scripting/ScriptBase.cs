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
//
using System;
using System.Dynamic;
using Core.Device;
using System.Collections.Generic;

namespace Core.Scripting
{
	public abstract class ScriptBase: DynamicObject, IScript 
	{
		protected string m_id;
		protected Guid m_guid;
		private List<IDeviceObserver> m_observers;

		public ScriptBase (string id)
		{
			m_id = id;
			m_guid = Guid.NewGuid ();
			m_observers = new List<IDeviceObserver> ();
		}

		public string Identifier { get {
				return m_id;}}

		public Guid Guid { get { return m_guid; } }

		public virtual void Start () {}

		public virtual void Stop () {}

		public virtual bool Ready { get { return true; } }

		public void AddObserver(IDeviceObserver observer) { m_observers.Add (observer); }

		public abstract void Set (string handle, dynamic value);
		public abstract dynamic Get (string handle);
		public abstract void Reload();
		public abstract dynamic Invoke (string handle, params dynamic[] args);

		public override bool TrySetMember (SetMemberBinder binder, object value) {

			Set (binder.Name, value);
			return true;

		}

		public override bool TryGetMember (GetMemberBinder binder, out object result) {
		
			result = Get (binder.Name);

			return true;

		}

		public override bool TryInvokeMember (InvokeMemberBinder binder, object[] args, out object result)
		{
			
			result = Invoke (binder.Name, args);
			return true;

		}
		/*
		public override bool TryInvokeM (InvokeBinder binder, object[] args, out object result)
		{
			result = Invoke(binder
		}*/

	}
}

