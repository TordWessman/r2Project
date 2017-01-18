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
using System.Collections.Generic;
using RaspberryPiDotNet;
using Core;


namespace GPIO 
{
	/// <summary>
	/// PRimitive raspberry pi implementation of an IGPIOFactory
	/// </summary>
	public class GPIOFactory: DeviceBase, IGPIOFactory
	{

		private RaspberryPiDotNet.GPIO m_clck;
		private RaspberryPiDotNet.GPIO m_spiIn;
		private RaspberryPiDotNet.GPIO m_spiOut;
		private RaspberryPiDotNet.GPIO m_spiS;
		private Dictionary<int, RaspberryPiDotNet.GPIOPins> m_pinConfiguration;

		private bool[] m_ioPortsUsed;

		public GPIOFactory (string id) : base (id)
		{
		
			m_ioPortsUsed = new bool[40];
			SetUpPinConfiguration ();
			SetUpMCP3008 ();

		}

		private void SetUpMCP3008() {

			m_clck = new RaspberryPiDotNet.GPIOMem(GPIOPins.GPIO_11);
			m_spiIn = new RaspberryPiDotNet.GPIOMem(GPIOPins.GPIO_10);
			m_spiOut = new RaspberryPiDotNet.GPIOMem(GPIOPins.GPIO_09);
			m_spiS = new RaspberryPiDotNet.GPIOMem(GPIOPins.GPIO_08);

			ReservePort (m_clck);
			ReservePort (m_spiIn);
			ReservePort (m_spiOut);
			ReservePort (m_spiS);
		
		}

		/// <summary>
		/// Maps the pin number to the actual pin
		/// </summary>
		private void SetUpPinConfiguration() {
			m_pinConfiguration = new Dictionary<int,RaspberryPiDotNet.GPIOPins> ();

			m_pinConfiguration.Add (7, GPIOPins.GPIO_04);
			m_pinConfiguration.Add (8, GPIOPins.GPIO_14);
			m_pinConfiguration.Add (10, GPIOPins.GPIO_15);
			m_pinConfiguration.Add (11, GPIOPins.GPIO_17);
			m_pinConfiguration.Add (12, GPIOPins.GPIO_18);
			m_pinConfiguration.Add (15, GPIOPins.GPIO_22);
			m_pinConfiguration.Add (16, GPIOPins.GPIO_23);
			m_pinConfiguration.Add (18, GPIOPins.GPIO_24);
			m_pinConfiguration.Add (19, GPIOPins.GPIO_10);
			m_pinConfiguration.Add (21, GPIOPins.GPIO_09);
			m_pinConfiguration.Add (22, GPIOPins.GPIO_25);
			m_pinConfiguration.Add (23, GPIOPins.GPIO_11);
			m_pinConfiguration.Add (24, GPIOPins.GPIO_08);
			m_pinConfiguration.Add (26, GPIOPins.GPIO_07);

		}
		
		private void ReservePort (RaspberryPiDotNet.GPIO port)
		{
			
			if (m_ioPortsUsed [(int)port.Pin]) {
				throw new InvalidOperationException ("Port: " + port + " is allready used!");
			}
			
			m_ioPortsUsed [(int)port.Pin] = true;

		}
		
		private RaspberryPiDotNet.MCP3008 GetMPCFor (int adcPort)
		{
			return  new RaspberryPiDotNet.MCP3008 (adcPort,
				                                        m_clck,
				            m_spiIn,
				            m_spiOut,
			                     m_spiS);
		}
		
		public IServoController CreateServoController (string id, int bus = 1, int address = 0x40, int frequency = 63)
		{
			return new PCA9685ServoController (id, bus, address, frequency);
		}
		
		public IInputMeter<int> CreateInputMeter (string id, int type, int adcPort)
		{
			return CreateInputMeter (id, (GPIOTypes)type, adcPort);
		}
		
		public IInputMeter<int> CreateInputMeter (string id, GPIOTypes type, int adcPort)
		{
			switch (type) {
				
			case GPIOTypes.Sharp2Y0A02:
				return new Sharp2Y0A02 (id, GetMPCFor (adcPort));
			case GPIOTypes.Sharp2D120:
				return new Sharp2D120 (id, GetMPCFor (adcPort));
			case GPIOTypes.MoistYL69:
				return new RT69 (id, GetMPCFor (adcPort));
			case GPIOTypes.LightSensor10k:
				return new LightSensor10k (id, GetMPCFor (adcPort));
			case GPIOTypes.SonarMaxBot:
				return new SonarMaxBot (id, GetMPCFor (adcPort));
			case GPIOTypes.PIRMotionSensor:
				return new PIRMotionSensor (id, GetMPCFor (adcPort));
			case GPIOTypes.InputMeter:
				return new InputMeter	 (id, GetMPCFor (adcPort));
			}
			
			throw new NotImplementedException ("No InputMeterType defined for: " + type.ToString ());
		}
		
		public IInputPort CreateInputPort (string id, int gpioPort)
		{
			

			//RaspberryPiDotNet.GPIOPins
			RaspberryPiDotNet.GPIO input = new GPIOFile (GetPort(gpioPort),
				                                                          RaspberryPiDotNet.GPIODirection.In,
				                                                         false);
			input.PinDirection = GPIODirection.In;

			Log.d("DIRection: " + input.PinDirection.ToString());

			//	new RaspberryPiDotNet.GPIOMem (GetPort(gpioPort),
			  //                                                           RaspberryPiDotNet.GPIODirection.In,
			    //                                                         false);
				
			ReservePort (input);
				
			return new InputPort(id, input);
		}
		
		public IOutputPort CreateOutputPort (string id, int gpioPort)
		{
			

			RaspberryPiDotNet.GPIO output = new RaspberryPiDotNet.GPIOMem (GetPort(gpioPort),
			                                                             RaspberryPiDotNet.GPIODirection.Out,
                                                           false);
			ReservePort (output);
				
			
			return new OutputPort(id, output);
		}

		public IDHT11 CreateTempHumidity(string id, int pin) {
		
			return new DHT11 (id, pin);

		}

		public void PrintPorts() {
			foreach (int portNum in m_pinConfiguration.Keys) {
				if (m_ioPortsUsed[(int)m_pinConfiguration[portNum]]) {
					Log.e("Used: " + String.Format("%i : %s", portNum, m_pinConfiguration[portNum].ToString()));
				} else {
					Log.d("Available: " + String.Format("%i : %s", portNum, m_pinConfiguration[portNum].ToString()));
				}
			}
		}

		/// <summary>
		/// Returns the GPIO enumeration port for the pin specified using the GPIO port naming specified
		/// </summary>
		/// <returns>The port.</returns>
		/// <param name="number">Number.</param>
		private RaspberryPiDotNet.GPIOPins GetPort (int number) {

			return m_pinConfiguration[number];
		}

	
	}
}

