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
using System.Threading;
using System.Collections.Generic;

namespace R2Core.GPIO
{
	/// <summary>
	/// Default ISerialNode implementation. Capable of synchronizing devices on associated node.
	/// </summary>
	internal class SerialNode : DeviceBase, ISerialNode {

		// Devices connected to this node
		private List<ISerialDevice> m_devices;

		// If true, the node should normally sleep
		private bool m_shouldSleep;

		// Used to communicate with the node
		private IArduinoDeviceRouter m_host;

		// Used for synchronization with sleeping nodes
		private Timer m_timer;

		// Release event for timer
		private AutoResetEvent m_release;

		/// <summary>
		/// Interval used for update frequencies. If value less than 0, the timer will not execute. 
		/// </summary>
		private readonly int m_updateInterval;

		public byte NodeId { get; private set; }
        public bool ContinousSynchronization { get; set; }

        /// <summary>
        /// Will be true if this node is being updated.
        /// </summary>
        /// <value><c>true</c> if is updating; otherwise, <c>false</c>.</value>
        public bool IsUpdating { get; private set; }

		/// <summary>
		/// Returns the timestamp of the time when this node did complete a successful synchronization.
		/// </summary>
		/// <value>The last update.</value>
		public DateTime LastUpdate { get; private set; }

		/// <summary>
		/// Returns the number of failures this node has encountered during Update.
		/// </summary>
		/// <value>The fail count.</value>
		public int FailCount { get; private set; }

		/// <summary>
		/// `nodeID` is the remote id of the node. ISerialHost is used for communication to remote. ISerialHost used for serial communication. `updateInterval`: how often should the nodes update if in sleep mode(if zero, do not update. if below zero, use default value).
		/// </summary>
		/// <param name="nodeId">Node identifier.</param>
		/// <param name="host">Host.</param>
		/// <param name="updateInterval">Update interval.</param>
		internal SerialNode(byte nodeId, IArduinoDeviceRouter host, int updateInterval) : base($"{Settings.Consts.SerialNodeIdPrefix()}{nodeId}") {

			m_host = host;
			NodeId = nodeId;
			m_devices = new List<ISerialDevice>();
			m_updateInterval = updateInterval;
			ContinousSynchronization = true;
			m_shouldSleep = host.IsNodeSleeping(nodeId);

            if (m_shouldSleep) {

				LastUpdate = DateTime.MinValue;
				StartScheduledSynchronization();

			} else {
			
				LastUpdate = DateTime.Now;

			}

		}

		public bool Sleep {

			get { return m_shouldSleep; }
			set { 

				m_host.Sleep(NodeId, value);
				m_shouldSleep = value;

				if (value && m_updateInterval > 0) { StartScheduledSynchronization(); }
				else if (m_updateInterval > 0) { StopScheduledSynchronization(); }

			}

		}

		public override bool Ready { get { return m_host.Ready && m_host.IsNodeAvailable(NodeId); } }
		public override void Start() { StartScheduledSynchronization(); }
		public override void Stop() { StopScheduledSynchronization(); }
		public void Synchronize() { m_devices.ForEach(device => device.Synchronize()); }
		public void Track(ISerialDevice device) { m_devices.Add(device);  }

		private void StopScheduledSynchronization() {

			m_release?.Set();
			m_timer?.Change(0, 0);
			m_timer?.Dispose();
			m_timer = null;

		}

		private void StartScheduledSynchronization() {

			m_timer?.Change(0, 0);
			m_timer?.Dispose();

			m_release = new AutoResetEvent(false);

			m_timer = new Timer(Update, m_release, m_updateInterval, m_updateInterval);

		}

		/// <summary>
		/// Will Synchronize it's values and make sure the node is in the right sleep mode.
		/// </summary>
		/// <param name="obj">Object.</param>
		private void Update(object obj) {

			// abort if the m_shouldUpdate flag is set to false.
			if (!ContinousSynchronization) { return; }

			// yield if an update is in progress or if scheduling has been stopped.
			if (IsUpdating || m_timer == null) { return; }

			IsUpdating = true;

			try {

				// Wake up node, just in case
				m_host.PauseSleep(NodeId, Settings.Consts.SerialNodePauseSleepInterval());

				// Update the values of tracked devices
				m_devices.ForEach(device => device.Update());

				LastUpdate = DateTime.Now;

			} catch (Exception ex) {
				
				Log.w($"Node({NodeId}) update error: {ex.Message}.", Identifier);
				FailCount++;

			} finally { IsUpdating = false; } 

		}

		public bool Validate() {

			byte[] checksum = m_host.GetChecksum(NodeId);

			if ((checksum[0] & 63) != m_devices.Count) {
			
				Log.e($"Checksum failed for device count(was {(checksum[0] & 63)}).", Identifier);
				return false;
			}

			for (int i = 0; i < m_devices.Count; i++) {
			
				if (checksum[i + 1] != m_devices[i].Checksum) {

					Log.e($"Checksum failed for serial device '{m_devices[i].Identifier}'.", Identifier);
					return false;

				}

			}

			return true;

		}

        public override string ToString() {

            return $"[SerialNode: NodeId: {NodeId}, ContinousSynchronization: {ContinousSynchronization}, Sleeping: {Sleep}, FailCount: {FailCount}, LastUpdate: {LastUpdate}]";
        
        }

    }

}