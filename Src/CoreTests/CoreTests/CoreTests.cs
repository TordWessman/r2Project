﻿// This file is part of r2Poject.
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
using System;
using NUnit.Framework;
using System.Collections.Generic;
using R2Core.Device;
using System.Threading;

namespace R2Core.Tests
{
	public class InvokerDummyDevice : DeviceBase {

		public int SomeValue;

		public InvokerDummyDevice (string id) : base (id) {
			
		}

		public void SomeMethod(int yep = 42) {
		
			SomeValue = yep;
			NotifyChange (SomeValue);

		}

	}

	[TestFixture]
	public class R2CoreTests: TestBase, IDeviceObserver
	{
		public R2CoreTests ()
		{
		}

		/*
		[Test]
		public void TestFileLogger() {

			Log.Instance.Write (new LogMessage ("lol", LogType.Message));
			Log.d ("HEJ");

		}*/
	
		[Test]
		public void TestInvoker() {

			ObjectInvoker invoker = new ObjectInvoker ();

			DummyDevice d = new DummyDevice ("duuumm");
			dynamic res = invoker.Invoke (d, "NoParamsNoNothing", null);
			dynamic ros = invoker.Invoke (d, "OneParam", new List<dynamic>() {9999} );

			Assert.IsNull (res);
			Assert.IsNull (ros);
		}

		DeviceManager deviceManager;
		bool wasInvoked = false;

		[Test]
		public void DeviceNotificationTest() {

			InvokerDummyDevice invokedDevice = new InvokerDummyDevice ("x");
			deviceManager = new DeviceManager ("dm");
			deviceManager.Add (invokedDevice);

			invokedDevice.AddObserver (this);
			invokedDevice.SomeMethod ();
			Thread.Sleep (200);
			Assert.IsTrue (wasInvoked);

			Assert.AreEqual (invokedDevice.SomeValue, 43);
			invokedDevice.RemoveObserver (this);
			wasInvoked = false;
			invokedDevice.SomeMethod (44);
			Thread.Sleep (200);
			Assert.False (wasInvoked);
			Assert.AreEqual (invokedDevice.SomeValue, 44);

		}

		public void OnValueChanged(IDeviceNotification<object> notification) {
		
			InvokerDummyDevice device = deviceManager.Get (notification.Identifier);

			Assert.NotNull (device);
			Assert.AreEqual (notification.NewValue, 42);
			Assert.AreEqual (notification.NewValue, device.SomeValue);
			Assert.AreEqual (notification.Action, "SomeMethod");

			device.SomeValue = 43;
			wasInvoked = true;

		}

	}

}

