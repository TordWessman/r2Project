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


namespace GPIO
{
	public class OutputPort : DeviceBase, IOutputPort
	{
		private RaspberryPiDotNet.GPIO m_gpi;
		private bool m_value;

		public OutputPort (string id, RaspberryPiDotNet.GPIO gpio, bool initialValue = false) : base (id)
		{
			m_gpi = gpio;
			
			if (m_gpi.PinDirection != RaspberryPiDotNet.GPIODirection.Out) {
				
				throw new ArgumentException ("Provided GPIO pin had not out-direction");

			}

			Value = initialValue;

		}

		#region IOutputPort implementation

		public bool Value  {
			
			set {
				
				m_value = value;
				m_gpi.Write (value);

			}

			get { return m_value; }

		}

		#endregion

	}

}

