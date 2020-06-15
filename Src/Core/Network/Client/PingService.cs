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
using R2Core.Device;

namespace R2Core.Network
{
	/// <summary>
	/// Send ping messages...
	/// </summary>
	public class PingService : DeviceBase {
		
		private INetworkConnection m_client;

		// Send ping to server.
		private System.Timers.Timer m_pingTimer;

		public PingService(INetworkConnection client, int timeout) : base(Settings.Identifiers.PingService()) {

			m_client = client;

			m_pingTimer = new System.Timers.Timer(timeout);
			m_pingTimer.Elapsed += PingEvent;

		}

		public override void Start() {

			m_pingTimer.Start();

		}

		public override void Stop() {

			m_pingTimer.Close();
            m_pingTimer.Enabled = false;
            m_pingTimer.Stop();
            m_pingTimer.Dispose();
            m_pingTimer = null;

        }

		void PingEvent(object sender, System.Timers.ElapsedEventArgs e) {
			
			if (m_client.Ready) { 

				PingMessage ping = new PingMessage();
				m_client.Send(ping);
				 
			}

		}

		/// <summary>
		/// Reply with a ´PongMessage´.
		/// </summary>
		public void Pong() {

			if (m_client.Ready) {
				
				PongMessage ping = new PongMessage();
				m_client.Send(ping);

			}

		}

	}

}

