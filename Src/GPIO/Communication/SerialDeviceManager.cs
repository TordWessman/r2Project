﻿// This file is part of r2Poject.
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
using System.Collections.Generic;
using System.Linq;

namespace R2Core.GPIO
{
	
	internal class SerialDeviceManager {
		
		private readonly IArduinoDeviceRouter m_host;
		private readonly int m_updateInterval;

		internal IList<ISerialNode> Nodes { get; private set; }

		/// <summary>
		/// ISerialHost used for serial communication. Update interval: how often(in seconds) should the nodes update if in sleep mode(if zero, do not update. if below zero, use default value). 
		/// </summary>
		/// <param name="host">Host.</param>
		/// <param name="updateInterval">Update interval.</param>
		internal SerialDeviceManager(IArduinoDeviceRouter host, int updateInterval = -1) {

			m_host = host;
			Nodes = new List<ISerialNode>();
			m_host.HostDidReset = NodeDidReset;
			m_updateInterval = updateInterval < 0 ? Settings.Consts.SerialNodeUpdateTime() * 1000 : updateInterval * 1000;

		}

		internal void NodeDidReset(byte nodeId) {
		
			// Wake up node, just in case
			m_host.PauseSleep(nodeId, Settings.Consts.SerialNodePauseSleepInterval());

			Nodes.FirstOrDefault(n => n.NodeId == nodeId)?.Synchronize();

		}

		internal ISerialNode GetNode(int nodeId) {
		
			ISerialNode node = Nodes.FirstOrDefault(n => n.NodeId == (byte)nodeId);

			if (node == null) {

				if (!m_host.IsNodeAvailable(nodeId)) {
				
					throw new System.IO.IOException($"Unable to retrieve node {nodeId}. It seems like it's unavailable...");

				}

				node = new SerialNode((byte)nodeId, m_host, m_updateInterval);
				Nodes.Add(node);

			}

			return node;

		}

	}

}

