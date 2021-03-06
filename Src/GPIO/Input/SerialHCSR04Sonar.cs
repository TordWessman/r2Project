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

namespace R2Core.GPIO
{
    internal class SerialHCSR04Sonar : SerialSonar {

        internal SerialHCSR04Sonar(string id, ISerialNode node, IArduinoDeviceRouter host, int triggerPort, int echoPort) : base(id, node, host, triggerPort, echoPort) {
        }

        protected override SerialDeviceType DeviceType { get { return SerialDeviceType.Sonar_HCSR04; } }

    }

}
