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
using System.Net;

namespace Core.Device
{
	/// <summary>
	/// Base implementation for remotly accissble devices. RemotlyAccessableDeviceBase sub classes is supposed to mirror a concrete local IDevice implementation
	/// </summary>
	public abstract class RemotlyAccessableDeviceBase : DeviceBase, IRemotlyAccessable
	{

		public RemotlyAccessableDeviceBase (string id) : base (id)
		{

		}

		/// <summary>
		/// Determines if the methodName is representing a standard base method.
		/// </summary>
		/// <returns><c>true</c> if this instance is base method the specified methodName; otherwise, <c>false</c>.</returns>
		/// <param name="methodName">Method name.</param>
		protected bool IsBaseMethod (string methodName)
		{
			
			foreach (string name in Enum.GetNames (typeof(StandardRemoteDeviceMethods))) {

				if (name.ToLower ().Equals (methodName.ToLower ())) {
				
					return true;
				
				}
			
			}
				
			return false;
			
		}

		/// <summary>
		/// Simplifies the access to the standard base methods
		/// </summary>
		/// <returns>The standard device method.</returns>
		/// <param name="methodName">Method name.</param>
		/// <param name="rawData">Raw data.</param>
		/// <param name="mgr">Mgr.</param>
		protected byte[] ExecuteStandardDeviceMethod (string methodName, 
		                                              byte[] rawData, 
		                                              Core.Device.IRPCManager<IPEndPoint> mgr)
		{
			
			if (methodName.ToLower ().Equals (
				StandardRemoteDeviceMethods.Start.ToString ().ToLower ())) {
			
				Start ();
				return null;
			
			} else if (methodName.ToLower ().Equals (
				StandardRemoteDeviceMethods.Stop.ToString ().ToLower ())) {
			
				Stop ();
				return null;
			
			} else if (methodName.ToLower ().Equals (
				StandardRemoteDeviceMethods.Ready.ToString ().ToLower ())) {

				return mgr.RPCReply<bool> (Guid, methodName, Ready);
			
			}
			
			throw new NotImplementedException ("Method: " + methodName + " is not implemented" +
				"as a StandardRemoteDeviceMethod. Please uppdate RemotlyAccessableDeviceBase.");
		
		}
		
		public abstract byte[] RemoteRequest(string methodName, byte[] rawData, IRPCManager<System.Net.IPEndPoint> mgr);
		
		public abstract RemoteDevices GetTypeId();

	}

}

