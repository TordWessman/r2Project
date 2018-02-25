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
using System.Collections.Generic;
using System.Linq;
using Core.Device;
using System.Threading;
using Core;

namespace GPIO
{
	internal class SerialNode: ISerialNode {

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
		private AutoResetEvent m_release;

		/// <summary>
		/// Interval used for update frequencies. If value < 0, the timer will not execute. 
		/// </summary>
		private int m_updateInterval;

		public byte NodeId { get { return m_nodeId; } }

		public bool Sleep {

			get { return m_shouldSleep; }
			set { 

				m_host.Sleep (NodeId, value);
				m_shouldSleep = value;
				StartScheduledSynchronization ();

			}

		}

		internal SerialNode(byte nodeId, ISerialHost host) {

			m_host = host;
			m_nodeId = nodeId;
			m_devices = new List<ISerialDevice> ();
			m_updateInterval = Settings.Consts.SerialNodeUpdateInterval();

		}

		public void Synchronize() { m_devices.ForEach (device => device.Synchronize ()); }

		public void Track (ISerialDevice device) {

			m_devices.Add (device); 
			device.Synchronize ();

		}

		private void StartScheduledSynchronization() {

			//if (SleepUpdateInterval <= 0) { throw new 
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
		
			try {

				// Wake up node
				m_host.Sleep (m_nodeId, false);

				// Synchronize it's values
				Synchronize ();

				if (m_shouldSleep) {

					// Sleep if it's supposed to sleep.
					m_host.Sleep (m_nodeId, true);

				}

			} catch (Exception ex) { Log.w ($"Node ({m_nodeId}) update error: {ex.Message}"); }

		}

	}

	/// <summary>
	/// A representation of a remote node
	/// </summary>
	public interface ISerialNode {
	
		/// <summary>
		/// Will synchronize each device assoicated with this node.
		/// </summary>
		void Synchronize ();

		/// <summary>
		/// Adds a device to this node representations tracking list. This allows devices connected to the node to periodically cache their values.
		/// </summary>
		/// <param name="device">Device.</param>
		void Track (ISerialDevice device);

		/// <summary>
		/// Sends this node to sleep.
		/// </summary>
		/// <value><c>true</c> if sleep; otherwise, <c>false</c>.</value>
		bool Sleep { get; set; }

		/// <summary>
		/// The node id of this node representation
		/// </summary>
		/// <value>The node identifier.</value>
		byte NodeId { get; }

	}

	internal class SerialDeviceManager
	{
		private IList<ISerialNode> m_nodes;
		private ISerialHost m_host;

		internal IList<ISerialNode> Nodes { get { return m_nodes; } }

		internal SerialDeviceManager (ISerialHost host) {

			m_host = host;
			m_nodes = new List<ISerialNode> ();

		}

		internal void NodeDidReset(byte nodeId) {
		
			m_nodes.Where(n => n.NodeId == nodeId).FirstOrDefault()?.Synchronize();

		}

		internal void Add(ISerialDevice device) {

			ISerialNode node = m_nodes.Where(n => n.NodeId == device.NodeId).FirstOrDefault();

			if (node == null) {
			
				node = new SerialNode (device.NodeId, m_host);
				m_nodes.Add (node);

			}

			node.Track (device);

		}
	}
}

