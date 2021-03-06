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
using System;
using R2Core.Network;

namespace R2Core.Device
{
    /// <summary>
    /// Represents a non-local device. 
    /// </summary>
    public interface IRemoteDevice : IDevice {

        /// <summary>
        /// Exposes the underlying network connection
        /// </summary>
        /// <value>The connection.</value>
        INetworkConnection Connection { get; }

        /// <summary>
        /// Asynchronously access this object (as ´dynamic´). ´callback´ is called when the operation finishes.
        /// dynamic contains any result. Exception is not null if any error occurred. 
        /// </summary>
        /// <param name="callback">Callback.</param>
        dynamic Async(Action<dynamic, Exception> callback);

        /// <summary>
        /// Returns true if the connection is busy sending.
        /// </summary>
        /// <value><c>true</c> if busy; otherwise, <c>false</c>.</value>
        bool Busy { get; }

    }

}

