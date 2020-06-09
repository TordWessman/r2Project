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

﻿using R2Core.Device;

namespace R2Core.PushNotifications {

	/// <summary>
	/// A generic interface providing functionality for sending push notifications through its IPushNotificationFacades
	/// </summary>
	public interface IPushNotificationProxy : IDevice {

		/// <summary>
		/// Adds a push notification sender for receiving push notification messages
		/// </summary>
		/// <param name="facade">Facade.</param>
		void AddFacade(IPushNotificationFacade facade);

		/// <summary>
		/// Sends a notification to its designated clients through its available facades.
		/// </summary>
		/// <param name="notification">Notification.</param>
		void Broadcast(PushNotification notification);

	}

}