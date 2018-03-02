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
using Core.Device;
using System.Threading;
using System.Collections.Generic;
using Core;

namespace GPIO
{
	/// <summary>
	/// Default ISerialNode implementation. Capable of synchronizing devices on associated node.
	/// </summary>
	internal class SerialNode: DeviceBase, ISerialNode {

		// Remote id of this node
		private byte m_nodeId;
		// Devices connected to this node
		private List<ISerialDevice> m_devices;
		// If true, the node should normally sleep
		private bool m_shouldSleep;
		// Used to communicate with the node
		private ISerialHost m_host;

		// Used for synchronization with sleeping nodes
		private Timer m_timer;
		// Release event for timer
		private AutoResetEvent m_release;
		// Will be true if an Update event is running for this node.
		private bool m_isUpdating;

		/// <summary>
		/// Interval used for update frequencies. If value < 0, the timer will not execute. 
		/// </summary>
		private int m_updateInterval;

		public byte NodeId { get { return m_nodeId; } }

		// Will be true if this node is being updated.
		public bool IsUpdating { get { return m_isUpdating; } }

		/// <summary>
		/// `nodeID` is the remote id of the node. ISerialHost is used for communication to remote. ISerialHost used for serial communication. `updateInterval`: how often should the nodes update if in sleep mode (if zero, do not update. if below zero, use default value).
		/// </summary>
		/// <param name="nodeId">Node identifier.</param>
		/// <param name="host">Host.</param>
		/// <param name="updateInterval">Update interval.</param>
		internal SerialNode(byte nodeId, ISerialHost host, int updateInterval) : base ($"{Settings.Consts.SerialNodeIdPrefix()}{nodeId}") {

			m_host = host;
			m_nodeId = nodeId;
			m_devices = new List<ISerialDevice> ();
			m_updateInterval = updateInterval;
			m_shouldSleep = host.IsNodeSleeping (nodeId);

			if (m_shouldSleep) {

				StartScheduledSynchronization ();

			}

		}

		public bool Sleep {

			get { return m_shouldSleep; }
			set { 

				m_host.Sleep (NodeId, value);
				m_shouldSleep = value;

				if (value && m_updateInterval > 0) { StartScheduledSynchronization (); }
				else if (m_updateInterval > 0) { StopScheduledSynchronization (); }
			}

		}

		public override bool Ready { get { return m_host.IsNodeAvailable (m_nodeId); } }

		public override void Start () { StartScheduledSynchronization (); }
		public override void Stop () { StopScheduledSynchronization (); }

		public void Synchronize() { m_devices.ForEach (device => device.Synchronize ()); }

		public void Track (ISerialDevice device) {

			m_devices.Add (device); 

		}

		private void StopScheduledSynchronization () {

			m_release?.Set ();
			m_timer?.Change (0, 0);
			m_timer?.Dispose ();
			m_timer = null;

		}

		private void StartScheduledSynchronization() {

			if (m_timer != null) {

				m_timer.Change (0, 0);
				m_timer.Dispose();

			}

			m_release = new AutoResetEvent(false);

			m_timer = new Timer(Update, m_release, m_updateInterval, m_updateInterval);

		}

		/// <summary>
		/// Will Synchronize it's values and make sure the node is in the right sleep mode.
		/// </summary>
		/// <param name="obj">Object.</param>
		private void Update(object obj) {

			// yield if an update is in progress or if scheduling has been stopped.
			if (IsUpdating || m_timer == null) { return; }

			m_isUpdating = true;

			try {

				// Wake up node, just in case
				m_host.PauseSleep (m_nodeId, Settings.Consts.SerialNodePauseSleepInterval());

				// Update the values of tracked devices
				m_devices.ForEach (device => device.Update());

			} catch (Exception ex) { Log.w ($"Node ({m_nodeId}) update error: {ex.Message}"); }
			finally { m_isUpdating = false; } 

		}

	}
}

