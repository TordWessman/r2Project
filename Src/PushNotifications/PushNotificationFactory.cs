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
using Core.Device;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace PushNotifications
{
	/// <summary>
	/// Factory methods for creating push notification messages as well as facades
	/// </summary>
	public class PushNotificationFactory : Core.Device.DeviceBase
	{
		private string m_certPath;

		/// <summary>
		/// Initializes a new instance of the <see cref="PushNotifications.PushNotificationFactory"/> class.
		/// Optional parameter certPath will be included as an additional search path for certificate files.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="certPath">Cert path.</param>
		public PushNotificationFactory (string id, string certPath = null) : base (id)
		{
			m_certPath = certPath;

			if (certPath != null && !m_certPath.EndsWith (Path.DirectorySeparatorChar.ToString())) {
				
				m_certPath += Path.DirectorySeparatorChar;	
			
			}
		
		}

		public IPushNotification CreateSimple( string message) {
			
			PushNotification note = new PushNotification (message);

			foreach (PushNotificationClientType type in Enum.GetValues(typeof(PushNotificationClientType)) ) {
				note.AddClientType (type);
			}

			return note;

		}

		public IPushNotification CreateApple(string message, string sound = "sound.caf", int badge = 1) {
			
			PushNotification note = new PushNotification (message);
			note.AddClientType (PushNotificationClientType.Apple);
			note.AddValue (PushNotificationValues.AppleSound, sound);

			if (badge > 0) {
				note.AddValue (PushNotificationValues.AppleBadge, badge);
			}

			return note;
		}

		public IPushNotificationFacade CreateAppleFacade(string id, string password, string appleCertFile ) {
			
			if (!File.Exists (appleCertFile)) {

				appleCertFile = (m_certPath ?? "") + appleCertFile;

			}

			return new ApplePushNotificationFacade (id, appleCertFile, password);

		}

		public IPushNotificationProxy CreateHandler(string id, IMemorySource memory) {
		
			return new PushNotificationHandler (id, memory);

		}

	}

}