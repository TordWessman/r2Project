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
		// Default local host value. This is the default host used. Data sent to other hosts requires a RH24 enabled slave (configured as RH24 master).
		public const byte DEVICE_HOST_LOCAL = 0x0;

		// Max size for content in packages
		const int MAX_CONTENT_SIZE = 100;

		// ATMEGA 328-specific constraints. 18 & 19 will not be available if running I2C, since they are used as SDA and SCL on the ATMEGA 328 board.
		public readonly byte[] VALID_ANALOGUE_PORTS_ON_ARDUINO = { 14, 15, 16, 17, 18, 19 };

		/// Keeps track of the devcie number for each host. This values does not correspond to the id's of the host.
		private byte[] m_deviceCount;

		/// <summary>
		/// Defines the maximum number of devices possible to allocate on each host
		/// </summary>
		private const byte MAXIMUM_NUMBER_OF_DEVICES_PER_HOST = 20;

		public const int POSITION_HOST = 0;
		public const int POSITION_ACTION = 1;
		public const int POSITION_ID = 2;
		public const int POSITION_CONTENT_LENGTH = 3;
		public const int POSITION_CONTENT = 4;

		// In the content part of the (create) message, here be the Type
		public const int POSITION_CONTENT_DEVICE_TYPE = 0;

		public ArduinoSerialPackageFactory() {

			m_deviceCount = new byte[sizeof(byte) * 256];

		}

		public DeviceResponsePackage ParseResponse (byte[] response) {

			int contentLength = response [POSITION_CONTENT_LENGTH];

			return new DeviceResponsePackage () {
				Host = response [POSITION_HOST],
				Action = (ActionType)response [POSITION_ACTION],
				Id = response [POSITION_ID],
				Content = contentLength > 0 ? response.Skip (POSITION_CONTENT).Take (contentLength)?.ToArray () ?? new byte[]{ } : new byte[]{ }
			};

		}

		public DeviceRequestPackage CreateDevice(byte hostId, SerialDeviceType type, byte[] ports) {

			// Slave expects <device type><IOPort1><IOPort2> ...
			byte[] content = new byte[1 + ports.Length];
			content [POSITION_CONTENT_DEVICE_TYPE] = (byte)type;
			Array.Copy (ports, 0, content, 1, ports.Length);

			if (type == SerialDeviceType.AnalogueInput && !(VALID_ANALOGUE_PORTS_ON_ARDUINO.Contains(ports[0]))) {

				throw new System.IO.IOException ($"Not a valid analogue port: '{ports[0]}'. Use: {string.Concat (VALID_ANALOGUE_PORTS_ON_ARDUINO.Select (b => b.ToString () + ' '))}");

			}

			DeviceRequestPackage package = new DeviceRequestPackage () { 
				Host = hostId, 
				Action = (byte) ActionType.Create, 
				Id = m_deviceCount[hostId]++, // Id's for devices are normally managed by the slave, so this value will normally be ignored.
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

		public DeviceRequestPackage SetNodeId (byte host) {

			return new DeviceRequestPackage () { 
				Host = 0x0, 
				Action = (byte) ActionType.Initialization, 
				Id = host, 
				Content = {} 
			};

		}
	}
}

