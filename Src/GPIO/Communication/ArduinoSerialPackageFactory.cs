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
		// Default local nodeId value. This is the default nodeId used. Data sent to other nodeIds requires a RH24 enabled slave (configured as RH24 master).
		public const byte DEVICE_NODE_LOCAL = 0x0;

		// Max size for content in packages
		const int MAX_CONTENT_SIZE = 10;

		// ATMEGA 328-specific constraints. 18 & 19 will not be available if running I2C, since they are used as SDA and SCL on the ATMEGA 328 board.
		public readonly byte[] VALID_ANALOGUE_PORTS_ON_ARDUINO = { 14, 15, 16, 17, 18, 19 };

		/// Keeps track of the devcie number for each host. This values does not correspond to the id's of the host.
		private byte[] m_deviceCount;

		/// <summary>
		/// Defines the maximum number of devices possible to allocate on each host
		/// </summary>
		private const byte MAXIMUM_NUMBER_OF_DEVICES_PER_HOST = 5;

		// During SendToSleep, this amount of cycles forces the node to sleep until woken up by tranciever
		public const int RH24_SLEEP_UNTIL_MESSAGE_RECEIVED = 0xFF;

		public const int POSITION_MESSAGE_ID = 0x0;
		public const int POSITION_HOST = 0x1;
		public const int POSITION_ACTION = 0x2;
		public const int POSITION_ID = 0x3;
		public const int POSITION_CONTENT_LENGTH = 0x4;
		public const int POSITION_CONTENT = 0x5;

		// In the content part of the (create) message, here be the Type
		public const int POSITION_CONTENT_DEVICE_TYPE = 0x0;

		// Where the the error code resides in the response content
		public const int POSITION_CONTENT_POSITION_ERROR_TYPE = 0x0;
		// Where (potentional) additional error information resides int response content.
		public const int POSITION_CONTENT_POSITION_ERROR_INFO = 0x1;

		// Content positions for Sleep data
		public const int POSITION_CONTENT_SLEEP_TOGGLE = 0x0;
		public const int POSITION_CONTENT_SLEEP_CYCLES = 0x1;

		public ArduinoSerialPackageFactory() {

			m_deviceCount = new byte[sizeof(byte) * 256];

		}

		public DeviceResponsePackage<T> ParseResponse<T> (byte[] response) {

			int contentLength = response [POSITION_CONTENT_LENGTH];

			return new DeviceResponsePackage<T> () {
				MessageId = response [POSITION_MESSAGE_ID],
				NodeId = response [POSITION_HOST],
				Action = (ActionType)response [POSITION_ACTION],
				Id = response [POSITION_ID],
				Content = contentLength > 0 ? response.Skip (POSITION_CONTENT).Take (contentLength)?.ToArray () ?? new byte[]{ } : new byte[]{ }
			};

		}

		public DeviceRequestPackage CreateDevice(byte nodeId, SerialDeviceType type, byte[] ports) {

			// Slave expects <device type><IOPort1><IOPort2> ...
			byte[] content = new byte[1 + ports.Length];
			content [POSITION_CONTENT_DEVICE_TYPE] = (byte)type;
			Array.Copy (ports, 0, content, 1, ports.Length);

			if (type == SerialDeviceType.AnalogueInput && !(VALID_ANALOGUE_PORTS_ON_ARDUINO.Contains(ports[0]))) {

				throw new System.IO.IOException ($"Not a valid analogue port: '{ports[0]}'. Use: {string.Concat (VALID_ANALOGUE_PORTS_ON_ARDUINO.Select (b => b.ToString () + ' '))}");

			}

			DeviceRequestPackage package = new DeviceRequestPackage () { 
				NodeId = nodeId, 
				Action = (byte) ActionType.Create, 
				Id = m_deviceCount[nodeId]++,
				Content = content
			};

			return package;

		}

		public DeviceRequestPackage SetDevice(byte deviceId, byte nodeId, int value) {

			byte[] content = { (byte) (value & 0xFF) , (byte) ((value >> 8) & 0xFF) };

			return new DeviceRequestPackage () { 
				NodeId = nodeId, 
				Action = (byte) ActionType.Set, 
				Id = deviceId, 
				Content = content 
			};

		}

		public DeviceRequestPackage GetDevice(byte deviceId, byte nodeId) {

			return new DeviceRequestPackage () { 
				NodeId = nodeId, 
				Action = (byte) ActionType.Get, 
				Id = deviceId, 
				Content = {} 
			};

		}

		public DeviceRequestPackage Sleep(byte nodeId, bool toggle, byte cycles) {
		
			byte[] content = new byte[2];
			content [POSITION_CONTENT_SLEEP_TOGGLE] = (byte)(toggle ? 1 : 0);
			content [POSITION_CONTENT_SLEEP_CYCLES] = (byte) cycles;

			return new DeviceRequestPackage () {
				NodeId = nodeId,
				Action = (byte)ActionType.SendToSleep, 
				Content = content
			};
		}

		public DeviceRequestPackage SetNodeId (byte nodeId) {

			return new DeviceRequestPackage () {
				NodeId = 0x0, 
				Action = (byte) ActionType.Initialization, 
				Id = nodeId, 
				Content = {} 
			};

		}

	}
}

