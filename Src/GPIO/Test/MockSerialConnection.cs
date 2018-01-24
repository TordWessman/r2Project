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
//
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Data;

namespace GPIO.Tests
{
	public class MockSlaveDevice {
	
		public byte Host;
		public byte Type;
		public byte Id;
		public int[] IntValues;

		public static MockSlaveDevice Create(byte host, byte type, byte id, int value) {
		
			return new MockSlaveDevice() {
				Host = host,
				Type = (byte)type,
				Id = id,
				IntValues = new int[]{ value, 0 }
			};

		}

		public byte[] ToBytes {
		
			get {

				byte bytesPerInt = 2;

				byte[] response = new byte[ArduinoSerialPackageFactory.POSITION_CONTENT + bytesPerInt * DeviceResponsePackage.NUMBER_OF_RETURN_VALUES];
				response [ArduinoSerialPackageFactory.POSITION_HOST] = Host;
				response [ArduinoSerialPackageFactory.POSITION_ACTION] = (byte)ActionType.Get;
				response [ArduinoSerialPackageFactory.POSITION_ID] = Id;
				response [ArduinoSerialPackageFactory.POSITION_CONTENT_LENGTH] = (byte)(((byte) DeviceResponsePackage.NUMBER_OF_RETURN_VALUES) * bytesPerInt);

				for (int i = 0; i < DeviceResponsePackage.NUMBER_OF_RETURN_VALUES; i++) {
					Array.Copy (IntValues [i].ToBytes (bytesPerInt), 0, response, ArduinoSerialPackageFactory.POSITION_CONTENT + i * bytesPerInt, bytesPerInt);
				}

				return response;

			}

		}

	}

	public class MockSerialConnection: Core.Device.DeviceBase, ISerialConnection
	{
		ISerialPackageFactory m_factory;

		// Response for any Read call.
		public DeviceResponsePackage ReadResponsePackage;

		public List<MockSlaveDevice> Devices = new List<MockSlaveDevice>();

		// The id for the next created device
		public byte CreatedId;

		private readonly object m_lock = new object (); 
		public MockSerialConnection (string id, ISerialPackageFactory factory) : base (id)
		{
			CreatedId = 0;
			m_factory = factory;

		}

		public MockSlaveDevice GetMockDevice(byte id) {
		
			return Devices.Where (d => d.Id == id).First ();
		}

		public byte [] Send(byte []data) {
		
			lock (m_lock) {
			
				byte host = data [ArduinoSerialPackageFactory.POSITION_HOST];
				byte action = data [ArduinoSerialPackageFactory.POSITION_ACTION];
				byte id = data [ArduinoSerialPackageFactory.POSITION_ID];
				byte[] content = new byte[data.Length - 3];

				if ((ActionType)action == ActionType.Get) {

					byte[] response = GetMockDevice (id).ToBytes;
					return response;

				}  else if ((ActionType)action == ActionType.Create) {

					byte type = content [ArduinoSerialPackageFactory.POSITION_CONTENT_DEVICE_TYPE];
					data [ArduinoSerialPackageFactory.POSITION_ID] = CreatedId++;
					Devices.Add(MockSlaveDevice.Create(host, type, data [ArduinoSerialPackageFactory.POSITION_ID], 0));
					return data;

				} else if ((ActionType)action == ActionType.Initialization || (ActionType)action == ActionType.Set) {

					return data;

				}

				throw new NotImplementedException();
			
			}

		}
			
		public byte [] Read() {

			byte[] response = new byte[3 + ReadResponsePackage.Content?.Length ?? 0];
			response [ArduinoSerialPackageFactory.POSITION_HOST] = ReadResponsePackage.Host;
			response [ArduinoSerialPackageFactory.POSITION_ACTION] = (byte) ReadResponsePackage.Action;
			response [ArduinoSerialPackageFactory.POSITION_ID] = ReadResponsePackage.Id;

			if (ReadResponsePackage.Content != null) {
			
				Array.Copy (ReadResponsePackage.Content, 0, response, 3, ReadResponsePackage.Content.Length);

			}

			return response;

		}
	}
}

