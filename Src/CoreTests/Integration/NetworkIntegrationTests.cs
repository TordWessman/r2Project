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
using R2Core.Tests;
using System.Threading;
using R2Core.Device;
using R2Core.Network;
using R2Core.Scripting;
using R2Core.Common;
using System.Threading.Tasks;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace R2Core.IntegrationTests
{
	[TestFixture]
	public class NetworkIntegrationTests : NetworkTests {
		
		int clientStop = 0;

		[Test]
		public void TestMultipleTCPClients() {

			int randomWorkCount = 500;
			int clientCount = 5;

			Log.Instance.LogLevel = LogType.Message;

			var packageFactory = new TCPPackageFactory(serialization);
			var server1 = factory.CreateTcpServer(Settings.Identifiers.TcpServer(), 10000);
			var device1 = new DummyDevice("dummy1");
			var deviceRouter = factory.CreateDeviceRouter(m_deviceManager);
			m_deviceManager.Add(device1);
			var endpoind = factory.CreateJsonEndpoint(deviceRouter);
			server1.AddEndpoint(endpoind);

			var scriptFactory = new PythonScriptFactory("sf", Settings.Instance.GetPythonPaths(), m_deviceManager);
			scriptFactory.AddSourcePath(Settings.Paths.TestData());
			var pythonScript = scriptFactory.CreateScript("python_test");
			m_deviceManager.Add(pythonScript);

			server1.Start();

			Thread.Sleep(200);
			//Client setup
			IList<Task> clientTasks = new List<Task>();

			int stopMask = 0;

			for (int i = 0; i < clientCount; i++) {
			
				var client = factory.CreateTcpClient($"tcp_client{i}", "localhost", 10000);
				var hostConnection = new HostConnection($"client{1}", client);
				int apa = i;
				clientTasks.Add(Task.Factory.StartNew(() => { Client_Task(apa, hostConnection, randomWorkCount); }, TaskCreationOptions.LongRunning));
			
				stopMask |= 1 << i;

			}

			while(clientStop != stopMask) { Thread.Sleep(500); Log.t(clientStop); }

			server1.Stop();

		}

		public void Client_Task(int number, IClientConnection connection, int randomWorkCount) {
		
			Random r = new Random();

			try {
				
				connection.Start();

			} catch (Exception ex) {
			
				Assert.Fail(ex.Message);
			}

			Thread.Sleep(100);

			for (int i = 0; i < randomWorkCount; i++) {
			
				try {

					double fraction = (double)i / (double)randomWorkCount;

					if (i % (int)(randomWorkCount / 10) == 0) { 
						
						Log.d($"Client: {number} : {fraction * 100} %");
					
					}

					DoRandomWork(connection, r.Next(3));

				} catch (Exception ex) {
				
					Log.e("Client_Task.DoRandomWork crashed!");
					Log.x(ex);
					break;

				}

					
			}

			clientStop |= 1 << number;

			connection.Stop();

		}

		private void DoRandomWork(IClientConnection connection, int func) {

			Log.t($"DoRandomWork: {func}");
			dynamic remoteDummy = new RemoteDevice("dummy1", Guid.Empty, connection);
			dynamic remoteScript = new RemoteDevice("python_test", Guid.Empty, connection);

			if (func == 0) {
			
				Assert.AreEqual(84, remoteScript.add_42 (42));

			} else if (func == 1) {

				TestInvokeDictionary(remoteDummy);

			} else if (func == 2) {

				int aValue = 65536;

				Assert.AreEqual(aValue * 2, remoteScript.wait_and_return_value_plus_value(aValue));

				//wait_and_return_value_plus_value
			} else {
			
			}


		}

		private void TestInvokeDictionary(dynamic remoteDummy) {

			object sendDict = new Dictionary<string, string> { {"apa" , "hund"}, {"gris" , "katt"} };

			R2Dynamic result = remoteDummy.AddCatToKeysAnd42ToValues(sendDict);
			for (int i = 0; i < result.Count; i++) {

				IDictionary<string,string> dictionary = (IDictionary<string,string>)sendDict;
				string expectedKey = dictionary.Keys.ElementAt(i) + "Cat";
				string expectedValue = dictionary.Values.ElementAt(i) + "42";
				Assert.True(result.ContainsKey(expectedKey));
				Assert.True(result.Values.Contains(expectedValue));

			}


		}



	}
}

