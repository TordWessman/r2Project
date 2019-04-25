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

﻿using System;
using R2Core.Device;

namespace R2Core.PushNotifications
{
	/// <summary>
	/// Represent a host environment specific push notification sender. Implementations should be specific for each host type(Apple, Google etc)
	/// </summary>
	public interface IPushNotificationFacade : IDevice {

		/// <summary>
		/// Adds the notification to the host type specific message queue
		/// </summary>
		/// <param name="notification">Notification.</param>
		void QueuePushNotification(IPushNotification notification , System.Collections.Generic.IEnumerable<string> deviceIds);

		/// <summary>
		/// Returns true if the facade accepts the notification
		/// </summary>
		/// <returns><c>true</c>, if notification was acceptsed, <c>false</c> otherwise.</returns>
		/// <param name="type">Type.</param>
		bool AcceptsNotification(IPushNotification notification);

	}
}

