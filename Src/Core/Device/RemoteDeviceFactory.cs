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
//using Core.Network.Data;
using System.Net;
//using Core.Network;
using Core.Memory;

namespace Core.Device
{
	public class RemoteDeviceFactory
	{
		private IRPCManager<IPEndPoint> m_rpcManager;
		
		public RemoteDeviceFactory (IRPCManager<IPEndPoint> rpcManager)
		{
			m_rpcManager = rpcManager;
		}
		
		public IDevice CreateRemoteDevice (string deviceId, Guid guid, string deviceType, IPEndPoint host)
		{

			RemoteDeviceReference reference = new RemoteDeviceReference (deviceId, guid.ToString(), m_rpcManager, host);

			if (deviceType.Equals (RemoteDevices.Dummy.ToString ())) {
				return new RemoteDummy (reference);
			} else if (deviceType.Equals (RemoteDevices.Espeak.ToString ())) {
				return new RemoteEspeak (reference);
			} else if (deviceType.Equals (RemoteDevices.MemoryBus.ToString ())) {
				return new RemoteMemoryBus (reference);
			} else if (deviceType.Equals (RemoteDevices.Camera.ToString ())) {
				return new RemoteCameraController (reference);
			} else if (deviceType.Equals (RemoteDevices.AnalogInput.ToString ())) {
				return new RemoteInputMeter (reference);
			} else if (deviceType.Equals (RemoteDevices.InputPort.ToString ())) {
				return new RemoteInputPort (reference);
			} else if (deviceType.Equals (RemoteDevices.OutputPort.ToString ())) {
				return new RemoteOutputPort (reference);
			} else if (deviceType.Equals (RemoteDevices.ScriptExecutor.ToString ())) {
				return new RemoteScriptExecutor (reference);
			} else if (deviceType.Equals (RemoteDevices.ScriptExecutorFactory.ToString ())) {
				return new RemoteScriptExecutorFactory (reference);
			} else if (deviceType.Equals (RemoteDevices.ASRController.ToString ())) {
				return new RemoteASRController (reference);
			} else if (deviceType.Equals (RemoteDevices.Servo.ToString ())) {
				return new RemoteServo (reference);
			} else if (deviceType.Equals (RemoteDevices.CommandExecutor.ToString ())) {
				return new RemoteCommandExecutor (reference);
			} else if (deviceType.Equals (RemoteDevices.AudioPlayer.ToString ())) {
				return new RemoteAudioPlayer (reference);
			} else if (deviceType.Equals (RemoteDevices.Gstream.ToString ())) {
				return new RemoteGstream (reference);
			} else if (deviceType.Equals (RemoteDevices.MemorySource.ToString ())) {
				return new RemoteMemorySource (reference);
			} else {
				throw new ApplicationException("YOU HAVE FORGOTTEN TO ADD DEVICE TO RemoteDeviceFactory. Device: " + deviceType + " not found.");
			}

		}
	}
}

