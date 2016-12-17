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
	public class RemoteServo  : RemoteDeviceBase, IServo
	{
		public static readonly string GET_VALUE_FUNCTION_NAME = "get_float_function_name";
		public static readonly string SET_VALUE_FUNCTION_NAME = "set_float_function_name";
		
		public static readonly string SET_MAX_VALUE_FUNCTION_NAME = "set_max_value_function_name";
		public static readonly string GET_MAX_VALUE_FUNCTION_NAME = "get_max_value_function_name";
		public static readonly string SET_MIN_VALUE_FUNCTION_NAME = "set_min_value_function_name";
		public static readonly string GET_MIN_VALUE_FUNCTION_NAME = "get_min_value_function_name";
		
		private float m_maxValue;
		private float m_minValue;
		
		public RemoteServo (RemoteDeviceReference reference) : 
			base (reference)
		{
			m_maxValue = -1;
			m_minValue = -1;
		}

		#region IServo implementation

		public float GetValue ()
		{
			return Execute<float> (GET_VALUE_FUNCTION_NAME);
		}

		public void SetValue (float value)
		{
			Execute<float> (SET_VALUE_FUNCTION_NAME, value);
		}

		public float Value {
			get {
				return Execute<float> (GET_VALUE_FUNCTION_NAME);
				
			}
			
			set {
				Execute<float> (SET_VALUE_FUNCTION_NAME, value);
			}
		}

		public float MaxValue {
			get {
				if (m_maxValue == -1) {
					m_maxValue = Execute<float> (GET_MAX_VALUE_FUNCTION_NAME); 
				}
				
				return m_maxValue;
				
			}
			set {
				Execute<float> (SET_MAX_VALUE_FUNCTION_NAME, value);
				m_maxValue = value;
			}
		}

		public float MinValue {
			get {
				if (m_minValue == -1) {
					m_minValue = Execute<float> (GET_MIN_VALUE_FUNCTION_NAME);
				}
				
				return m_minValue;
				
			}
			set {
				Execute<float> (SET_MIN_VALUE_FUNCTION_NAME, value);
				m_minValue = value;
			}
		}

		public void SetMaxValue(float value) { MaxValue = value; }

		public void SetMinValue(float value) { MinValue = value; }

		#endregion
	}
}

