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
using System.Linq;
using System.Net.NetworkInformation;
using R2Core.Device;

namespace R2Core {

    /// <summary>
    /// Default IIdentity implementation.
    /// </summary>
    public class Identity : DeviceBase, IIdentity {

        public string Name { get; }
        public string MachineId { get; }

        public static readonly string UnknownMachineId = "unknown";

        public Identity(string name) : base(Settings.Identifiers.Identity()) {

            Name = name;
            MachineId = GetMachineId();

        }

        private string GetMachineId() {

            NetworkInterface[] networkInterface = NetworkInterface.GetAllNetworkInterfaces();
            string id = networkInterface.FirstOrDefault(i => i.GetPhysicalAddress().GetAddressBytes().Length > 0)?.GetPhysicalAddress().ToString();

            return id ?? Identity.UnknownMachineId;

        }

        public override string ToString() {

            return $"Identity [Name: {Name}, MachineId: {MachineId}]";
        
        }

    }

}
