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
using Core.Device;
using RaspberryPiDotNet;
using Core;


namespace GPIO
{
	public abstract class InputMeterBase  : RemotlyAccessableDeviceBase, IInputMeter
	{
		private MCP3008 m_ad;
		
		private float m_c; // Calibration value

		public InputMeterBase (string id, MCP3008 ad) : base (id)
		{
			m_ad = ad;
			m_c = 1.0f;

		}
		
		public int Value {
			get {
				return  (int)((float) ResolveValue () * m_c);
			}
		}
			
		/// <summary>
		/// Used by overriding classes to resolve the value
		/// </summary>
		/// <value>The analog value.</value>
		protected int AnalogValue {
			get {
				return m_ad.AnalogToDigital;
			}
		}

		public void Calibrate(float c) {
			m_c = c;
		}
		
		#region IRemotlyAccessable implementation
		public override byte[] RemoteRequest (string methodName, byte[] rawData, IRPCManager<System.Net.IPEndPoint> mgr)
		{
			if (IsBaseMethod (methodName)) {
				return ExecuteStandardDeviceMethod (methodName, rawData, mgr);
			} else if (methodName == RemoteInputMeter.GET_VALUE_FUNCTION_NAME) {
				return mgr.RPCReply<int> (Guid, methodName, Value);
			} else
				throw new NotImplementedException ("Method name: " + methodName + " is not implemented for Distance meter.");
			
		}

		public override RemoteDevices GetTypeId ()
		{
			return RemoteDevices.InputMeter;
		}
		#endregion
		
		protected abstract int ResolveValue ();

	}
}

