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
using Core.Network;
using System.Net;
using System.Linq;

using System.Threading.Tasks;

namespace Core.Device
{
	/// <summary>
	/// Implementation of IDeviceManager. Able to handle remote devices.
	/// </summary>
	public class DeviceManager : DeviceBase, IDeviceManager
	{
		private IDictionary<Guid, IDevice> m_devices;

		private IList<IDeviceManagerObserver> m_observers;
		
		private static readonly object m_lock = new object ();
		private static readonly object m_removelock = new object ();

		/// <summary>
		/// Initializes a new instance of the <see cref="Core.Device.DeviceManager"/.
		/// </summary>
		public DeviceManager (string id) : base(id)
		{
			m_devices = new Dictionary<Guid, IDevice> ();
			m_observers = new List<IDeviceManagerObserver> ();
		}

		public IEnumerable<IDevice> LocalDevices { get { return m_devices.Values.Where (device => !(device is IRemoteDevice)); } }

		public IEnumerable<IDevice> Devices { get { return m_devices.Values; } }

		/// <summary>
		/// Add the device internally
		/// </summary>
		/// <param name="newDevice">New device.</param>
		private void _Add (IDevice newDevice)
		{
			lock (m_lock) {

				if (m_devices.ContainsKey (newDevice.Guid)) {

					Log.w ("Replacing device duplicated device with id: " + newDevice.Identifier);
					m_devices.Remove (newDevice.Guid);
					m_devices.Add (newDevice.Guid, newDevice);
				
				} else {
				
					m_devices.Add (newDevice.Guid, newDevice);
				
				}

				// Device Manager will be able to propagate changes in any device to it's own observers. 
				newDevice.AddObserver (this);
			
			}

			foreach (IDeviceManagerObserver observer in m_observers) {
			
				observer.DeviceAdded (newDevice);

			}

		}

		private IDevice _Get(string identifier) {
		
			return  m_devices.Select (d => d.Value).Where(dv => dv.Identifier == identifier).FirstOrDefault ();

		}
		
		public void Add (IDevice newDevice)
		{
			/*
			if (m_hostManager.Ready && newDevice is IRemotlyAccessable) {

				//Broadcast to network:

				if (newDevice is IRemotlyAccessable) {

					m_hostManager.SendToAll (GetDeviceAddedPackage(newDevice));

				}

			}*/
			
			_Add (newDevice);

		}

		public T GetByGuid<T> (Guid guid) 
		{

			IDevice device = m_devices [guid];

			if (device != null && device is T) {

				return  (T) device;

			} else {

				throw new InvalidCastException ("Device with Guid: " + guid.ToString() + " cannot be retrieved, is correct type: " + (device is T).ToString());

			}

		}

		public T Get<T> (string identifier) 
		{

			IDevice device = _Get (identifier);

			if (device == null) {

				throw new DeviceException ("Device identifier not found: " + identifier);

			}

			if (device is T) {

				return  (T) device;

			} else {

				throw new InvalidCastException ("Device: " + identifier + " cannot be retrieved, since it is not of type: " + typeof (T));

			}

		}

		public dynamic Get (string identifier) {

			return Get<IDevice> (identifier);

		}

		public void AddObserver (IDeviceManagerObserver observer) {
		
			m_observers.Add (observer);

		}

		public void Stop(IDevice[] ignoreDevices) {
		
			if (ignoreDevices == null) {
			
				ignoreDevices = new IDevice[] { };

			}

			lock (m_lock) {

				foreach (IDevice device in m_devices.Values) {

					if (!(device is IRemoteDevice) && !ignoreDevices.Any(d => d.Guid == device.Guid )) {

						Log.d ("Stopping device: " + device.Identifier);

						device.Stop ();

					}

				}

			}

		}

		#region INewHostIdentified implementation

		public bool Has (string identifier)
		{

			lock (m_lock) {

				return _Get (identifier) != null;
			}

		}

		#endregion

		private void _Remove (Guid guid)
		{

			lock (m_lock) {

				if (m_devices.ContainsKey (guid)) {

					m_devices.Remove (guid);

				}

			}

		}

		public void Remove (string id)
		{

			lock (m_removelock) {

				IDevice device = Get<IDevice> (id);

				if (device != null) {

					foreach (IDeviceManagerObserver observer in m_observers) {

						observer.DeviceRemoved (device);

					}
					/*
					if (device is IRemotlyAccessable) {

						m_hostManager.SendToAll (GetDeviceRemovedPackage (device));

					}*/

					_Remove (device.Guid);

				}

			}

		}

		public void PrintDevices ()
		{
			foreach (IDevice device in m_devices.Values) {

				if (Has (device.Identifier)) {
				
					Log.d (device.Identifier + "    - [" + device.Guid.ToString() + "]" + (device is IRemoteDevice ? " (remote)" : "")); 
				
				} else {
				
					Log.e (device.Identifier + " does not exist - yet it does. It's an esoterical device. Please contact God for more information.");
				
				}
				
			}
		
		}

		#region IDeviceObserver

		public void OnValueChanged(IDeviceNotification<object> notification) {
		
			NotifyChange (notification);

		}

		#endregion
	
	}

}

