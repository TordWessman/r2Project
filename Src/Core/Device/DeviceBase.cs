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
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Collections.Generic;

namespace R2Core.Device
{
	/// <summary>
	/// Base class usable by IDevice implementations. Providing some standard properties and functionality
	/// </summary>
	public abstract class DeviceBase : IDevice {
		
		protected string m_id;
		protected Guid m_guid;
		private List<WeakReference<IDeviceObserver>> m_observers;
        private readonly object m_lock;

		public DeviceBase(string id) {

            m_lock = new object();
			m_id = id;
			m_guid = Guid.NewGuid();
			m_observers = new List<WeakReference<IDeviceObserver>>();

		}

        /// <summary>
        /// An assigned identifier for this device
        /// </summary>
        /// <value>The identifier.</value>
		public string Identifier { get { return m_id; } }

        /// <summary>
        /// A unique identifier for this instance of a device
        /// </summary>
        /// <value>The GUID.</value>
		public Guid Guid { get { return m_guid; } }
		
		public virtual void Start() {}
		
		public virtual void Stop() {}
		
		public virtual bool Ready { get { return true; } }

		public void AddObserver(IDeviceObserver observer) {

            lock(m_lock)  {

                m_observers.Add(new WeakReference<IDeviceObserver>(observer));
            
            }
                
        }

        public void RemoveObserver(IDeviceObserver observer) {

            lock(m_lock) {

                WeakReference<IDeviceObserver> remove = null;

                m_observers.ForEach((refence) => {

                    IDeviceObserver obj = refence.GetTarget();

                    if (obj == observer) {

                        remove = refence;

                    }

                });

                if (remove != null) {

                    m_observers.Remove(remove);

                }

            }

        }

        /// <summary>
        /// Will notify observers of a value change
        /// </summary>
        protected void NotifyChange<T>(T value) {
		
			if (m_observers?.Count == 0) { return; }

			IDeviceNotification<object> deviceNotification = new DeviceNotification(m_id, GetCurrentMethod().Name, value);
			NotifyChange(deviceNotification);

		}

		protected void NotifyChange(IDeviceNotification<object> notification) {

            lock(m_lock)  {

                m_observers = m_observers.RemoveEmpty();

				m_observers.Sequential((obj) => obj.OnValueChanged(notification));
 
            }

        }

		[MethodImpl(MethodImplOptions.NoInlining)]
		private System.Reflection.MethodBase GetCurrentMethod() {

			StackTrace st = new StackTrace();
			StackFrame sf = st.GetFrame(2);

			return sf.GetMethod();

		}

    }

}


