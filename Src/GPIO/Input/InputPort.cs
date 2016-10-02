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
using System.Threading;
using Core;

namespace GPIO
{

	public class InputPort : RemotlyAccessableDeviceBase, IInputPort, IJSONAccessible
	{

		RaspberryPiDotNet.GPIO m_gpi;

		private Func<object> m_triggerFunc;
		private Timer m_timer;
		private AutoResetEvent m_release;
		private bool m_timerDisposed;
		private bool m_pullUp;

		// Used to make sure the trigger function is not executed concurrently (will ignore calls while the trigger function is running).
		private bool m_threadUnsafeTriggerFunctionIsExecuting;

		public void SetTrigger(Func<object> triggerFunc, bool pullUp = false, int interval = 100) {

			m_pullUp = pullUp;

			if (m_timer != null) {
			
				m_timer.Change (0, 0);
				m_timer.Dispose();
			
			}

			m_timerDisposed = false;
			m_pullUp = false;
			m_triggerFunc = triggerFunc;
			m_release = new AutoResetEvent(false);

			TimerCallback tcb = CheckValue;

			m_timer = new Timer(tcb, m_release, 1000, interval);

		}

		public void StopTrigger() {

			m_timerDisposed = true;
			m_release.Set ();
			m_timer.Change (0, 0);
			m_timer.Dispose ();

		}

		private void CheckValue(object obj) {
		
			if (m_threadUnsafeTriggerFunctionIsExecuting) {
			
				// Do not continue if the function is being executed.
				return;

			}

			bool value = Value;

			if ((value && !m_pullUp) || (!value && m_pullUp) && !m_timerDisposed) {
			
				m_threadUnsafeTriggerFunctionIsExecuting = true;

				try {
				
					m_triggerFunc ();
				
				} catch (Exception ex) {
				
					Log.x (ex);
					StopTrigger ();
				
				} finally {
				
					m_threadUnsafeTriggerFunctionIsExecuting = false;

				}
			
			}
		
		}

		public InputPort (string id, RaspberryPiDotNet.GPIO gpio) : base (id)
		{
			m_gpi = gpio;
			
			if (m_gpi.PinDirection != RaspberryPiDotNet.GPIODirection.In) {
		
				throw new ArgumentException ("Provided GPIO pin had not in-direction");
			
			}
		
		}

		#region IRemotlyAccessable implementation

		public override byte[] RemoteRequest (string methodName, byte[] rawData, IRPCManager<System.Net.IPEndPoint> mgr)
		{
			if (IsBaseMethod (methodName)) {
		
				return ExecuteStandardDeviceMethod (methodName, rawData, mgr);
			
			} else if (methodName == RemoteInputPort.GET_VALUE_FUNCTION_NAME) {
			
				return mgr.RPCReply<bool> (Guid, methodName, Value);
			
			} else
			
				throw new NotImplementedException ("Method name: " + methodName + " is not implemented for Distance meter.");
		}

		public override RemoteDevices GetTypeId ()
		{
		
			return RemoteDevices.InputPort;
		
		}

		#endregion

		#region IInputPort implementation

		public bool Value {

			get {
			
				return m_gpi.Read ();
				//return m_gpi.Read () == RaspberryPiDotNet.PinState.High;
			
			}
		
		}
			

		#endregion

		#region IDynamicReadable implementation

		public string Interpret (string functionName, string parameters = null)
		{

			if (functionName == "get_value") { 
			
				return Value.ToString ();
			
			}

			throw new InvalidOperationException ("function not registered: " + functionName);
		
		}

		#endregion

	}

}