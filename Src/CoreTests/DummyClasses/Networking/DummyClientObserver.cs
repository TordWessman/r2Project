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
using R2Core.Network;
using NUnit.Framework;

namespace R2Core.Tests
{
	
	public class DummyClientObserver : IMessageClientObserver {

		private string m_destination;

		public bool OnResponseCalled = false;
		public bool OnRequestCalled = false;
		public bool OnBroadcastReceived = false;
		public bool WasClosed = false;

		public INetworkMessage LastResponse;
		public INetworkMessage LastRequest;

		public DummyClientObserver(string destination = null) {

			m_destination = destination;

		}

		public void OnBroadcast(INetworkMessage message) {

			if (Asserter != null) {

				Asserter(message);

			}

			OnBroadcastReceived = true;

		}

		public void OnRequest(INetworkMessage request) { 

			OnRequestCalled = true;
			LastRequest = request;
			if (Asserter != null) {
			
				Asserter(request);

			}

		}

		public void OnResponse(INetworkMessage response, Exception ex) { 

			OnResponseCalled = true;
			LastResponse = response;
			if (Asserter != null) {

				Asserter(response);

			}
		}

		public void OnClose(IMessageClient client, Exception ex) {
		
			if (OnCloseAsserter != null) {

				OnCloseAsserter(client, ex);

			}

		}

		public Action<IMessageClient, Exception> OnCloseAsserter;

		public Action<INetworkMessage> Asserter;

		public string Destination { get { return m_destination; } }

	}

}

