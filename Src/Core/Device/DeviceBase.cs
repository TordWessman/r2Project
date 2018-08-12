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
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Dynamic;

namespace R2Core.Device
{
	/// <summary>
	/// Base class usable by IDevice implementations. Providing some standard properties and functionality
	/// </summary>
	public abstract class DeviceBase: IDevice
	{
		protected string m_id;
		protected Guid m_guid;
		private List<IDeviceObserver> m_observers;

		public DeviceBase (string id)
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

		/// <summary>
		/// Will notify observers of a value change
		/// </summary>
		protected void NotifyChange<T>(T value) {
		
			if (m_observers?.Count == 0) { return; }

			IDeviceNotification<object> deviceNotification = new DeviceNotification (m_id, GetCurrentMethod ().Name, value);
			NotifyChange (deviceNotification);

		}

		protected void NotifyChange(IDeviceNotification<object> deviceNotification) {

			Task.Factory.StartNew( () => m_observers.AsParallel ().ForAll (y => y.OnValueChanged (deviceNotification)) );

		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private System.Reflection.MethodBase GetCurrentMethod ()
		{
			StackTrace st = new StackTrace ();
			StackFrame sf = st.GetFrame (2);

			return sf.GetMethod();
		}
		
	}
}

