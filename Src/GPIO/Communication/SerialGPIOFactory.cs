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
using Core.Device;
using Core;
using System.Collections.Generic;
using System.Linq;

namespace GPIO
{
	/// <summary>
	/// Represents a device available through the serial bus (i.e. I2C or Serial)
	/// </summary>
	public interface ISerialDevice: IDevice {

		/// <summary>
		/// (Re)creates the device representation on the node, typically after slave reboot.
		/// </summary>
		void Synchronize ();

		/// <summary>
		/// Allows the device to retrieve it's value from the node. 
		/// </summary>
		void Update();

		/// <summary>
		/// The node to which this device belongs
		/// </summary>
		/// <value>The node identifier.</value>
		byte NodeId { get; }

		/// <summary>
		/// If the node to which this device is attached is in sleep mode.
		/// </summary>
		/// <value><c>true</c> if this instance is sleeping; otherwise, <c>false</c>.</value>
		bool IsSleeping { get; set; }

	}

	public class SerialGPIOFactory: DeviceBase
	{

		// Interface to the host(s)
		private ISerialHost m_connection;

		// Keeps track of all devices per host
		private SerialDeviceManager m_devices;

		public SerialGPIOFactory (string id, ISerialHost connection) : base (id) {

			m_connection = connection;
			m_devices = new SerialDeviceManager (connection);
			m_connection.HostDidReset = m_devices.NodeDidReset;

		}

		/// <summary>
		/// Return a node with the specified id
		/// </summary>
		/// <returns>The node.</returns>
		/// <param name="nodeId">Node identifier.</param>
		public ISerialNode GetNode(int nodeId) { return m_devices.Nodes.Where (node => (int) node.NodeId == nodeId).FirstOrDefault (); } 
		public ISerialNode this[int key] { get { return GetNode (key); } }
			
		//public IDictionary<byte, ISerialNode> Nodes { get { return m_devices.Nodes; } }

		public override void Start() { 
		
			if (Ready) { throw new ApplicationException ("Unable to start. We are connected!"); } 
		
			m_connection.Start ();

		}

		public override void Stop() { 

			if (!Ready) { Log.w ("Will not stop ISerialHost. Not connected!"); } 
			else { m_connection.Start (); } 

		}

		public override bool Ready { get { return m_connection.Ready; } }

		public IInputMeter<double> CreateAnalogInput (string id, int port, int nodeId = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {

			var device = new SerialAnalogInput (id, (byte)nodeId, m_connection, port); 
			m_devices.Add (device);
			return device; 

		}

		public IInputPort CreateInputPort (string id, int port, int nodeId = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {
		
			var device = new SerialDigitalInput (id, (byte)nodeId, m_connection, port); 
			m_devices.Add (device);
			return device; 

		}


		public IOutputPort CreateOutputPort (string id, int port, int nodeId = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {

			var device = new SerialDigitalOutput (id, (byte)nodeId, m_connection, port); 
			m_devices.Add (device);
			return device; 

		}

		public IServo CreateServo (string id, int port, int nodeId = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {

			var device = new SerialServo (id, (byte)nodeId, m_connection, port); 
			m_devices.Add (device);
			return device; 

		}

		public IInputMeter<int> CreateSonar (string id, int triggerPort, int echoPort, int nodeId = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {

			var device = new SerialHCSR04Sonar (id, (byte)nodeId, m_connection, triggerPort, echoPort); 
			m_devices.Add (device);
			return device; 

		}

		public IDHT11 CreateDht11 (string id, int port, int nodeId = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {

			var device = new SerialDHT11(id, (byte)nodeId, m_connection, port); 
			m_devices.Add (device);
			return device; 

		}

	}

}