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
using System.IO;

namespace PushNotifications
{
	/// <summary>
	/// Factory methods for creating push notification messages as well as facades
	/// </summary>
	public class PushNotificationFactory : Core.Device.DeviceBase
	{
		private const string DEFAULT_APPLE_CERT_FILE = "iphone_dev.p12";
		private string m_certPath;

		public PushNotificationFactory (string id, string certPath) : base (id)
		{
			m_certPath = certPath;
			if (!m_certPath.EndsWith (Path.DirectorySeparatorChar.ToString())) {
				m_certPath += Path.DirectorySeparatorChar;	
			}
		}

		public IPushNotification CreateSimple( string message) {
			PushNotification note = new PushNotification (message);

			foreach (PushNotificationClientTypes type in Enum.GetValues(typeof(PushNotificationClientTypes)) ) {
				note.AddClientType (type);
			}

			return note;
		}

		public IPushNotification CreateApple(string message, string sound = "sound.caf", int badge = 1) {
			PushNotification note = new PushNotification (message);
			note.AddClientType (PushNotificationClientTypes.Apple);
			note.AddValue (PushNotificationValues.AppleSound, sound);

			if (badge > 0) {
				note.AddValue (PushNotificationValues.AppleBadge, badge);
			}

			return note;
		}

		public IPushNotificationFacade CreateAppleFacade(string id, string password, string appleCertFile = null ) {
			 appleCertFile = appleCertFile == null ? DEFAULT_APPLE_CERT_FILE : appleCertFile;

			return new ApplePushNotificationFacade (id, m_certPath + appleCertFile, password);
		}
	}
}

