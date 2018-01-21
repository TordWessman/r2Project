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
	public interface ISerialDevice {

		/// <summary>
		/// Allows us to update the slave id of the device, typically after slave reboot.
		/// </summary>
		/// <param name="newSlaveId">New slave identifier.</param>
		void Synchronize ();

	}

	public class SerialGPIOFactory: DeviceBase
	{

		// Interface to the host(s)
		private ISerialHost m_connection;

		// Keeps track of all devices per host
		private IDictionary<byte, List<ISerialDevice>> m_devices;

		public SerialGPIOFactory (string id, ISerialHost connection) : base (id) {

			m_connection = connection;
			m_devices = new Dictionary<byte, List<ISerialDevice>> ();
			m_connection.HostDidReset = HostDidReset;

		}

		/// <summary>
		/// Called whenever the slave informs that it did reset and that it needs to be reinitialized.
		/// </summary>
		/// <param name="hostId">Host identifier.</param>
		private void HostDidReset(byte hostId) {

			if (m_devices.ContainsKey (hostId)) {
			
				// Resynchronize each device
				m_devices [hostId].ForEach (device => device.Synchronize ());

			}

		}

		public override void Start() { 
		
			if (Ready) { throw new ApplicationException ("Unable to start. We are connected!"); } 
		
			m_connection.Start ();

		}

		public override void Stop() { 

			if (!Ready) { Log.w ("Will not stop. We are not connected!"); } 
			else { m_connection.Start (); } 

		}

		public override bool Ready { get { return m_connection.Ready; } }

		/// <summary>
		/// Synchronizes the device to the host and add it for re-synchronization purposes.
		/// </summary>
		/// <param name="hostId">Host identifier.</param>
		/// <param name="device">Device.</param>
		private void SynchronizeDevice(byte hostId, ISerialDevice device) {
		
			if (!m_devices.ContainsKey (hostId)) { m_devices.Add(hostId, new List<ISerialDevice>()); }
			m_devices [hostId].Add (device);

			// Synchronize the device with the slave host
			device.Synchronize ();

		}

		public IInputMeter<double> CreateAnalogInput (string id, int port, int hostId = ArduinoSerialPackageFactory.DEVICE_HOST_LOCAL) {

			var device = new SerialAnalogInput (id, (byte)hostId, m_connection, port); 
			SynchronizeDevice ((byte) hostId, device);
			return device; 

		}

		public IInputPort CreateInputPort (string id, int port, int hostId = ArduinoSerialPackageFactory.DEVICE_HOST_LOCAL) {
		
			var device = new SerialDigitalInput (id, (byte)hostId, m_connection, port); 
			SynchronizeDevice ((byte) hostId, device);
			return device; 

		}


		public IOutputPort CreateOutputPort (string id, int port, int hostId = ArduinoSerialPackageFactory.DEVICE_HOST_LOCAL) {

			var device = new SerialDigitalOutput (id, (byte)hostId, m_connection, port); 
			SynchronizeDevice ((byte) hostId, device);
			return device; 

		}

		public IServo CreateServo (string id, int port, int hostId = ArduinoSerialPackageFactory.DEVICE_HOST_LOCAL) {

			var device = new SerialServo (id, (byte)hostId, m_connection, port); 
			SynchronizeDevice ((byte) hostId, device);
			return device; 

		}

		public IInputMeter<int> CreateSonar (string id, int triggerPort, int echoPort, int hostId = ArduinoSerialPackageFactory.DEVICE_HOST_LOCAL) {

			var device = new SerialHCSR04Sonar (id, (byte)hostId, m_connection, triggerPort, echoPort); 
			SynchronizeDevice ((byte) hostId, device);
			return device; 

		}

		public IDHT11 CreateDht11 (string id, int port, int hostId = ArduinoSerialPackageFactory.DEVICE_HOST_LOCAL) {

			var device = new SerialDHT11(id, (byte)hostId, m_connection, port); 
			SynchronizeDevice ((byte) hostId, device);
			return device; 

		}

	}

}