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
using System.Runtime.InteropServices;
using System.Linq;
using Core.Data;

namespace GPIO
{
	/// <summary>
	/// Device types defined in r2I2CDeviceRouter.h
	/// </summary>
	public enum SerialDeviceType: byte
	{
		
		DigitalInput = 0x1,		// Simple digital input
		DigitalOutput = 0x2,	// Simple digital output
		AnalogueInput = 0x3,	// Uses slave's AD converter to read a value
		Servo = 0x4,			// PWM Servo
		Sonar_HCSR04 = 0x5,		// HC-SR04 Sonar implementation
		DHT11 = 0x6				// DHT11 Temperature/Humidity sensor
			
	}

	/// <summary>
	/// Actions defined in r2I2CDeviceRouter.h
	/// </summary>
	public enum ActionType: byte {
		
		Unknown = 0x0,	// Hmm... This must be an error 
		Create = 0x1,	// Tell the slave to create a device
		Set = 0x2,		// Set the value of a device on slave
		Get = 0x3,		// Return the value of a device
		Error = 0xF0,	// Yep, this was an error (only used by responses).
		Initialization = 0x4, // As a request header, this tells the slave that it's ready for communication. As a response header, this indicate that the slave has been rebooted and needs to be reinitialized.
		InitializationOk = 0x5, // Response header telling that the initialization was successfull.
		SetNodeId = 0x6, // This action will cause the node to change it's id. This action should not be propagated.
		IsNodeAvailable = 0x7, // Check if a specified host, determined by `Host` is currently connected.
		GetNodes = 0x8, // Returns all node Id:s currently connected (the number is limited by MAX_CONTENT_SIZE in r2I2CDeviceRouter.h)
		SendToSleep = 0x0A // Sends the node to sleep
	}

	public enum ErrorType: byte {
	
		// No error status was received, but response indicated an error.
		Undefined = 0,
		// No device with the specified id was found.
		NO_DEVICE_FOUND = 1,
		// The port you're trying to assign is in use.
		PORT_IN_USE = 2,
		// The device type specified was not declared
		DEVICE_TYPE_NOT_FOUND = 3,
		// Too many devices has been allocated
		MAX_DEVICES_IN_USE = 4,
		// This device does not suport set operation
		DEVICE_TYPE_NOT_FOUND_SET_DEVICE = 5,
		// This device does not support read operation
		DEVICE_TYPE_NOT_FOUND_READ_DEVICE = 6,
		// Unknown action received
		UNKNOWN_ACTION = 7,
		// General device read error
		DEVICE_READ_ERROR = 8,
		// The node (during remote communication) was not available.
		RH24_NODE_NOT_AVAILABLE = 9,
		// Whenever a read did not return the expected size
		RH24_BAD_SIZE_READ = 10,
		// Called if a read-write timed out during RH24 communication
		RH24_TIMEOUT = 11,
		// If a write operation fails for RH24
		RH24_WRITE_ERROR = 12,
		// Unknown message. Returned if the host receives a message with an unexpected type.
		RH24_UNKNOWN_MESSAGE_TYPE_ERROR = 13,
		// If the the master's read operation was unable to retrieve data.
		ERROR_RH24_NO_NETWORK_DATA = 14,
		// Routing through a node that is not the master is not supported.
		ERROR_RH24_ROUTING_THROUGH_NON_MASTER = 15,
		// If the master receives two messages with the same id
		ERROR_RH24_DUPLICATE_MESSAGES = 16,
		// Failed to make the node sleep
		ERROR_FAILED_TO_SLEEP = 18,
		// Messages are not in sync. Unrecieved messages found in the masters input buffer.
		ERROR_RH24_MESSAGE_SYNCHRONIZATION = 19
	}

	public struct DeviceResponsePackage {
	
		// Id for a message. Used for debugging.
		public byte MessageId;

		// Currently not used. Expected to be equal to DEVICE_HOST_LOCAL.
		public byte NodeId;

		// Action required by slave.
		public ActionType Action;

		// Id of an affected device on slave. A slave have a limited range of id:s (i.e. 0-19).
		public byte Id;

		// Contains the response data (value, error message or null).
		public byte[] Content;

		// The number of int16 returned from slave upon a ActionType.Get request.
		public const int NUMBER_OF_RETURN_VALUES = 2;

		/// <summary>
		/// If true, the request to slave generated an error.
		/// </summary>
		/// <value><c>true</c> if this instance is error; otherwise, <c>false</c>.</value>
		public bool IsError { get { return Action == ActionType.Error || Action == ActionType.Unknown; } }

		/// <summary>
		/// Contains the response. Normally it's an int16 containing some requested response data, but it can also conains a string (error message).
		/// </summary>
		/// <value>The value.</value>
		public dynamic Value {
		
			get {
			
				if (IsError) {
				
					return (Content?.Length ?? 0) > 0 ? (ErrorType) Content[ArduinoSerialPackageFactory.POSITION_CONTENT_POSITION_ERROR_TYPE] : ErrorType.Undefined;
				
				} else if (Action == ActionType.IsNodeAvailable) {

					return Content [0] > 0; 

				} else if (Action == ActionType.Get) {
				
					int[] values = new int[NUMBER_OF_RETURN_VALUES];

					for (int i = 0; i < NUMBER_OF_RETURN_VALUES; i++) { values[i] = Content.ToInt (i * 2, 2); }

					return values;
				}

				return null;
			
			}
		
		}

	}

	/// <summary>
	/// Represents a request sent to slave (for creating a device, getting a value etc).
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public struct DeviceRequestPackage
	{
		// Currently not used. Expected to be equal to DEVICE_HOST_LOCAL.
		public byte NodeId;

		// Action required by slave.
		public byte Action;

		// Id of an affected device on slave. A slave have a limited range of id:s (i.e. 0-19).
		public byte Id;

		// Additional data sent to slave.
		public byte[] Content;

		/// <summary>
		/// Converts the package to a byte array before transmission.
		/// </summary>
		/// <returns>The bytes.</returns>
		public byte[] ToBytes()
		{  

			Byte[] structData = new Byte[3 + (Content?.Length ?? 0)]; 

			GCHandle pinStructure = GCHandle.Alloc(this, GCHandleType.Pinned);  

			try {  

				Marshal.Copy(pinStructure.AddrOfPinnedObject(), structData, 0, structData.Length); 

				if (Content?.Length > 0) {

					Array.Copy(Content,0,structData,3, Content.Length);

				}

				return structData;  

			} finally {  

				pinStructure.Free();  

			}  

		}  

	}

	public static class ResponsePackageExtensions {
	
		/// <summary>
		/// Helps identify the info part of the error message 
		/// </summary>
		/// <returns>The error info.</returns>
		/// <param name="response">Response.</param>
		public static string GetErrorInfo(this DeviceResponsePackage response) {

			int info = (response.Content?.Length ?? 0) > 1 ? response.Content [ArduinoSerialPackageFactory.POSITION_CONTENT_POSITION_ERROR_INFO] : 0;

			if (response.Value == ErrorType.ERROR_RH24_MESSAGE_SYNCHRONIZATION) {

				// Will return the action of an unread message in the master node's pipe.
				return $"Action from the previous message: `{(ActionType) info}`";

			}

			return info.ToString ();
		}

	}
}

