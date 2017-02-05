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
using Core;
using System.Linq;
using Newtonsoft.Json.Linq;


namespace PushNotifications
{
	public class ApplePushNotificationFacade: DeviceBase, IPushNotificationFacade
	{
	
		private ApnsServiceBroker m_push;
		private ApnsConfiguration m_configuration;

		public ApplePushNotificationFacade (string id,  string certFileName, string password) : base (id)
		{

			if (!File.Exists (certFileName)) {
			
				throw new IOException ($"A push notification certificate with name '{certFileName}' could not be found.");

			}

			m_configuration = new ApnsConfiguration (ApnsConfiguration.ApnsServerEnvironment.Sandbox, 
				certFileName, password);
			
			//Create our push services broker
			m_push = new ApnsServiceBroker(m_configuration);

			m_push.OnNotificationFailed += (notification, aggregateEx) => {

				aggregateEx.Handle (ex => {

					// See what kind of exception it was to further diagnose
					if (ex is ApnsNotificationException) {
						
						var notificationException = (ApnsNotificationException)ex;

						// Deal with the failed notification
						var apnsNotification = notificationException.Notification;
						var statusCode = notificationException.ErrorStatusCode;

						Log.e ($"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");

					} else {
						
						// Inner exception might hold more useful information like an ApnsConnectionException           
						Log.e ($"Apple Notification Failed for some unknown reason : {ex.InnerException}");
					
					}

					// Mark it as handled
					return true;

				});

			};

			m_push.OnNotificationSucceeded += (ApnsNotification notification) =>  {

				Log.t($"Did send notification: {notification.DeviceToken}. ");

			};

			var fbs = new FeedbackService (m_configuration);

			fbs.FeedbackReceived += (string deviceToken, DateTime timestamp) => {

				Log.w("Obsolete device token: " + deviceToken);

			};

			fbs.Check ();

		}

		public void QueuePushNotification(IPushNotification notification , IEnumerable<string> deviceIds) {

			if (!AcceptsNotification(notification)) {
				
				throw new ArgumentException ($"Cannot push notification with mask: '{notification.ClientTypeMask}' to Apple.");
			
			}

			m_push.Start ();

			foreach (string deviceId in deviceIds) {

				string payload = "{\"aps\":{\"alert\": \"" + notification.Message + "\""; 

				if (notification.Values.ContainsKey (PushNotificationValues.AppleBadge)) {
					
					payload +=  ", \"badge\":" + ((int)notification.Values [PushNotificationValues.AppleBadge]);
				
				}

				if (notification.Values.ContainsKey (PushNotificationValues.AppleSound)) {
					
					payload +=  ", \"sound\":\"" + (string)notification.Values [PushNotificationValues.AppleSound] + "\"";

				}

				payload += "}}";

				Log.t (payload);

				m_push.QueueNotification (new ApnsNotification {

					DeviceToken = deviceId,
					Payload = JObject.Parse (payload)
				
				});

			}

			m_push.Stop ();

		}

		public bool AcceptsNotification (IPushNotification notification)
		{

			return (notification.ClientTypeMask & (int)PushNotificationClientType.Apple) > 0;

		}

	}

}