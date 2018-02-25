﻿using System;
using Core.Device;

namespace GPIO
{

	public struct DeviceData<T> {
	
		public byte Id;
		public T Value;

	}

	/// <summary>
	/// A hight level interface for interaction to a serial host exposing the available interactions. 
	/// </summary>
	public interface ISerialHost: IDevice
	{

		/// <summary>
		/// Returns the value property of a device on slave with id `nodeId`. `slaveId` is the id (local on the slave) of the device.
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="slaveId">Slave identifier.</param>
		DeviceData<T> GetValue<T>(byte slaveId, int nodeId);

		/// <summary>
		/// Sets the value of an object. `slaveId` is the id of the device located on the node with id `nodeId`. 
		/// </summary>
		/// <param name="slaveId">Slave identifier.</param>
		/// <param name="value">Value.</param>
		void Set(byte slaveId, int nodeId, int value);

		/// <summary>
		/// Creates a device on slave with id `nodeId` of type `type` using `parameters` as arguments. Returns the remote id and the values of the new device.
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

		// Permanently name this node `nodeId`. This method is not propagated. 
		void SetNodeId(int nodeId);

		/// <summary>
		/// Delegate called whenever a host replied that it will be reset. The argument is the host name.
		/// </summary>
		Action<byte> HostDidReset { get; set; }

		void WaitFor(int nodeId);
	}

}
