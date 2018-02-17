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
		private byte m_nodeId;

		// Represent the remote connection
		private ISerialHost m_host;

		protected ISerialHost Host { get { return m_host; } }
		protected byte NodeId { get { return m_nodeId; } }
		protected byte DeviceId { get { return m_deviceId; } }

		public SerialDeviceBase (string id, byte nodeId, ISerialHost host): base(id) {
			
			m_host = host;
			m_nodeId = nodeId;

		}

		/// <summary>
		/// Initializes (creates) self on node. Return the new remote id
		/// </summary>
		protected abstract byte ReCreate();

		public void Synchronize() {
			
			m_deviceId = ReCreate();
			Core.Log.t ($"Device id: {m_deviceId}");

		}

	}

}
