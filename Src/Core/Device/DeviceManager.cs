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
using Core.Network.Data;
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
		private IHostManager<IPEndPoint> m_hostManager;
		private IRPCManager<IPEndPoint> m_rpcManager;
		
		private INetworkPackageFactory m_networkPackageFactory;
		private RemoteDeviceFactory m_remoteDeviceFactory;

		private IList<IDeviceManagerObserver> m_observers;
		
		private static readonly object m_lock = new object ();
		private static readonly object m_removelock = new object ();

		/// <summary>
		/// Initializes a new instance of the <see cref="Core.Device.DeviceManager"/> requiring a IHostManager and IRPCManager to handle remote devices.
		/// </summary>
		/// <param name="hostManager">Host manager.</param>
		/// <param name="rpcManager">Rpc manager.</param>
		public DeviceManager (string id, IHostManager<IPEndPoint> hostManager, IRPCManager<IPEndPoint> rpcManager, INetworkPackageFactory networkPackageFactory) : base(id)
		{
			m_rpcManager = rpcManager;
			m_hostManager = hostManager;
			m_devices = new Dictionary<Guid, IDevice> ();
			m_networkPackageFactory = networkPackageFactory;
			m_remoteDeviceFactory = new RemoteDeviceFactory (m_rpcManager);
			m_observers = new List<IDeviceManagerObserver> ();

			//Listen to deviceAdded events:
			m_hostManager.Server.AddObserver (DataPackageType.DeviceAdded, this);
			m_hostManager.Server.AddObserver (DataPackageType.DeviceRemoved, this);
			//Listen to the rpc event:
			m_hostManager.Server.AddObserver (DataPackageType.Rpc, this);
			
			m_hostManager.AddObserver (this);
			
		}

		public IEnumerable<IDevice> LocalDevices { get { return m_devices.Values.Where (device => !(device is IRemotlyAccessable)); } }

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

			if (m_hostManager.Ready && newDevice is IRemotlyAccessable) {

				//Broadcast to network:

				if (newDevice is IRemotlyAccessable) {

					m_hostManager.SendToAll (GetDeviceAddedPackage(newDevice));

				}

			}
			
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
		
		private IDataPackage GetDeviceAddedPackage (IDevice device)
		{

			if (!(device is IRemotlyAccessable)) {
			
				throw new DeviceException("Device is not remote device.");
			
			}
			
			IRemotlyAccessable remoteDevice = (IRemotlyAccessable)device;

			return m_networkPackageFactory.CreateDeviceAddedPackage (
				device.Identifier,
				device.Guid,
				remoteDevice.GetTypeId ().ToString (),
				m_hostManager.Server);

		}
		
		
		private IDataPackage GetDeviceRemovedPackage (IDevice device)
		{
		
			return m_networkPackageFactory.CreateDeviceRemovedPackage (
				device.Guid);
		
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

		/// <summary>
		/// Called whenever a host manager identifies a new host. Will send my devices to remote host
		/// </summary>
		/// <param name="host">Host.</param>
		public void NewHostIdentified (System.Net.IPEndPoint host)
		{

			Log.d ("Will send my devices to the new host: " + host.ToString ());

			lock (m_lock) {

				foreach (IDevice device in (
					from remoteDevice in m_devices.Values 
					where remoteDevice is IRemotlyAccessable 
					select remoteDevice)) {

					IDevice deviceCopy = device;
					Task sendTask = new Task (() => {

						ClientConnection con = new ClientConnection (host);
						con.Send (m_networkPackageFactory.Serialize (
						GetDeviceAddedPackage (deviceCopy)
						));

					});

					sendTask.Start ();
					sendTask.Wait ();
				
				}
			
			}

		}

		#endregion

		#region IDataReceived implementation

		/// <summary>
		/// Handle information received by the serer
		/// </summary>
		/// <returns>The received.</returns>
		/// <param name="type">Type.</param>
		/// <param name="rawData">Raw data.</param>
		/// <param name="source">Source.</param>
		public byte[] DataReceived (DataPackageType type, byte[] rawData, IPEndPoint source)
		{

			IDataPackageHeader header = m_networkPackageFactory.UnserializeHeader (rawData);
			Guid guid = Guid.Empty;

			switch (type) {

			case DataPackageType.DeviceRemoved:

				guid = new Guid (header.GetValue (HeaderFields.Target.ToString ()));

				_Remove (guid);
				return null;

			case DataPackageType.DeviceAdded:
				
				string deviceId = header.GetValue (HeaderFields.DeviceId.ToString ());
				string deviceType = header.GetValue (HeaderFields.DeviceType.ToString ());
				guid = new Guid (header.GetValue (HeaderFields.Target.ToString ()));
				
				string sourceIp = header.GetValue (HeaderFields.Ip.ToString ());
				int sourcePort = int.Parse (header.GetValue (HeaderFields.Port.ToString ()));

				Log.d ("New remote device added: " + deviceId + " type: " + deviceType);
				IPEndPoint newHost = new IPEndPoint (IPAddress.Parse (sourceIp), sourcePort);

				IDevice newRemoteDevice = m_remoteDeviceFactory.CreateRemoteDevice (
					                          deviceId,
					                          guid,
					                          deviceType,
					                          newHost);
			

				// Do not add remote devices with the same identifier + host

				List<Guid> removeDevices = new List<Guid> ();

				foreach (Guid remoteDevice in m_devices.Values.Select (d => d as IRemoteDevice).Where (d => d is IRemoteDevice).Where (d => d.Host.Equals (newHost) && d.Identifier == deviceId).Select(di => di.Guid)) {

					// Make a copy...
					removeDevices.Add (remoteDevice);

				}

				foreach (Guid removeDevice in removeDevices) {
				
					Log.w ("Remote device was duplicate on id/host and will be replaced: " + deviceId);
					_Remove (removeDevice);

				}

				_Add (newRemoteDevice);

				return null;

			case DataPackageType.Rpc:
			
				if (IsStandardRemoteDeviceMethod (header.GetValue (HeaderFields.Method.ToString ()))) {
	
					guid = new Guid (header.GetValue (HeaderFields.Target.ToString ()));

					IDevice device = GetByGuid<IDevice> (guid);
					string methodName = header.GetValue (HeaderFields.Method.ToString ());
					
					if (methodName == StandardRemoteDeviceMethods.Ready.ToString ()) {
					
						return m_rpcManager.RPCReply<bool> (device.Guid, methodName, device.Ready);
					
					} else if (methodName == StandardRemoteDeviceMethods.Stop.ToString ()) {

						device.Stop ();
					
					} else if (methodName == StandardRemoteDeviceMethods.Start.ToString ()) {
					
						device.Start ();
					
					} else {
					
						throw new NotImplementedException ("Have you added new standard methods? Method not found in DeviceManager: " + methodName);
					
					}
					
				} else {
				
					guid = new Guid (header.GetValue (HeaderFields.Target.ToString ()));

					IRemotlyAccessable device = GetByGuid<IRemotlyAccessable> (guid);
			
					return device.RemoteRequest (header.GetValue (HeaderFields.Method.ToString ()),
			                             rawData, m_rpcManager);

				}

				break;

			default:

				throw new NotImplementedException ("Unknown answer for: " + type.ToString ());
			
			}

			return null;
			
		}
		
		private bool IsStandardRemoteDeviceMethod (string methodName)
		{

			foreach (string enumName in Enum.GetNames(typeof(StandardRemoteDeviceMethods))) {

				if (enumName.Equals (methodName)) {
				
					return true;
			
				}
		
			}
			
			return false;
		
		}
		
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

					if (device is IRemotlyAccessable) {

						m_hostManager.SendToAll (GetDeviceRemovedPackage (device));

					}

					_Remove (device.Guid);

				}

			}

		}

		#region IHostManagerObserver implementation

		/// <summary>
		/// Will remove all devices from the specified host.
		/// </summary>
		/// <param name="host">Host.</param>
		public void HostDropped (IPEndPoint host)
		{

			Log.d ("Will remove my devices from host: " + host.ToString ());

			ICollection<Guid> removeDevices = new List<Guid> ();

			foreach (IDevice device in (
				from remoteDevice in m_devices.Values 
				where remoteDevice is IRemoteDevice 
				select remoteDevice)) {

				if (((IRemoteDevice)device).Host.Equals (host)) {
				
					removeDevices.Add (device.Guid);

				}

			}

			foreach (Guid guid in removeDevices) {

				_Remove (guid);
			
			}

		}

		#endregion

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

