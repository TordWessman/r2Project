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
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace Core.Device
{
	public class RemoteInputPort : RemoteDeviceBase, IInputPort 
	{

		private Func<object> m_triggerFunc;
		private Timer m_timer;
		private AutoResetEvent m_release;
		private bool m_timerDisposed;
		private bool m_pullUp;
		private bool m_isExecuting;

		public RemoteInputPort (RemoteDeviceReference reference) : base (reference) {}

		public void SetTrigger(Func<object> triggerFunc, bool pullUp = false, int interval = 250) {


			m_pullUp = pullUp;
			if (m_timer != null) {
				m_timer.Change (0, 0);
				m_timer.Dispose();
			}

			m_isExecuting = false;
			m_timerDisposed = false;
			m_pullUp = false;
			m_triggerFunc = triggerFunc;

			m_release = new AutoResetEvent(false);

			TimerCallback tcb = CheckValue;

			m_timer = new Timer(tcb, m_release, 1000, interval);


		}

		public void StopTrigger() {
		
			Log.t ("Trigger stopped");
			m_timerDisposed = true;
			m_release.Set ();
			m_timer.Change (0, 0);
			m_timer.Dispose ();

		}

		private void CheckValue(object obj) {

			if (!m_isExecuting) {
				m_isExecuting = true;
				bool value = Value;

				if ((value && !m_pullUp) || (!value && m_pullUp) && !m_timerDisposed) {
					try {
						m_triggerFunc ();
					} catch (Exception ex) {
						Log.x (ex);
						StopTrigger ();
					}
				}

				m_isExecuting = false;

			}
		}

		public static readonly string GET_VALUE_FUNCTION_NAME = "get_bool_function_name";

		public bool Value {
			get {
				bool val = Execute<bool> (GET_VALUE_FUNCTION_NAME);
				return val;
			}
		}

	}
}

