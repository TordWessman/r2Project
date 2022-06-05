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
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using dotAPNS;
using R2Core.Device;

namespace R2Core.PushNotifications {

    public class ApplePushNotificationFacade2 : DeviceBase, IPushNotificationFacade {

        private ApnsClient m_client;

        public PushNotificationClientType ClientType => PushNotificationClientType.Apple;

        public ApplePushNotificationFacade2(string id, string certFileName, string password) : base(id) {

            if (!File.Exists(certFileName)) {

                throw new IOException($"A push notification certificate with name '{certFileName}' could not be found.");

            }

            m_client = ApnsClient.CreateUsingCert(certFileName, password);

        }

        public void Send(PushNotification notification, string deviceToken) {

            var push = new ApplePush(ApplePushType.Alert)
    .AddAlert("Title", notification.Message)
    .AddToken(deviceToken);

            try {
                var response = m_client.Send(push);
                if (response.IsCompleted) {
                    Console.WriteLine("An alert push has been successfully sent!");
                } else {
                    switch (response.Result.Reason) {
                        case ApnsResponseReason.BadCertificateEnvironment:
                            Log.e($"Certificate is for the wrong environment: {response.Result.Reason}.", Identifier);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(response.Result.Reason), response.Result.Reason, null);
                    }
                    Log.e("Failed to send a push, APNs reported an error: " + response.Result.ReasonString, Identifier);
                }
            } catch (TaskCanceledException ex) {

                Log.e($"HTTP request timed out {ex.Message}.", Identifier);

            } catch (HttpRequestException ex) {

                Log.e($"Failed to send push: {ex.Message}.", Identifier);

            } catch (ApnsCertificateExpiredException ex) {

                Log.e($"APNs certificate has expired: {ex.Message}.", Identifier);

            }
        }
    }
}
