using System;
using Core.Device;

namespace GPIO
{
	/// <summary>
	/// Base implementation for serial devices. Contains standard functionality including access to the ISerialHost and slaveId management.
	/// </summary>
	public abstract class SerialDeviceBase: DeviceBase, ISerialDevice
	{

		// The id of the device at the slave device
		private byte m_deviceId;
		// The id for the host where the device resides
		private byte m_hostId;

		// Represent the remote connection
		private ISerialHost m_host;

		protected ISerialHost Host { get { return m_host; } }
		protected byte HostId { get { return m_hostId; } }
		protected byte DeviceId { get { return m_deviceId; } }

		public SerialDeviceBase (string id, byte hostId, ISerialHost host): base(id) {
			
			m_host = host;
			m_hostId = hostId;

		}

		/// <summary>
		/// Initialize self on slave host. Return the new remote id
		/// </summary>
		protected abstract byte Update();

		public void Synchronize() {
			
			m_deviceId = Update(); 
		
		}

	}

}
