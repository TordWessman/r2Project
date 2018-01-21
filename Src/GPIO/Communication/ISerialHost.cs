using System;
using Core.Device;

namespace GPIO
{
	/// <summary>
	/// A hight level interface for interaction to a serial host exposing the available interactions. 
	/// </summary>
	public interface ISerialHost: IDevice
	{

		/// <summary>
		/// Returns the value property of an object.
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="slaveId">Slave identifier.</param>
		dynamic GetValue(byte slaveId, int host);

		/// <summary>
		/// Sets the value of an object.
		/// </summary>
		/// <param name="slaveId">Slave identifier.</param>
		/// <param name="value">Value.</param>
		void Set(byte slaveId, int host, int value);

		/// <summary>
		/// Creates a device on slave with id `hostId` of type `type` using `parameters` as arguments. Returns the remote id of the new device.
		/// </summary>
		/// <param name="hostId">Host identifier.</param>
		/// <param name="type">Type.</param>
		/// <param name="parameters">Parameters.</param>
		byte Create(int hostId, SerialDeviceType type, byte[] parameters);

		/// <summary>
		/// Initializes (resets) the specified host.
		/// </summary>
		/// <param name="host">Host.</param>
		void Initialize(int host);

		/// <summary>
		/// Delegate called whenever a host replied that it will be reset. The argument is the host name.
		/// </summary>
		Action<byte> HostDidReset { get; set; }

		/// <summary>
		/// Exposes the internal mechanism for sending packages.
		/// </summary>
		/// <param name="request">Request.</param>
		//DeviceResponsePackage Send(DeviceRequestPackage request);

	}

}
