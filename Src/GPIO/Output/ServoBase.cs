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
using Core;


namespace GPIO
{
	public abstract class ServoBase : RemotlyAccessableDeviceBase, ILocalServo
	{
		public float DEFAULT_MIN_VALUE = 2;
		public float DEFAULT_MAX_VALUE = 177f;
		
		protected IServoController m_servoController;
		protected int m_value;
		protected int m_channel;
		
		protected float m_maxValue;
		protected float m_minValue;
		
		public ServoBase (string id, int channel, IServoController controller) : base (id)
		{
			m_servoController = controller;
			m_channel = channel;
			
			m_maxValue = DEFAULT_MAX_VALUE;
			m_minValue = DEFAULT_MIN_VALUE;
			m_value = (int) LevelModifier;
		}

		public float Value {
		
			get {
				return (float)(m_value - LevelModifier) / Denomiator;
			}
			set {
				if (value < MinValue) {
					value = MinValue;
				} else if (value > MaxValue) {
					value = MaxValue;
				}
				
				m_value = (int)(value * Denomiator + LevelModifier);
				//Log.t ("Setting value: " + m_value);
				m_servoController.Set (m_channel, m_value);
			}
		}
		
		#region implemented abstract members of Core.Device.RemotlyAccessableDeviceBase
		public override byte[] RemoteRequest (string methodName, byte[] rawData, IRPCManager<System.Net.IPEndPoint> mgr)
		{
			if (IsBaseMethod (methodName)) {
				return ExecuteStandardDeviceMethod (methodName, rawData, mgr);
			} else if (methodName == RemoteServo.GET_VALUE_FUNCTION_NAME) {
				return mgr.RPCReply<float> (Guid, methodName, Value);
			} else if (methodName == RemoteServo.SET_VALUE_FUNCTION_NAME) {
				Value = mgr.ParsePackage<float> (rawData);
			} else if (methodName == RemoteServo.GET_MAX_VALUE_FUNCTION_NAME) {
				return mgr.RPCReply<float> (Guid, methodName, MaxValue);
			} else if (methodName == RemoteServo.SET_MAX_VALUE_FUNCTION_NAME) {
				MaxValue = mgr.ParsePackage<float> (rawData);
			} else if (methodName == RemoteServo.GET_MIN_VALUE_FUNCTION_NAME) {
				return mgr.RPCReply<float> (Guid, methodName, MinValue);
			} else if (methodName == RemoteServo.SET_MIN_VALUE_FUNCTION_NAME) {
				MinValue = mgr.ParsePackage<float> (rawData);
			} else
				throw new NotImplementedException ("Method name: " + methodName + " is not implemented for servo.");

			return null;
		}

		public override RemoteDevices GetTypeId ()
		{
			return RemoteDevices.Servo;
		}
		#endregion

		#region ILocalServo implementation
		public float MaxValue {
			get {
				return m_maxValue;
			}
			set {
				m_maxValue = value;
			}
		}

		public float MinValue {
			get {
				return m_minValue;
			}
			set {
				m_minValue = value;
			}
		}

		#endregion

		protected abstract float Denomiator {get;}
		
		protected abstract float LevelModifier {get;}
	}
}

