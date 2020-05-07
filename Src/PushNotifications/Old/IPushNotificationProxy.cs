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
		/// Stores a client(phone) and makes it available for receiving push notifications
		/// </summary>
		/// <param name="deviceId">Device identifier.</param>
		/// <param name="deviceToken">Device token.</param>
		/// <param name="type">Type.</param>
		void RegisterClient(string deviceId, string deviceToken, PushNotificationClientType type = PushNotificationClientType.Apple);

		/// <summary>
		/// As above but takes the int value of PushNotificationClientType as parameter instead.
		/// </summary>
		/// <param name="deviceId">Device identifier.</param>
		/// <param name="deviceToken">Device token.</param>
		/// <param name="rawType">Raw type.</param>
		void Register(string deviceId, string deviceToken, int rawType);

		/// <summary>
		/// Adds a push notification sender for receiving push notification messages
		/// </summary>
		/// <param name="facade">Facade.</param>
		void AddFacade(IPushNotificationFacade facade);

		/// <summary>
		/// Sends a notification to its designated clients through its available facades(using PushNotificationClientTypes in the IPushNotification.ClientTypeMask)
		/// </summary>
		/// <param name="notification">Notification.</param>
		void Broadcast(IPushNotification notification);

	}

}