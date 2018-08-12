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


namespace R2Core.GPIO
{
	/// <summary>
	/// Creates stuff communicating through a GPIO port
	/// </summary>
	public interface IGPIOFactory : IDevice
	{
		/// <summary>
		/// Create an input meter representation (i e sonar, temperature etc)
		/// </summary>
		/// <returns>The input meter.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="type">Type.</param>
		/// <param name="adcPort">Adc port.</param>
		IInputMeter<double> CreateAnalogInput (string id, int adcPort);

		/// <summary>
		/// Creates an input port on the specified pin
		/// </summary>
		/// <returns>The input port.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="gpioPort">Gpio port.</param>
		IInputPort CreateInputPort (string id, int gpioPort);

		/// <summary>
		/// Creates an output port on the specified pin.
		/// </summary>
		/// <returns>The output port.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="gpioPort">Gpio port.</param>
		IOutputPort CreateOutputPort (string id, int gpioPort);

	}
}

