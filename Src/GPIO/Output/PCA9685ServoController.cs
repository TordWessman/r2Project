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
using System.Runtime.InteropServices;
using Core;


namespace GPIO
{
	
	public class PCA9685ServoController : DeviceBase, IServoController
	{
	
		private const string dllPath = "libr2servo.so";
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
   	 	protected static extern void _ext_init_pca9685(int bus, int address, int frequency);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_set_pwm_pca9685 (int channel, int value);
		
		public PCA9685ServoController (string id, int bus = 1, int address = 0x40, int frequency = 63) : base (id)
		{

			Log.t ("GOTOTOTOTOT");
			_ext_init_pca9685(bus, address, frequency);
		
		}

		#region IServoController implementation
		public void Set (int channel, int value)
		{

			if (value < 100 || value > 1000) {
			
				throw new ArgumentException ("Bad value: " + value + " cannel: " + channel);
			
			}
			
			_ext_set_pwm_pca9685 (channel, value);
		}

		public IServo CreateServo (string id, int channel)
		{

			return new CheapBlueServo(id, channel, this);
		
		}

		#endregion
	}
}

