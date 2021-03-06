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
using PushSharp.Apple;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace R2Core.PushNotifications
{
	public class ApplePushNotificationFacade : DeviceBase, IPushNotificationFacade {
		
		private ApnsServiceBroker m_push;
		private ApnsConfiguration m_configuration;
        private bool m_sending;

        public PushNotificationClientType ClientType => PushNotificationClientType.Apple;

        public override bool Ready => !m_sending;

        public ApplePushNotificationFacade(string id,  string certFileName, string password) : base(id) {
                
			if (!File.Exists(certFileName)) {
			
				throw new IOException($"A push notification certificate with name '{certFileName}' could not be found.");

			}

			m_configuration = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, 
				certFileName, password);
			
			//Create our push services broker

			var fbs = new FeedbackService(m_configuration);

			fbs.FeedbackReceived += (string deviceToken, DateTime timestamp) => {

				Log.w($"Obsolete device token: {deviceToken}");

			};

			try {

				fbs.Check();

			} catch (AggregateException ex) {

				Log.e($"Certificate `{certFileName}` is probably invalid. Exception: `{ex.InnerException?.InnerException?.Message ?? ex.InnerException?.Message ?? ex.Message}`. Apple Push notification is broken.");

			}

		}

		public void Send(PushNotification notification, string deviceToken) {

            m_sending = true;

            SetupBroker();

			m_push.Start();

			string payload = "{\"aps\":{\"alert\": \"" + notification.Message + "\""; 

            if (notification.Metadata != null) {

                foreach (KeyValuePair<string, object> kvp in notification.Metadata) {

                    payload += ", \"" + kvp.Key + "\":\"" + kvp.Value + "\"";

                }

            }

			payload += "}}";
			var pl = JObject.Parse(payload);

			Log.i($"Sending Push: '{payload}'");

			m_push.QueueNotification(new ApnsNotification {

				DeviceToken = deviceToken,
				Payload = pl
			
			});

			m_push.Stop();

		}

		private void SetupBroker() {
		
			m_push = new ApnsServiceBroker(m_configuration);

            m_push.OnNotificationFailed += (notification, aggregateEx) => {

				aggregateEx.Handle(ex => {

                    // See what kind of exception it was to further diagnose
                    if (ex is ApnsNotificationException notificationException) {

                        // Deal with the failed notification
                        var apnsNotification = notificationException.Notification;
                        var statusCode = notificationException.ErrorStatusCode;

                        Log.e($"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");

                    } else {

                        // Inner exception might hold more useful information like an ApnsConnectionException           
                        Log.e($"Apple Notification Failed for some unknown reason : {ex.InnerException}");

                    }

                    // Mark it as handled
                    return true;

				});

                m_sending = false;

			};

			m_push.OnNotificationSucceeded += (ApnsNotification notification) =>  {

                m_sending = false;
				Log.i($"Did send notification: {notification.DeviceToken}. ");

			};


		}

	}

}