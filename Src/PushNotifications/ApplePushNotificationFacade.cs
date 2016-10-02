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
using Core.Device;
using PushSharp;
using PushSharp.Apple;
using System.IO;
using System.Collections.Generic;

namespace PushNotifications
{
	public class ApplePushNotificationFacade: DeviceBase, IPushNotificationFacade
	{
	
		private PushBroker m_push;

		public ApplePushNotificationFacade (string id,  string certFileName, string password) : base (id)
		{

			//Create our push services broker
			m_push = new PushBroker();

			//(CERT_PATH + Path.AltDirectorySeparatorChar + 

			var appleCert = File.ReadAllBytes(certFileName);
			m_push.RegisterAppleService(new ApplePushChannelSettings(appleCert, password));

		}

		public void QueuePushNotification(IPushNotification notification , IEnumerable<string> deviceIds) {
			//"9ff08be00e8ea8cba4590b55cf3179dc906ca21eedfcfb82d50ca37b928e2d22"

			if (!AcceptsNotification(notification)) {
				throw new ArgumentException ("Cannot push notification with mask: " + notification.ClientTypeMask + " to Apple.");
			}

			foreach (string deviceId in deviceIds) {
				AppleNotification note = new AppleNotification ()
					.ForDeviceToken (deviceId)
					.WithAlert (notification.Message);

				if (notification.Values.ContainsKey (PushNotificationValues.AppleBadge)) {
					note = note.WithBadge ((int)notification.Values [PushNotificationValues.AppleBadge]);
				}
				if (notification.Values.ContainsKey (PushNotificationValues.AppleSound)) {
					note = note.WithSound ((string)notification.Values [PushNotificationValues.AppleSound]);
				}

				m_push.QueueNotification(note);
			}




		}

		public bool AcceptsNotification (IPushNotification notification)
		{
			return (notification.ClientTypeMask & (int)PushNotificationClientTypes.Apple) > 0;
		}

	}
}

