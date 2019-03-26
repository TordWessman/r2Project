using System;
using R2Core.Device;

namespace R2Core.GPIO
{

	public struct DeviceData<T> {
	
		public byte Id;
		public T Value;

	}

	/// <summary>
	/// A hight level interface for interaction to a serial host exposing the available interactions. 
	/// </summary>
	public interface ISerialHost: IDevice {

		/// <summary>
		/// Number of retries for a transmission before giving up and rethrowing IO & Serial exceptions.
		/// </summary>
		/// <value>The retry count.</value>
		int RetryCount { get; set; }

		/// <summary>
		/// Returns the value property of a device on node with id `nodeId`. `deviceId` is the id (local on the node) of the device.
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="nodeId">Node identifier.</param>
		DeviceData<T> GetValue<T>(byte deviceId, int nodeId);

		/// <summary>
		/// Sets the value of an object. `deviceId` is the id of the device located on the node with id `nodeId`. 
		/// </summary>
		/// <param name="deviceId">Device identifier.</param>
		/// <param name="nodeId">Id of the node.</param>
		/// <param name="value">Value.</param>
		void Set(byte deviceId, int nodeId, int value);

		/// <summary>
		/// Creates a device on node with id `nodeId` of type `type` using `parameters` as arguments. Returns the remote id and the values of the new device.
		/// </summary>
		/// <param name="nodeId">Host identifier.</param>
		/// <param name="type">Type.</param>
		/// <param name="parameters">Parameters.</param>
		DeviceData<T> Create<T>(int nodeId, SerialDeviceType type, byte[] parameters);

		/// <summary>
		/// Initializes (resets) the specified host.
		/// </summary>
		/// <param name="host">Host.</param>
		void Initialize(int nodeId);

		/// <summary>
		/// Resets the node. Set state to uninitialized and remove all devices.
		/// </summary>
		/// <param name="nodeId">Node identifier.</param>
		void Reset(int nodeId);

		/// <summary>
		/// Returns true if the specified node is currently registered at the master (not to be confused with sleep mode).
		/// </summary>
		/// <returns><c>true</c> if this instance is node available the specified host; otherwise, <c>false</c>.</returns>
		/// <param name="host">Host.</param>
		bool IsNodeAvailable(int nodeId);

		/// <summary>
		/// Returns true if the node has been sent to sleep
		/// </summary>
		/// <returns><c>true</c> if this instance is node sleeping the specified nodeId; otherwise, <c>false</c>.</returns>
		/// <param name="nodeId">Node identifier.</param>
		bool IsNodeSleeping(int nodeId);

		/// <summary>
		/// Retrieves the nodes connected to the host. The number might be limited to response content size.
		/// </summary>
		/// <returns>The nodes.</returns>
		byte[] GetNodes();

		/// <summary>
		/// Sets a node to sleep mode. Sleep the specified nodeId for `cycles` (1 cycle equals ~8 seconds).
		/// </summary>
		void Sleep(int nodeId, bool toggle, int cycles = ArduinoSerialPackageFactory.RH24_SLEEP_UNTIL_MESSAGE_RECEIVED);

		/// <summary>
		/// Allows the node to stay awake for up to 60 seconds. Use this method if multiple actions are required (i.e. synchronization and value updates) in order to lower the writing to the nodes EEPROM.
		/// </summary>
		/// <param name="nodeId">Node identifier.</param>
		/// <param name="seconds">Seconds.</param>
		void PauseSleep(int nodeId, int seconds);

		/// <summary>
		/// Permanently name this node `nodeId`. This method is not propagated.
		/// </summary>
		/// <param name="nodeId">Node identifier.</param>
		void SetNodeId(int nodeId);

		/// <summary>
		/// Returns the checksum calculated by the node with id ´nodeId´.
		/// </summary>
		/// <returns>The checksum.</returns>
		/// <param name="nodeId">Node identifier.</param>
		byte[] GetChecksum(int nodeId);

		/// <summary>
		/// Delegate called whenever a host replied that it will be reset. The argument is the host name.
		/// </summary>
		Action<byte> HostDidReset { get; set; }

		void WaitFor(int nodeId);
	}

}
