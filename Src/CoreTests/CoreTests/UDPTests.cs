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
using System.Threading;
using Core.Network;
using Core.Data;
using System.Threading.Tasks;

namespace Core.Tests
{
	[TestFixture]
	public class UDPTests: NetworkTests
	{

		[TestFixtureSetUp]
		public override void Setup() {

			base.Setup ();

		}
		[Test]
		public void TestSendReceive() {

			var s = factory.CreateUdpServer ("s", 9876);
			DummyEndpoint ep = new DummyEndpoint ("/dummy");
			UDPClient client = factory.CreateUdpClient ("c", 9876);

			Task waitForResponse;

			s.AddEndpoint (ep);

			s.Start ();
			Thread.Sleep (100);
			Assert.True (s.Ready);
			s.Stop ();
			Thread.Sleep (100);
			Assert.False (s.Ready);
			s.Start ();
			Thread.Sleep (100);
			Assert.True (s.Ready);

			bool didReceiveResponse = false;

			waitForResponse = client.Broadcast (new TCPMessage () { Destination = "should not be found" }, 2000, (response, error) => {


				Assert.IsNull(error);
				Assert.AreEqual (response.Code, (int)WebStatusCode.NotFound);
				didReceiveResponse = true;

			});

			waitForResponse.Wait();
			Assert.True (didReceiveResponse);
			didReceiveResponse = false;

			dynamic payload = new R2Dynamic ();
			payload.Input = 10;

			TCPMessage request = new TCPMessage () {
			
				Payload = payload,
				Destination = "/dummy"
			};

			ep.MessingUp = new Func<INetworkMessage, INetworkMessage> (msg => {

				dynamic returnPayload = new R2Dynamic();
				returnPayload.PaybackTime = msg.Payload.Input * 42;

				return new TCPMessage() {Code = 4242, Payload = returnPayload};

			});

			waitForResponse = client.Broadcast (request, 2000, (response, error) => {

				Log.t("T22222");
				Assert.IsNull(error);
				Assert.AreEqual (4242, response.Code);
				Assert.AreEqual (420, response.Payload.PaybackTime);
				didReceiveResponse = true;
			
			});

			waitForResponse.Wait();
			Assert.True (didReceiveResponse);

			//TCPMessage request = new TCPMessage() {


		}
	}
}

