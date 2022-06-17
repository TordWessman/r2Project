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
using R2Core.Device;
using System.Linq;

namespace R2Core.GPIO
{
    /// <summary>
    /// GPIO factory that requires an Arduino running i2CDeviceRouter.
    /// The Arduino is used as a hardware interface.
    /// </summary>
	public class SerialGPIOFactory : DeviceBase {

		// Interface to the host(s)
		private IArduinoDeviceRouter m_connection;

		// Keeps track of all devices per host
		private SerialDeviceManager m_devices;

		public SerialGPIOFactory(string id, IArduinoDeviceRouter connection) : base(id) {

			m_connection = connection;
			m_devices = new SerialDeviceManager(connection);

		}

		/// <summary>
		/// Return a node with the specified id
		/// </summary>
		/// <returns>The node.</returns>
		/// <param name="nodeId">Node identifier.</param>
		public ISerialNode GetNode(int nodeId) { return m_devices.Nodes.FirstOrDefault(node => node.NodeId == nodeId); } 
		public ISerialNode this[int key] { get { return GetNode(key); } }
			
		//public IDictionary<byte, ISerialNode> Nodes { get { return m_devices.Nodes; } }

		public override void Start() { 
		
			if (Ready) { throw new ApplicationException("Unable to start. We are connected!"); } 
		
			m_connection.Start();

		}

		public override void Stop() { 

			if (!Ready) { Log.w("Will not stop ISerialHost. Not connected!"); } 
			else { m_connection.Stop(); } 

		}

		public override bool Ready { get { return m_connection.Ready; } }

		public IInputMeter<double> CreateAnalogInput(string id, int port, int nodeId = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {

			return InstantiateDevice(new SerialAnalogInput(id, m_devices.GetNode(nodeId), m_connection, new int[1] {port})); 


		}

		public IInputMeter<double> CreateMoisture(string id, int analogueInput, int digitalOutput, int nodeId = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {

			return InstantiateDevice(new SimpleAnalogueHumiditySensor(id, m_devices.GetNode(nodeId), m_connection, new int[2] {analogueInput, digitalOutput})); 

		}

		public IInputPort CreateInputPort(string id, int port, int nodeId = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {
		
			return InstantiateDevice(new SerialDigitalInput(id, m_devices.GetNode(nodeId), m_connection, port)); 

		}


		public IOutputPort<bool> CreateOutputPort(string id, int port, int nodeId = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {

			return InstantiateDevice(new SerialDigitalOutput(id, m_devices.GetNode(nodeId), m_connection, port)); 

		}

		public IServo CreateServo(string id, int port, int nodeId = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {

			return InstantiateDevice(new SerialServo(id, m_devices.GetNode(nodeId), m_connection, port));

        }

        public IInputMeter<int> CreateHCSR04Sonar(string id, int triggerPort, int echoPort, int nodeId = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {

			return InstantiateDevice(new SerialHCSR04Sonar(id, m_devices.GetNode(nodeId), m_connection, triggerPort, echoPort)); 

        }

        public IInputMeter<int> CreateSonar(string id, int triggerPort, int echoPort, int nodeId = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {

            return InstantiateDevice(new SerialSonar(id, m_devices.GetNode(nodeId), m_connection, triggerPort, echoPort));

        }


        public IDHT11 CreateDht11(string id, int port, int nodeId = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {

			return InstantiateDevice(new SerialDHT11(id, m_devices.GetNode(nodeId), m_connection, port)); 

		}

        public IOutputPort<ushort> CreateAnalogOutput(string id, int port, int nodeId = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {

            return InstantiateDevice(new SerialAnalogOutput(id, m_devices.GetNode(nodeId), m_connection, port));

        }

        /// <summary>
        /// Synchronizes(remotely creates) the device.
        /// </summary>
        /// <returns>The device.</returns>
        /// <param name="device">Device.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        private T InstantiateDevice<T>(T device) where T: ISerialDevice {

			device.Synchronize();
			return device;

		}

	}

}