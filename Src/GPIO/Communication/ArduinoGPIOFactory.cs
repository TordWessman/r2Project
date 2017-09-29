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

namespace GPIO
{
	public class ArduinoGPIOFactory: DeviceBase
	{
		private ISerialConnection m_connection;
		private ISerialPackageFactory m_packageFactory;
		private byte m_deviceCount;
		private const string SERIAL_CONNECTOR_ID_POSTFIX = "_serial_connector";
		private const int DEFAULT_BAUD_RATE = 9600;

		// port identifier for ISerialConnection
		private string m_portIdentifier;

		public ArduinoGPIOFactory (string id, ISerialConnection connection) : base (id) {

			m_connection = connection;
			m_packageFactory = new ArduinoSerialPackageFactory ();
			m_deviceCount = 0;

		}

		public ArduinoGPIOFactory (string id, string portIdentifier, int baudRate = DEFAULT_BAUD_RATE) : base (id) {

			m_portIdentifier = portIdentifier;
			m_packageFactory = new ArduinoSerialPackageFactory ();
			m_deviceCount = 0;

		}

		public override void Start () {

			if (m_connection == null) {
			
				m_connection = new ArduinoSerialConnector(Identifier + SERIAL_CONNECTOR_ID_POSTFIX, m_portIdentifier);

			}

			if (!m_connection?.Ready == true) {
		
				m_connection.Start ();

			}
		
		}

		public override void Stop () {

			if (m_connection?.Ready == true) {

				m_connection.Stop ();

			}

		}

		public override bool Ready { get { return m_connection.Ready; } }

		/// <summary>
		/// Sends a create request to slave and return the slaves device id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="adcPort">Adc port.</param>
		/// <param name="type">Type.</param>
		private byte Create(string id, DeviceType type, params byte[] ports) {

			if (!Ready) {

				throw new System.IO.IOException ("Communication not started.");
			}

			DeviceResponsePackage response = new DeviceResponsePackage (m_connection.Send (m_packageFactory.CreateDevice (m_deviceCount++, type, ports).ToBytes ()));

			if (response.IsError) {

				throw new System.IO.IOException ($"Unable to create device: {response.Value}");
			}

			return response.Id;
		}

		public IInputMeter<double> CreateAnalogInput (string id, int port) {

			return new SerialAnalogInput (id, Create(id, DeviceType.AnalogueInput, (byte)port), m_connection, m_packageFactory);

		}

		public IInputPort CreateInputPort (string id, int port) {
		
			return new SerialDigitalInput (id, Create(id, DeviceType.DigitalInput, (byte)port), m_connection, m_packageFactory);
		}


		public IOutputPort CreateOutputPort (string id, int port) {
			
			return new SerialDigitalOutput (id, Create(id, DeviceType.DigitalOutput, (byte)port), m_connection, m_packageFactory);

		}

		public IServo CreateServo (string id, int port) {
			
			return new SerialServo (id, Create(id, DeviceType.Servo, (byte)port), m_connection, m_packageFactory);

		}

	}

}