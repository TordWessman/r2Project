using System;
using Core.Device;

namespace GPIO
{
	/// <summary>
	/// Base implementation for serial devices. Contains standard functionality including access to the ISerialHost and slaveId management.
	/// </summary>
	internal abstract class SerialDeviceBase<T>: DeviceBase, ISerialDevice
	{

		// The id of the device at the slave device
		private byte m_deviceId;
		// The id for the host where the device resides
		private byte m_nodeId;

		// Represent the remote connection
		private ISerialHost m_host;

		private bool m_isSleeping;

		protected T InternalValue;
		protected ISerialHost Host { get { return m_host; } }
		protected byte DeviceId { get { return m_deviceId; } }

		public byte NodeId { get { return m_nodeId; } }
		public bool IsSleeping { get { return m_isSleeping; } set {m_isSleeping = value;} }

		internal SerialDeviceBase (string id, byte nodeId, ISerialHost host): base(id) {
			
			m_host = host;
			m_nodeId = nodeId;

		}

		/// <summary>
		/// For creation of the device remotely: Parameters (i.e. ports) required
		/// </summary>
		protected abstract byte[] CreationParameters { get; }

		/// <summary>
		/// For creation of the device remotely: The explicit device type.
		/// </summary>
		/// <value>The type of the device.</value>
		/// 
		protected abstract SerialDeviceType DeviceType { get; }

		/// <summary>
		/// Determines the sleep state of the node. Will retrieve an updated value if not sleeping or return the privious value if it is. 
		/// </summary>
		/// <returns>The value.</returns>
		protected T GetValue() {

			if (!IsSleeping) { Update (); }

			return InternalValue; 

		}

		public void Update() {

			InternalValue = Host.GetValue<T> (m_deviceId, m_nodeId).Value;

		}

		public void Synchronize() {
					
			DeviceData<T> info = Host.Create<T> ((byte)NodeId, DeviceType, CreationParameters);
			m_deviceId = info.Id;
			InternalValue = info.Value;

			Core.Log.t ($"Created device and got id: {m_deviceId}");

		}

	}

}
