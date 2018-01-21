﻿// This file is part of r2Poject.
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
		public const byte DEVICE_HOST_LOCAL = 0xFF;

		const int MAX_CONTENT_SIZE = 100;

		// ATMEGA 328-specific constraints. 18 & 19 will not be available if running I2C, since they are used as SDA and SCL on the ATMEGA 328 board.
		public readonly byte[] VALID_ANALOGUE_PORTS_ON_ARDUINO = { 14, 15, 16, 17, 18, 19 };

		/// <summary>
		/// Keeps track of the devcie number for each host.
		/// </summary>
		private byte[] m_deviceCount;

		/// <summary>
		/// Defines the maximum number of devices possible to allocate on each host
		/// </summary>
		private const byte MAXIMUM_NUMBER_OF_DEVICES_PER_HOST = 20;

		public ArduinoSerialPackageFactory() {

			m_deviceCount = new byte[sizeof(byte) * 256];

		}

		public DeviceRequestPackage CreateDevice(byte hostId, SerialDeviceType type, byte[] ports) {

			// Slave expects <device type><IOPort1><IOPort2> ...
			byte[] content = new byte[1 + ports.Length];
			content [0] = (byte)type;
			Array.Copy (ports, 0, content, 1, ports.Length);

			if (type == SerialDeviceType.AnalogueInput && !(VALID_ANALOGUE_PORTS_ON_ARDUINO.Contains(ports[0]))) {

				throw new System.IO.IOException ($"Not a valid analogue port: '{ports[0]}'. Use: {string.Concat (VALID_ANALOGUE_PORTS_ON_ARDUINO.Select (b => b.ToString () + ' '))}");

			}

			DeviceRequestPackage package = new DeviceRequestPackage () { 
				Host = hostId, 
				Action = (byte) ActionType.Create, 
				Id = m_deviceCount[hostId]++,
				Content = content
			};

			return package;

		}

		public DeviceRequestPackage SetDevice(byte deviceId, byte hostId, int value) {

			byte[] content = { (byte) (value & 0xFF) , (byte) ((value >> 8) & 0xFF) };

			return new DeviceRequestPackage () { 
				Host = hostId, 
				Action = (byte) ActionType.Set, 
				Id = deviceId, 
				Content = content 
			};

		}

		public DeviceRequestPackage GetDevice(byte deviceId, byte hostId) {

			return new DeviceRequestPackage () { 
				Host = hostId, 
				Action = (byte) ActionType.Get, 
				Id = deviceId, 
				Content = {} 
			};

		}

		public DeviceRequestPackage Initialize (byte host) {

			return new DeviceRequestPackage () { 
				Host = host, 
				Action = (byte) ActionType.Initialization, 
				Id = 0x0, 
				Content = {} 
			};

		}
	}
}

