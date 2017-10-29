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


namespace Audio.ASR
{

	public class ASRController : RemotlyAccessibleDeviceBase, IASRController
	{
		private IASR m_asr;
		
		public ASRController (string id, IASR asr) : base (id)
		{
			m_asr = asr;
		}
		
		public void SetActive (bool active)
		{
			m_asr.Active = active;
		}
		
		#region implemented abstract members of Core.Device.RemotlyAccessibleDeviceBase
		public override byte[] RemoteRequest (string methodName, byte[] rawData, IRPCManager<System.Net.IPEndPoint> mgr)
		{
			
			if (methodName == RemoteASRController.SET_ACTIVE_FUNCTION_NAME) {
				SetActive (mgr.ParsePackage<bool> (rawData));
				return null;
			}
			
			throw new System.NotImplementedException ("Method not implemented: " + methodName + " wanted: " + RemoteASRController.SET_ACTIVE_FUNCTION_NAME);
		}

		public override RemoteDevices GetTypeId ()
		{
			return RemoteDevices.ASRController;
		}
		#endregion
	}
}

