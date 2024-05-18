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
using System.Linq;

namespace R2Core.GPIO
{
	/// <summary>
	/// Handles serialization & deserialization of the packages communicated to the Arduino r2I2CDeviceRouter 
	/// </summary>
	public class ArduinoSerialPackageFactory : ISerialPackageFactory {
		
		/// <summary>
		/// Default local nodeId value. This is the default nodeId used. Data sent to other nodeIds requires a RH24 enabled node(configured as RH24 master). 
		/// </summary>
		public const byte DEVICE_NODE_LOCAL = 0x0;

		/// <summary>
		/// Max size for content in packages
		/// </summary>
		const int MAX_CONTENT_SIZE = 10;

		/// <summary>
		/// Keeps track of the devcie number for each host. This values does not correspond to the id's of the host.
		/// </summary>
		private byte[] m_deviceCount;

		/// <summary>
		/// Defines the maximum number of devices possible to allocate on each host
		/// </summary>
		private const byte MAXIMUM_NUMBER_OF_DEVICES_PER_HOST = 5;

		/// <summary>
		/// During SendToSleep, this amount of cycles forces the node to sleep until woken up by tranciever.
		/// </summary>
		public const int RH24_SLEEP_UNTIL_MESSAGE_RECEIVED = 0xFF;

		/// <summary>
		/// The maximum interval allowed for the PauseSleep action.
		/// </summary>
		public const int RH24_MAXIMUM_PAUSE_SLEEP_INTERVAL = 60;

		// Below are the positions for Response messages. Request messages do no have the message id, and is thus 1 byte `lower`.
		public const int RESPONSE_POSITION_CHECKSUM = 0x0;
		public const int RESPONSE_POSITION_MESSAGE_ID = 0x1;
		public const int RESPONSE_POSITION_HOST = 0x2;
		public const int RESPONSE_POSITION_ACTION = 0x3;
		public const int RESPONSE_POSITION_ID = 0x4;
		public const int RESPONSE_POSITION_CONTENT_LENGTH = 0x5;
		public const int RESPONSE_POSITION_CONTENT = 0x6;

		// Request package positions:
		public const int REQUEST_POSITION_CHECKSUM = 0x0;
		public const int REQUEST_POSITION_HOST = 0x1;
		public const int REQUEST_POSITION_ACTION = 0x2;
		public const int REQUEST_POSITION_ID = 0x3;
		public const int REQUEST_POSITION_CONTENT_LENGTH = 0x4;
		public const int REQUEST_POSITION_CONTENT = 0x5;

		/// <summary>
		/// In the content part of the(create) message, here be the Type
		/// </summary>
		public const int POSITION_CONTENT_DEVICE_TYPE = 0x0;

		/// <summary>
		/// Where the the error code resides in the response content
		/// </summary>
		public const int POSITION_CONTENT_POSITION_ERROR_TYPE = 0x0;

		/// <summary>
		/// Where(potentional) additional error information resides int response content.
		/// </summary>
		public const int POSITION_CONTENT_POSITION_ERROR_INFO = 0x1;

		/// <summary>
		/// Content positions for Sleep data
		/// </summary>
		public const int POSITION_CONTENT_SLEEP_TOGGLE = 0x0;
		public const int POSITION_CONTENT_SLEEP_CYCLES = 0x1;

		private const int MAX_CONTENT_LENGTH = 0xFF;

		public ArduinoSerialPackageFactory() {

			m_deviceCount = new byte[sizeof(byte)* 256];

		}

		public byte[] SerializeRequest(DeviceRequestPackage request) {
		
			int contentLength = (request.Content?.Length ?? 0);

			if (contentLength > MAX_CONTENT_LENGTH) {
				
				throw new ArgumentOutOfRangeException($"Invalid content length: {contentLength}. Max contentLength: {MAX_CONTENT_LENGTH}");
			
			}

			byte[] requestData = new byte[REQUEST_POSITION_CONTENT + contentLength];

			requestData[REQUEST_POSITION_HOST] = request.NodeId;
			requestData[REQUEST_POSITION_ACTION] = (byte)request.Action;
			requestData[REQUEST_POSITION_ID] = request.Id;
			requestData[REQUEST_POSITION_CONTENT_LENGTH] = (byte)contentLength;

			if (contentLength > 0) {
			
				Array.Copy(request.Content, 0, requestData, REQUEST_POSITION_CONTENT, contentLength);

			}

			// Add checksum
			for (int i = 1; i < requestData.Length; i++) {

				requestData[REQUEST_POSITION_CHECKSUM] += requestData[i];

			}

			return requestData;

		}

		public DeviceResponsePackage<T> ParseResponse<T>(byte[] response) {

			if (response.Length < RESPONSE_POSITION_CONTENT_LENGTH) {
			
				return new DeviceResponsePackage<T> {
					Checksum = 0,
					Action = SerialActionType.Error,
					Content = new byte[] {(byte)SerialErrorType.ERROR_INVALID_RESPONSE_SIZE}
				};

			}

			int contentLength = response[RESPONSE_POSITION_CONTENT_LENGTH];

			return new DeviceResponsePackage<T> {
				Checksum = response[RESPONSE_POSITION_CHECKSUM],
				MessageId = response[RESPONSE_POSITION_MESSAGE_ID],
				NodeId = response[RESPONSE_POSITION_HOST],
				Action = (SerialActionType)response[RESPONSE_POSITION_ACTION],
				Id = response[RESPONSE_POSITION_ID],
				Content = contentLength > 0 ? response.Skip(RESPONSE_POSITION_CONTENT).Take(contentLength)?.ToArray() ?? new byte[]{ } : new byte[]{ }
			};

		}

		public DeviceRequestPackage CreateDevice(byte nodeId, SerialDeviceType type, byte[] parameters) {

			// Node expects <device type><IOPort1><IOPort2> ...
			byte[] content = new byte[1 + parameters.Length];
			content[POSITION_CONTENT_DEVICE_TYPE] = (byte)type;
			Array.Copy(parameters, 0, content, 1, parameters.Length);

			DeviceRequestPackage package = new DeviceRequestPackage { 
				NodeId = nodeId, 
				Action = SerialActionType.Create, 
				Id = m_deviceCount[nodeId]++, // Actually decided by the node...
				Content = content
			};

			return package;

		}

        public DeviceRequestPackage DeleteDevice(byte deviceId, byte nodeId) {

            m_deviceCount[nodeId]--;

            DeviceRequestPackage package = new DeviceRequestPackage {
                NodeId = nodeId,
                Action = SerialActionType.DeleteDevice,
                Id = deviceId,
                Content = { }
            };

            return package;

        }

        public DeviceRequestPackage SetDevice(byte deviceId, byte nodeId, int value) {

			byte[] content = { (byte)(value & 0xFF) , (byte)((value >> 8) & 0xFF) };

			return new DeviceRequestPackage { 
				NodeId = nodeId, 
				Action = SerialActionType.Set, 
				Id = deviceId, 
				Content = content 
			};

		}

        public DeviceRequestPackage GetDevice(byte deviceId, byte nodeId, byte[] parameters = null) {

			return new DeviceRequestPackage { 
				NodeId = nodeId, 
				Action = SerialActionType.Get, 
				Id = deviceId, 
				Content = (parameters ?? new byte[0])
            };

		}

		public DeviceRequestPackage Sleep(byte nodeId, bool toggle, byte cycles) {
		
			byte[] content = new byte[2];
			content[POSITION_CONTENT_SLEEP_TOGGLE] = (byte)(toggle ? 1 : 0);
			content[POSITION_CONTENT_SLEEP_CYCLES] = cycles;

			return new DeviceRequestPackage {
				NodeId = nodeId,
				Action = SerialActionType.SendToSleep, 
				Content = content
			};
		}

		public DeviceRequestPackage SetNodeId(byte nodeId) {

			return new DeviceRequestPackage {
				NodeId = 0x0, 
				Action = SerialActionType.Initialization, 
				Id = nodeId, 
				Content = {} 
			};

		}

	}

}
