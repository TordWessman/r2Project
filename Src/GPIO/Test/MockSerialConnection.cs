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
using System.Threading;

namespace R2Core.GPIO.Tests
{
    public class MockSlaveDevice {
	
		public byte MessageId;
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

				byte[] response = new byte[ArduinoSerialPackageFactory.RESPONSE_POSITION_CONTENT + bytesPerInt * DeviceResponsePackage<byte[]>.NUMBER_OF_RETURN_VALUES];
				response[ArduinoSerialPackageFactory.RESPONSE_POSITION_HOST] = Host;
				response[ArduinoSerialPackageFactory.RESPONSE_POSITION_ACTION] = (byte)SerialActionType.Get;
				response[ArduinoSerialPackageFactory.RESPONSE_POSITION_ID] = Id;
				response[ArduinoSerialPackageFactory.RESPONSE_POSITION_CONTENT_LENGTH] = (byte)(((byte)DeviceResponsePackage<byte[]>.NUMBER_OF_RETURN_VALUES) * bytesPerInt);

				for (int i = 0; i < DeviceResponsePackage<byte[]>.NUMBER_OF_RETURN_VALUES; i++) {
					Array.Copy(IntValues[i].ToBytes(bytesPerInt), 0, response, ArduinoSerialPackageFactory.RESPONSE_POSITION_CONTENT + i * bytesPerInt, bytesPerInt);
				}

				return response;

			}

		}

	}

    /// <summary>
    /// Returns whatever
    /// </summary>
    public class DummySerialConnection : Device.DeviceBase, ISerialConnection {

        public bool ShouldRun { get; private set; } = true;

        public byte[] Response;
        public int Delay = 0;

        public DummySerialConnection() : base("dummy_serial") { }

        public byte[] Read() { return Response; }

        public byte[] Send(byte[] data) {

            if (Delay > 0) { Thread.Sleep(Delay); }
            return Response ?? data;

        }

        public override void Stop() { ShouldRun = false; }

    }


    /// <summary>
    /// Mocks the behaviour of a remote device (such an r2DeviceRouter micro controller.
    /// </summary>
    public class MockSerialConnection : Device.DeviceBase, ISerialConnection {
		
		ISerialPackageFactory m_factory;

		// Response for any Read call.
		public DeviceResponsePackage<int> ReadResponsePackage;

		public List<MockSlaveDevice> Devices = new List<MockSlaveDevice>();

		// The id for the next created device
		public byte CreatedId;

		// Simulate message id:s
		public byte messageId;

		public IList<byte> nodes;

		private readonly object m_lock = new object();

        public bool ShouldRun { get; set; }

        public MockSerialConnection(string id, ISerialPackageFactory factory) : base(id) {
			CreatedId = 0;
			m_factory = factory;
            ShouldRun = true;
            nodes = new List<byte>();
		}

		public MockSlaveDevice GetMockDevice(byte id) {
		
			return Devices.Where(d => d.Id == id).First();
		}

		private byte CalculateChecksum(byte[]response, int stupidSubtracohduthoudoudh = 0) {
		
			byte checksum = 0;
//			//Checksum starts with host
//			for (int i = ArduinoSerialPackageFactory.RESPONSE_POSITION_HOST; i < response.Length; i++) {
//
//				checksum += response[i];
//
//			}

			//Checksum starts with host
			for (int i = ArduinoSerialPackageFactory.RESPONSE_POSITION_HOST; i < ArduinoSerialPackageFactory.RESPONSE_POSITION_CONTENT; i++) {

				checksum += response[i];

			}

			for (int i = ArduinoSerialPackageFactory.RESPONSE_POSITION_CONTENT; i < ArduinoSerialPackageFactory.RESPONSE_POSITION_CONTENT + response[ArduinoSerialPackageFactory.RESPONSE_POSITION_CONTENT_LENGTH] - stupidSubtracohduthoudoudh; i++) {

				checksum += response[i];

			}


			return(byte)(checksum - 0);

		}

		public byte[] Send(byte[] data) {
		
			lock(m_lock) {

				byte host = data[ArduinoSerialPackageFactory.REQUEST_POSITION_HOST];
				byte action = data[ArduinoSerialPackageFactory.REQUEST_POSITION_ACTION];
				byte id = data[ArduinoSerialPackageFactory.REQUEST_POSITION_ID];
				byte contentLength = (byte)(data.Length - ArduinoSerialPackageFactory.REQUEST_POSITION_CONTENT);
				byte[] content = new byte[2];

				if (contentLength > 0) { 
				
					Array.Copy(data, ArduinoSerialPackageFactory.REQUEST_POSITION_CONTENT, content, 0, contentLength);

				}

				byte[] response = new byte[ArduinoSerialPackageFactory.RESPONSE_POSITION_CONTENT + 1 + contentLength];
				response[ArduinoSerialPackageFactory.RESPONSE_POSITION_MESSAGE_ID] = messageId++;
				response[ArduinoSerialPackageFactory.RESPONSE_POSITION_HOST] = host;
				response[ArduinoSerialPackageFactory.RESPONSE_POSITION_ACTION] = action;
				response[ArduinoSerialPackageFactory.RESPONSE_POSITION_ID] = id;
				response[ArduinoSerialPackageFactory.RESPONSE_POSITION_CONTENT_LENGTH] = 2;

				if ((SerialActionType)action == SerialActionType.Get) {

					response = GetMockDevice(id).ToBytes;

				} else if ((SerialActionType)action == SerialActionType.Create) {

					if (!nodes.Contains(host)) {
						nodes.Add(host);
					}

					byte type = content[ArduinoSerialPackageFactory.POSITION_CONTENT_DEVICE_TYPE];
					response[ArduinoSerialPackageFactory.RESPONSE_POSITION_ID] = CreatedId++;
					Devices.Add(MockSlaveDevice.Create(host, type, response[ArduinoSerialPackageFactory.RESPONSE_POSITION_ID], 0));

				} else if ((SerialActionType)action == SerialActionType.Initialization) {

					response[ArduinoSerialPackageFactory.RESPONSE_POSITION_ACTION] = (byte)SerialActionType.InitializationOk;

				} else if ((SerialActionType)action == SerialActionType.IsNodeAvailable) {
				
					response[ArduinoSerialPackageFactory.RESPONSE_POSITION_CONTENT_LENGTH] = 1;
					response[ArduinoSerialPackageFactory.RESPONSE_POSITION_CONTENT] = host == ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL ? (byte)1 : nodes.Contains(host) ? (byte)1 :  (byte)0;

				}

				var len = ArduinoSerialPackageFactory.RESPONSE_POSITION_CONTENT +
				          response[ArduinoSerialPackageFactory.RESPONSE_POSITION_CONTENT_LENGTH];
				
				if (response.Length < len) {

					byte[] dinPappa = new byte[len];

					Array.Copy(response, dinPappa, response.Length);

					response = dinPappa;
				}

				response[ArduinoSerialPackageFactory.RESPONSE_POSITION_CHECKSUM] = CalculateChecksum(response, 0);

				return response;
			
			}

		}

		public byte[] Read() {

			byte[] response = new byte[4 + ReadResponsePackage.Content?.Length ?? 0];
			response[ArduinoSerialPackageFactory.RESPONSE_POSITION_MESSAGE_ID] = messageId++;
			response[ArduinoSerialPackageFactory.RESPONSE_POSITION_HOST] = ReadResponsePackage.NodeId;
			response[ArduinoSerialPackageFactory.RESPONSE_POSITION_ACTION] = (byte)ReadResponsePackage.Action;
			response[ArduinoSerialPackageFactory.RESPONSE_POSITION_ID] = ReadResponsePackage.Id;

			if (ReadResponsePackage.Content != null) {
			
				Array.Copy(ReadResponsePackage.Content, 0, response, 4, ReadResponsePackage.Content.Length);

			}

			return response;

		}
	}
}

