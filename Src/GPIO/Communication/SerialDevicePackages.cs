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
	public enum DeviceType: byte
	{
		DigitalInput = 0x1,
		DigitalOutput = 0x2,
		AnalogueInput = 0x3,
		Servo = 0x4,
		Sonar_HCSR04 = 0x5
	}

	/// <summary>
	/// Actions defined in r2I2CDeviceRouter.h
	/// </summary>
	public enum ActionType: byte {
		Unknown = 0x0,
		Create = 0x1,
		Set = 0x2,
		Get = 0x3
	}

	public struct DeviceResponsePackage {
	
		// Currently not used. Expected to be equal to DEVICE_HOST_LOCAL.
		public byte Host;

		// Action required by slave.
		public byte Action;

		// Id of an affected device on slave. A slave have a limited range of id:s (i.e. 0-19).
		public byte Id;

		// Contains the response data (value, error message or null).
		public byte[] Content;

		public DeviceResponsePackage(byte[] response) {
		
			Host = response [0];
			Action = response [1];
			Id = response [2];
			int contentLength = response [3];
			Content = response.Skip (4).Take (contentLength)?.ToArray() ?? new byte[]{};

		}

		public bool IsError { get { return Action == (byte)ActionType.Unknown; } }

		public dynamic Value {
		
			get {
			
				if (IsError) {
				
					return System.Text.Encoding.Default.GetString (Content);
				
				} else if (Action == (byte)ActionType.Get) {
				
					return Content.ToInt ();

				}

				return null;
			
			}
		
		}

	}

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

