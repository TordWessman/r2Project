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

		// Represent the remote connection
		private ISerialHost m_host;

		// The node to which this node is attached
		private ISerialNode m_node;

		// The cached internal value of this node
		protected T InternalValue;

		// The ISerialHost this device is using to transmit data.
		protected ISerialHost Host { get { return m_host; } }

		// Remote device id
		protected byte DeviceId { get { return m_deviceId; } }

		public byte NodeId { get { return m_node.NodeId; } }
		public bool IsSleeping { get { return m_node.Sleep; } }

		internal SerialDeviceBase (string id, ISerialNode node, ISerialHost host): base(id) {
			
			m_host = host;
			m_node = node;
			m_node.Track (this);

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

			InternalValue = Host.GetValue<T> (m_deviceId, NodeId).Value;

		}

		public void Synchronize() {
					
			DeviceData<T> info = Host.Create<T> ((byte)NodeId, DeviceType, CreationParameters);
			m_deviceId = info.Id;
			InternalValue = info.Value;

			Core.Log.t ($"Created device and got id: {m_deviceId}");

		}

	}

}
