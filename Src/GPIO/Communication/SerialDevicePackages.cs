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
		InitializationOk = 0x5 // Response header telling that the initialization was successfull.
	}

	public struct DeviceResponsePackage {
	
		// Currently not used. Expected to be equal to DEVICE_HOST_LOCAL.
		public byte Host;

		// Action required by slave.
		public ActionType Action;

		// Id of an affected device on slave. A slave have a limited range of id:s (i.e. 0-19).
		public byte Id;

		// Contains the response data (value, error message or null).
		public byte[] Content;

		private const int POSITION_HOST = 0;
		private const int POSITION_ACTION = 1;
		private const int POSITION_ID = 2;
		private const int POSITION_CONTENT_LENGTH = 3;
		private const int POSITION_CONTENT = 4;

		// The number of int16 returned from slave upon a ActionType.Get request.
		private const int NUMBER_OF_RETURN_VALUES = 2;

		// If the POSITION_ACTION bart has this value, the response was an error.
		private const byte ACTION_ERROR = 0xF0;

		public DeviceResponsePackage(byte[] response) {
		
			Host = response [POSITION_HOST];
			Action = (ActionType) response [POSITION_ACTION];
			Id = response [POSITION_ID];
			int contentLength = response [POSITION_CONTENT_LENGTH];
			Content = contentLength > 0 ? response.Skip (POSITION_CONTENT).Take (contentLength)?.ToArray() ?? new byte[]{} : new byte[]{};

		}

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
				
					return System.Text.Encoding.Default.GetString (Content);
				
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
		public byte Host;

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

			Console.WriteLine(Marshal.SizeOf(typeof(DeviceRequestPackage)));
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
}

