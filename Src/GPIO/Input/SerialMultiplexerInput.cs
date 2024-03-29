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

namespace R2Core.GPIO {

    /// <summary>
    /// Represents an input object (similar to IInputMeter) that is capable of returning values from multiple sensors.
    /// </summary>
    public interface ISerialMultiplexerInput<T>: ISerialDevice {

        /// <summary>
        /// Returns a value of type `T` using the internal sensor with index `sensorIndex`.
        /// </summary>
        /// <returns>The for.</returns>
        /// <param name="sensorIndex">Sensor index.</param>
        T ValueFor(int sensorIndex);

        /// <summary>
        /// Creates a "link" to a sensor with index `sensorPairIndex`.
        /// </summary>
        /// <returns>The sensor.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="sensorPairIndex">Sensor pair index.</param>
        IInputMeter<T> LinkSensor(string id, int sensorPairIndex);

    }

}
