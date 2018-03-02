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
using Core;

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
		private byte[] m_packageHeader;

		private SerialPort m_serialPort;
		private int m_timeout = 0;

		private static readonly object m_lock = new object();

		/// <summary>
		/// portIdentifier is either an explicit name of the port (i.e. /dev/ttyACM0) or a regexp pattern (i.e. /dev/ttyACM). In the latter case, the first matching available port is used. 
		/// </summary>
		/// <param name="portIdentifier">Port identifier.</param>
		/// <param name="baudRate">Baud rate.</param>
		public ArduinoSerialConnector(string id, string portIdentifier, int baudRate): base(id) {

			string portName = GetSerialPort(portIdentifier);

			m_packageHeader = Settings.Consts.ArduinoSerialConnectorPackageHeader().Split(',').Select( b => byte.Parse(b, System.Globalization.NumberStyles.HexNumber)).ToArray();
			m_timeout = Settings.Consts.ArduinoSerialConnectorTimeout ();

			if (portName == null) {

				throw new System.IO.IOException ($"Unable to match any serial port to identifier '{portIdentifier}'.");

			}

			m_serialPort = new SerialPort (portName, baudRate);

			m_serialPort.DtrEnable = true;
			m_serialPort.Parity = Parity.None;
			m_serialPort.DataBits = 8;
			m_serialPort.StopBits = StopBits.One;
			m_serialPort.Handshake = Handshake.None;
			m_serialPort.BaudRate = baudRate;
			m_serialPort.ReadTimeout = m_timeout;
			m_serialPort.WriteTimeout = m_timeout;

		}

		/// <summary>
		/// Gets or sets the serial port's read & write timouts.
		/// </summary>
		/// <value>The timeout.</value>
		public int Timeout {
		
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

			Log.d ($"Connectiing to {m_serialPort.PortName}");

			try {
				m_serialPort.Open ();
			} catch (TimeoutException) {
				//ehh this seems to be needed
				m_serialPort.Open ();
				Log.t ("The fulhack did work...");
			
			}
			m_serialPort.DiscardOutBuffer ();
			m_serialPort.DiscardInBuffer ();
			System.Threading.Thread.Sleep (m_timeout);

		}

		public override bool Ready { get { return m_serialPort.IsOpen; } }

		public byte[] Send(byte[] request) {

			lock (m_lock) {

				// Make sure the input buffer is empty before sending.
				ClearPipe ();

				byte[] requestPackage = new byte[request.Length + 1 + m_packageHeader.Length];

				// First byte should have the value of the rest of the transaction.
				requestPackage [m_packageHeader.Length] = (byte) request.Length;
				System.Buffer.BlockCopy (request, 0, requestPackage, 1 + m_packageHeader.Length, request.Length);

				if (m_packageHeader != null) {

					System.Buffer.BlockCopy (m_packageHeader, 0, requestPackage, 0, m_packageHeader.Length);

				}

				m_serialPort.Write (requestPackage, 0, requestPackage.Length);

				return Read ();

			}

		}

		public byte[] Read() {
			
			for (int i = 0; i < m_packageHeader.Length; i++) {

				byte headerByte = (byte) m_serialPort.ReadByte ();

				if (headerByte != m_packageHeader [i]) {

					throw new System.IO.IOException ($"Bad Package header: {headerByte} at {i} (should have been {m_packageHeader [i]}).");
				
				}

			}

			// First byte should contain the size of the rest of the transaction.
			int responseSize = m_serialPort.ReadByte ();

			byte[] readBuffer = new byte[responseSize];

			for (int i = 0; i < readBuffer.Length; i++) {

				readBuffer[i] = (byte)m_serialPort.ReadByte ();

			}

			ClearPipe ();

			return readBuffer;

		}

		public override void Stop() {

			m_serialPort.Close ();

		}

		/// <summary>
		/// Make sure the input stream is empty.
		/// </summary>
		private void ClearPipe() {

			if (m_serialPort.BytesToRead > 0) { Log.w ("Ahr, there was apparently some data in the pipe."); }
			m_serialPort.DiscardInBuffer ();

		}

	}

}