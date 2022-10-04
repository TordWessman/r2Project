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
using System;
using R2Core.Device;

namespace R2Core.GPIO
{
	/// <summary>
	/// Base implementation for serial devices. Contains standard functionality including access to the ISerialHost and node management.
	/// </summary>
	internal abstract class SerialDeviceBase<T>: DeviceBase, ISerialDevice {

		// Defined by the host application. A port with this value is not in use.
		private const byte DEVICE_PORT_NOT_IN_USE = 0xFF;

        /// <summary>
        /// The cached internal value of this node.
        /// </summary>
        protected T InternalValue;

        /// <summary>
        /// The ´ISerialHost´ this device is using to transmit data.
        /// </summary>
        protected IArduinoDeviceRouter Host { get; private set; }

        /// <summary>
        /// The identifier at the serial host.
        /// </summary>
        protected byte DeviceId { get; private set; }

		public ISerialNode Node { get; private set; }

        /// <summary>
        /// True if this device has been removed from the node.
        /// </summary>
        /// <value><c>true</c> if deleted; otherwise, <c>false</c>.</value>
        public bool Deleted { get; private set; }

        public bool IsSleeping { get { return Node.Sleep; } }
		public override bool Ready { get {

                try {

                    return !Deleted && Node.Ready;
                    
                } catch (Exception ex) { 

                    Log.w(ex.Message);
                    return false;

                }

            }

        }

		internal SerialDeviceBase(string id, ISerialNode node, IArduinoDeviceRouter host): base(id) {
			
			Host = host;
			Node = node;
			Node.Track(this);

		}

		/// <summary>
		/// For creation of the device remotely: Parameters(i.e. ports) required
		/// </summary>
		protected abstract byte[] CreationParameters { get; }

		/// <summary>
		/// For creation of the device remotely: The explicit device type.
		/// </summary>
		/// <value>The type of the device.</value>
		/// 
		protected abstract SerialDeviceType DeviceType { get; }

		/// <summary>
		/// Determines the sleep state of the node. Will retrieve an updated value if not sleeping or return the privious value if it is. 
		/// </summary>
		/// <returns>The value.</returns>
		protected T GetValue() {

            if (!Ready) { throw new System.IO.IOException("Unable to get value. Device not Ready." + (Deleted ? " Deleted" : "")); }

            if (!IsSleeping) { Update(); }

			return InternalValue; 

		}

        public void Update() {

            InternalValue = Host.GetValue<T>(DeviceId, Node.NodeId).Value;

		}

        public void Delete() {

            Host.DeleteDevice(DeviceId, Node.NodeId);
            Node.RemoveDevice(this);
            Deleted = true;

        }

        public void Synchronize() {

			DeviceData<T>info = Host.Create<T>((byte)Node.NodeId, DeviceType, CreationParameters);
            DeviceId = info.Id;
			InternalValue = info.Value;

		}

		// Calculation is defined by the node application
		public byte Checksum { get { return (byte)(((int)DeviceType << 4) + DeviceId + ((CreationParameters.Length == 0 ? DEVICE_PORT_NOT_IN_USE : CreationParameters[0]) ^ 0xFF)); } }

		public override string ToString() {
			
			return $"[SerialDevice: Id={DeviceId}, NodeId={Node.NodeId}, Value={InternalValue}, IsSleeping={IsSleeping}, Ready={Ready}]";
		
		}
	
	}

}
