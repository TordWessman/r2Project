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
using R2Core.Tests;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using R2Core;
using System.Threading;

namespace R2Core.GPIO.Tests
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
		public void TestPackageSerialization() {
		
			DeviceRequestPackage request = new DeviceRequestPackage () {
				Action = SerialActionType.Create,
				Id = 42,
				Content = new byte[2] {
					10,
					20
				},
				NodeId = 99
			};

			byte[] serialized = m_packageFactory.SerializeRequest (request);

			Assert.AreEqual ((byte)SerialActionType.Create, serialized [ArduinoSerialPackageFactory.REQUEST_POSITION_ACTION]);
			Assert.AreEqual (42, serialized [ArduinoSerialPackageFactory.REQUEST_POSITION_ID]);
			Assert.AreEqual (10, serialized [ArduinoSerialPackageFactory.REQUEST_POSITION_CONTENT]);
			Assert.AreEqual (20, serialized [ArduinoSerialPackageFactory.REQUEST_POSITION_CONTENT + 1]);
			Assert.AreEqual (99, serialized [ArduinoSerialPackageFactory.REQUEST_POSITION_HOST]);

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

		[Test]
		public void TestSerialDeviceManager() {

			// For testing purposes, set the update interval to 1 second
			Settings.Consts.SerialNodeUpdateTime (1);

			var host = new SerialHost ("h", mock_connection, m_packageFactory);

			// No device added, so host should not be available
			Assert.IsFalse(host.IsNodeAvailable(3));

			var factory = new SerialGPIOFactory ("f", host);

			// Let the mock behave like there's a node with the id 3.
			mock_connection.nodes.Add (3);

			// Creates an input on node 3
			var sensor = factory.CreateAnalogInput ("inp", 14, 3);

			// The host should have been created at the mock node.
			Assert.IsTrue(host.IsNodeAvailable(3));

			var remoteMock = mock_connection.Devices.Last ();
			remoteMock.IntValues [0] = 42;

			Assert.AreEqual (42, sensor.Value);

			// Send node 3 to sleep and start update cycle.
			factory[3].Sleep = true;

			// Change value. This should not affect the sensor value immediately
			remoteMock.IntValues [0] = 543;

			// Now we should use the cached value
			Assert.AreEqual (42, sensor.Value);

			// Wait for the next update cycle
			Thread.Sleep (2500);

			// Enough time has passed for an update cycle. The value should have been updated
			Assert.AreEqual (543, sensor.Value);

			// The update should now be disabled
			((SerialNode) factory [3]).ShouldUpdate = false;

			// This value should therefore never be read
			remoteMock.IntValues [0] = 43;

			// Wait for the next update cycle (which should not occur)
			Thread.Sleep (2500);

			// The update cycle should not have occured, and the device should still have it's previous value. 
			Assert.AreEqual (543, sensor.Value);
		}
	}
}

