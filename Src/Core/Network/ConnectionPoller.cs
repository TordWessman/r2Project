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
using System.Net.Sockets;

namespace R2Core.Network
{
	/// <summary>
	/// Polls a socket of a TcpClient. If the polling fails (the connection is down)
	/// ´failDelegate´ is called.
	/// </summary>
	public class ConnectionPoller : DeviceBase {
		
		// Re-do every failed poll.
		private bool m_previousPollSuccess = true;

		private System.Timers.Timer m_connectionCheckTimer;
		private TcpClient m_client;
		private Action m_failDelegate;

		/// <summary>
		/// Initializes a new instance of the <see cref="R2Core.Network.ConnectionPoller"/> class.
		/// ´client´ is the TcpClient containing the Socket to be polled. ´failDelegate´ will be called
		/// if the Socket is closed. 
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="failDelegate">Fail delegate.</param>
		public ConnectionPoller(TcpClient client, Action failDelegate) : base(Settings.Identifiers.ConnectionPoller()) {
			
			m_client = client;
			m_failDelegate = failDelegate;
			m_connectionCheckTimer = new System.Timers.Timer(m_client.SendTimeout);
			m_connectionCheckTimer.Elapsed += ConnectionCheckEvent;

		}

		public override bool Ready {
			
			get {
				
				return m_connectionCheckTimer.Enabled;
			
			}
		
		}

		public override void Start() {

			m_previousPollSuccess = true;
			m_connectionCheckTimer?.Start ();

		}

		public override void Stop() {
		
			m_connectionCheckTimer?.Stop();

		}

		private void ConnectionCheckEvent(object sender, System.Timers.ElapsedEventArgs e) {

			if (!Ready) { return; }

			bool pollSuccessful = m_client.GetSocket()?.Poll(m_client.ReceiveTimeout * 1000, SelectMode.SelectError) ?? false;
			bool connected = m_client.IsConnected();
			bool pollStatus = !pollSuccessful && !m_previousPollSuccess;

			if (!connected || !pollStatus) {

				Log.d($"Polling failed. Will call fail delegate. Poll result: {pollStatus}. Connected: {connected}.");
				m_failDelegate();
				Stop();

			}

			m_previousPollSuccess = pollSuccessful;

		}

	}

}

