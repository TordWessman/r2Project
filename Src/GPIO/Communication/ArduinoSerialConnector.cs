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
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Linq;
using Core.Device;

namespace GPIO
{
	/// <summary>
	/// Used to interact with a device running a device router (see r2I2CDeviceRouter.ino). Currently only supports packages smaller than 256 bytes.
	/// </summary>
	public class ArduinoSerialConnector: DeviceBase, ISerialConnection
	{
		/// <summary>
		/// The package headers used as "checksum". Defined in the source code for the Arduino slave in r2I2CDeviceRouter.h (PACKAGE_HEADER_IDENTIFIER).
		/// </summary>
		public readonly byte[] PackageHeader = { 0xF0, 0x0F, 0xF1 };

		private SerialPort m_serialPort;
		public const int DEFAULT_BAUD_RATE = 9600;
		private const int DEFAULT_TIMOUT_MS = 5000;

		/// <summary>
		/// portIdentifier is either an explicit name of the port (i.e. /dev/ttyACM0) or a regexp pattern (i.e. /dev/ttyACM). In the latter case, the first matching available port is used. 
		/// </summary>
		/// <param name="portIdentifier">Port identifier.</param>
		/// <param name="baudRate">Baud rate.</param>
		public ArduinoSerialConnector(string id, string portIdentifier, int baudRate = DEFAULT_BAUD_RATE): base(id) {

			string portName = GetSerialPort(portIdentifier);

			if (portName == null) {

				throw new System.IO.IOException ($"Unable to match any serial port to identifier '{portIdentifier}'.");

			}

			m_serialPort = new SerialPort (portName);
			m_serialPort.DtrEnable = true;
			m_serialPort.BaudRate = baudRate;
			m_serialPort.ReadTimeout = DEFAULT_TIMOUT_MS;
			m_serialPort.WriteTimeout = DEFAULT_TIMOUT_MS;
			m_serialPort.ReadTimeout = DEFAULT_TIMOUT_MS;


		}

		/// <summary>
		/// Gets or sets the serial port's read & write timouts.
		/// </summary>
		/// <value>The timout.</value>
		public int Timout {
		
			get { return m_serialPort.WriteTimeout; }
			set {

				m_serialPort.WriteTimeout = value;
				m_serialPort.ReadTimeout = value;
			
			}
		}

		private string GetSerialPort(string portIdentifier) {

			return SerialPort.GetPortNames ().Where (p => p == portIdentifier).FirstOrDefault() ??
				SerialPort.GetPortNames().FirstOrDefault(p => new Regex(portIdentifier).IsMatch(p));

		}

		public override void Start() {

			m_serialPort.Open ();

		}

		public override bool Ready { get { return m_serialPort.IsOpen; } }

		public byte[] Send(byte[] request) {

			if (m_serialPort.BytesToRead > 0) {

				m_serialPort.ReadExisting ();

			}

			byte[] requestPackage = new byte[request.Length + 1 + (PackageHeader?.Length ?? 0)];

			// First byte should have the value of the rest of the transaction.
			requestPackage [PackageHeader?.Length ?? 0] = (byte) request.Length;
			System.Buffer.BlockCopy (request, 0, requestPackage, 1 + (PackageHeader?.Length ?? 0), request.Length);

			if (PackageHeader != null) {
			
				System.Buffer.BlockCopy (PackageHeader, 0, requestPackage, 0, PackageHeader.Length);
			
			}

			m_serialPort.Write (requestPackage, 0, requestPackage.Length);

			return Read ();

		}

		public byte[] Read() {
			
			for (int i = 0; i < (PackageHeader?.Length ?? 0); i++) {

				byte headerByte = (byte) m_serialPort.ReadByte ();

				if (headerByte != PackageHeader [i]) {

					throw new System.IO.IOException ($"Bad Package header: {headerByte} at {i}.");
				
				}

			}

			// First byte should contain the size of the rest of the transaction.
			int responseSize = m_serialPort.ReadByte ();

			byte[] readBuffer = new byte[responseSize];

			for (int i = 0; i < readBuffer.Length; i++) {

				readBuffer[i] = (byte)m_serialPort.ReadByte ();

			}

			return readBuffer;

		}

		public override void Stop() {

			m_serialPort.Close ();

		}

	}

}