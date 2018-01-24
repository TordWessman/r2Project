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
//
using System;
using NUnit.Framework;
using Core.Tests;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Core;
using System.Threading;

namespace GPIO.Tests
{
	[TestFixture]
	public class GPIOTests: TestBase
	{
		private MockSerialConnection mock_connection;
		private ISerialPackageFactory m_packageFactory;

		public GPIOTests ()
		{
			m_packageFactory = new ArduinoSerialPackageFactory ();
			mock_connection = new MockSerialConnection ("mock", m_packageFactory);
		}

		[Test]
		public void TestSerial() {
			
			var host = new SerialHost ("h", mock_connection, m_packageFactory);
			var factory = new SerialGPIOFactory ("f", host);
			var sensor = factory.CreateAnalogInput ("inp", 14);
			var remoteMock = mock_connection.Devices.First ();

			Random random = new Random();

			for (int i = 0; i < 100; i++) {

				int rand = random.Next(0, 65535);
				remoteMock.IntValues [0] = rand; Assert.AreEqual (rand, sensor.Value);
	
			}

			var dht11 = factory.CreateDht11 ("inp2",2);
			remoteMock = mock_connection.Devices[1];
			var temp = dht11.GetTemperatureSensor ("temp");
			var humid = dht11.GetHumiditySensor ("humid");

			for (int i = 0; i < 100; i++) {

				int rand = random.Next(0, 65535);
				int rand2 = random.Next(0, 65535);
				remoteMock.IntValues [0] = rand;
				remoteMock.IntValues [1] = rand2;

				Assert.AreEqual (rand, temp.Value);
				Assert.AreEqual (rand2, humid.Value);

			}
		}
	}
}

