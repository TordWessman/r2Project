﻿using System;
using Core.Device;
using Raspberry.IO.GeneralPurpose;
using System.Threading;
using Core;

namespace GPIO
{
	public class InputPort2: RemotlyAccessableDeviceBase, IInputPort
	{
		// GPIO values:
		private ProcessorPin m_pin;
		private IGpioConnectionDriver m_driver;

		// Trigger values

		private Func<object> m_triggerFunc;
		private Timer m_timer;
		private AutoResetEvent m_release;
		private bool m_timerDisposed;
		private bool m_pullUp;

		public InputPort2 (string id, ConnectorPin pin, IGpioConnectionDriver driver = null) : base(id)
		{
		
			m_pin = pin.ToProcessor();
			m_driver = driver ?? GpioConnectionSettings.DefaultDriver;
			m_driver.Allocate(m_pin, PinDirection.Input);

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

				return m_driver.Read (m_pin);

			}

		}


		#endregion

		#region Timer/trigger stuff


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

		#endregion

	}
}
