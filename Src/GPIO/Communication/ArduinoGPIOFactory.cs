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

		public ArduinoGPIOFactory (string id, ISerialConnection connection) : base (id) {

			m_connection = connection;
			m_packageFactory = new ArduinoSerialPackageFactory ();
			m_deviceCount = 0;

		}

		public ArduinoGPIOFactory (string id, string portIdentifier, int baudRate = DEFAULT_BAUD_RATE) : base (id) {

			m_connection = new ArduinoSerialConnector(id + SERIAL_CONNECTOR_ID_POSTFIX, portIdentifier);
			m_packageFactory = new ArduinoSerialPackageFactory ();
			m_deviceCount = 0;

		}

		public override void Start () {

			if (!m_connection.Ready) {
		
				m_connection.Start ();

			}
		
		}

		public override void Stop () {

			if (!m_connection.Ready) {

				m_connection.Start ();

			}

		}

		public override bool Ready { get { return m_connection.Ready; } }

		public IInputMeter<double> CreateAnalogInput (string id, int adcPort) {
			
			DeviceResponsePackage response = new DeviceResponsePackage (m_connection.Send (m_packageFactory.CreateDevice (m_deviceCount++, DeviceType.AnalogueInput, (byte) adcPort).ToBytes ()));

			if (response.IsError) {
			
				throw new System.IO.IOException ("Unable to create device: {response.Value}");
			}

			return new SerialAnalogInput (id, response.Id, m_connection, m_packageFactory);

		}

		public IInputPort CreateInputPort (string id, int adcPort) {
		
			DeviceResponsePackage response = new DeviceResponsePackage (m_connection.Send (m_packageFactory.CreateDevice (m_deviceCount++, DeviceType.DigitalInput, (byte) adcPort).ToBytes ()));

			if (response.IsError) {

				throw new System.IO.IOException ("Unable to create device: {response.Value}");
			}

			return new SerialDigitalInput (id, response.Id, m_connection, m_packageFactory);
		}


		public IOutputPort CreateOutputPort (string id, int adcPort) {
		
			DeviceResponsePackage response = new DeviceResponsePackage (m_connection.Send (m_packageFactory.CreateDevice (m_deviceCount++, DeviceType.DigitalOutput, (byte) adcPort).ToBytes ()));

			if (response.IsError) {

				throw new System.IO.IOException ("Unable to create device: {response.Value}");
			}

			return new SerialDigitalOutput (id, response.Id, m_connection, m_packageFactory);

		}

		public IServo CreateServo (string id, int adcPort) {
		
			DeviceResponsePackage response = new DeviceResponsePackage (m_connection.Send (m_packageFactory.CreateDevice (m_deviceCount++, DeviceType.Servo, (byte) adcPort).ToBytes ()));

			if (response.IsError) {

				throw new System.IO.IOException ("Unable to create device: {response.Value}");
			}

			return new SerialServo (id, response.Id, m_connection, m_packageFactory);

		}

	}
}

