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
using Raspberry.IO.GeneralPurpose;
using System.Collections.Generic;

namespace R2Core.GPIO
{
	/// <summary>
	/// This factory should replace PiGPIOFactory, since it's using the newer libraries.
	/// </summary>
	public class PiGPIOFactory2 : DeviceBase, IGPIOFactory
	{
		public readonly IDictionary<int,ConnectorPin> AvailablePorts = new Dictionary<int,ConnectorPin> {
			{18, ConnectorPin.P1Pin12},
			{23, ConnectorPin.P1Pin16},
			{24, ConnectorPin.P1Pin18},
			{25, ConnectorPin.P1Pin22},
			{4, ConnectorPin.P1Pin07},
			{17, ConnectorPin.P1Pin11}
		};
		public PiGPIOFactory2(string id) : base(id) {
		}

		public IInputMeter<double> CreateAnalogInput(string id, int adcPort) {
		
			throw new NotImplementedException();
		}

		public IServoController CreateServoController(string id, int bus = 1, int address = 0x40, int frequency = 63) {

			return new PCA9685ServoController(id, bus, address, frequency);

		}

		public IInputPort CreateInputPort(string id, int gpioPort) {
		
			return new InputPort2(id,AvailablePorts[gpioPort]);

		}

		public IOutputPort CreateOutputPort(string id, int gpioPort) {

			return new OutputPort2(id, AvailablePorts[gpioPort]);

		}

		public IDHT11 CreateTempHumidity(string id, int pin) {
		
			throw new NotImplementedException();

		}

	}
}

