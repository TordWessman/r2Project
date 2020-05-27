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
		private readonly WeakReference<TcpClient> m_client;
		private readonly Action m_failDelegate;

		/// <summary>
		/// Initializes a new instance of the <see cref="R2Core.Network.ConnectionPoller"/> class.
		/// ´client´ is the TcpClient containing the Socket to be polled. ´failDelegate´ will be called
		/// if the Socket is closed. The polling interval is equal to 2 x TcpClient.SendTimeout.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="failDelegate">Fail delegate.</param>
		public ConnectionPoller(TcpClient client, Action failDelegate) : base(Settings.Identifiers.ConnectionPoller()) {
			
			m_client = new WeakReference<TcpClient>(client);
			m_failDelegate = failDelegate;

		}

        public override bool Ready => m_connectionCheckTimer?.Enabled == true;

        public override void Start() {

            if (m_client.GetTarget() == null) {

                throw new NetworkException("TcpClient was null.");

            }

            m_previousPollSuccess = true;
            m_connectionCheckTimer = new System.Timers.Timer(m_client.GetTarget().SendTimeout);
            m_connectionCheckTimer.Elapsed += ConnectionCheckEvent;
            m_connectionCheckTimer.Enabled = true;
            m_connectionCheckTimer.Start ();

		}

		public override void Stop() {

            m_connectionCheckTimer?.Stop();
            m_connectionCheckTimer?.Dispose();
            m_connectionCheckTimer = null;

        }

		private void ConnectionCheckEvent(object sender, System.Timers.ElapsedEventArgs e) {

			if (!Ready || m_client.GetTarget()?.GetSocket() == null) { return; }

			bool pollSuccessful = m_client.GetTarget()?.IsConnected() ?? false;

			if (!m_previousPollSuccess && !pollSuccessful) {
				
				Log.i($"Polling failed to: {m_client.GetTarget()?.GetDescription() ?? "null"}. Will call fail delegate and stop polling.");
				Stop();

                try {

                    m_failDelegate();

                } catch (Exception ex) {

                    Log.w($"Exception when calling fail delegate: {ex.Message}.");
                    Log.x(ex);

                }

			}

			m_previousPollSuccess = pollSuccessful;

		}

	}

}

