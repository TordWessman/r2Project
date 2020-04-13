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

namespace R2Core.Network {

    /// <summary>
    /// Sent when registering self at a TCPRouterEndpoint.
    /// </summary>
    public struct RoutingRegistrationRequest {

        /// <summary>
        /// The IIdentity host name of my instance.
        /// </summary>
        /// <value>The name of the host.</value>
        public string HostName { get; set; }

        /// <summary>
        /// My local address.
        /// </summary>
        /// <value>The address.</value>
        public string Address { get; set; }

        /// <summary>
        /// My local port.
        /// </summary>
        /// <value>The port.</value>
        public int Port { get; set; }

        public override string ToString() {
            return $"RoutingRegistrationRequest: [HostName: {HostName}, Address: {Address}, Port: {Port}]";
        }

    }

    /// <summary>
    /// Response froma a TCPRouterEndpoint if registration was successfull
    /// </summary>
    public struct RoutingRegistrationResponse {

        /// <summary>
        /// The address from which I made the request.
        /// </summary>
        /// <value>The address.</value>
        public string Address { get; set; }

        /// <summary>
        /// The remote port used by my connection.
        /// </summary>
        /// <value>The port.</value>
        public int Port { get; set; }

    }

}
