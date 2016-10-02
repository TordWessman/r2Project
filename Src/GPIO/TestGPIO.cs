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
using RaspberryPiDotNet;
using Core;

namespace GPIO
{
	public class TestGPIO
	{
		public bool yes = true;
		private GPIOMem led;
		public TestGPIO ()
		{
			//RaspberryPiDotNet.DS1620 a;
			
			//RaspberryPiDotNet.MCP3008 a = new MCP3008(0,led,led,led, led);
			//int hej = a.AnalogToDigital;
			Console.WriteLine ("SH0T");
			//new GPIOMem (
			led = new GPIOMem (GPIOPins.GPIO_18);
			
		}
		public void Start ()
		{
			while (yes) {
				Console.Write (".");
				led.Write (true);
				System.Threading.Thread.Sleep (500);
				led.Write (false);
				System.Threading.Thread.Sleep (500);
			}
		}
		
	}
}

