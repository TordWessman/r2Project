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

namespace GPIO
{
	public class ArduinoSerialPackageFactory: ISerialPackageFactory
	{
		// Default local host value. (Not used).
		const byte DEVICE_HOST_LOCAL = 0xFF;

		const int MAX_CONTENT_SIZE = 100;

		readonly byte[] VALID_ANALOGUE_PORTS_ON_ARDUINO = { 14, 15, 16, 17, 18, 19 };

		public DeviceRequestPackage CreateDevice(byte remoteDeviceId, DeviceType type, byte port) {

			// Slave expects <device type><IOPort>.
			byte[] content = { (byte) type , port };

			if (type == DeviceType.AnalogueInput && !(VALID_ANALOGUE_PORTS_ON_ARDUINO.Contains(port))) {

				throw new System.IO.IOException ($"Not a valid analogue port: '{port}'. Use: {string.Concat (VALID_ANALOGUE_PORTS_ON_ARDUINO.Select (b => b.ToString () + ' '))}");

			}

			DeviceRequestPackage package = new DeviceRequestPackage () { 
				Host = DEVICE_HOST_LOCAL, 
				Action = (byte) ActionType.Create, 
				Id = remoteDeviceId, 
				Content = content
			};

			return package;

		}

		public DeviceRequestPackage SetDevice(byte remoteDeviceId, int value) {

			byte[] content = { (byte) (value & 0xFF) , (byte) ((value >> 8) & 0xFF) };

			return new DeviceRequestPackage () { 
				Host = DEVICE_HOST_LOCAL, 
				Action = (byte) ActionType.Set, 
				Id = remoteDeviceId, 
				Content = content 
			};

		}

		public DeviceRequestPackage GetDevice(byte remoteDeviceId) {

			return new DeviceRequestPackage () { 
				Host = DEVICE_HOST_LOCAL, 
				Action = (byte) ActionType.Get, 
				Id = remoteDeviceId, 
				Content = {} 
			};

		}
	}
}

