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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace R2Core.Device {

    /// <summary>
    /// Responsible of monitoring <code>IDevice</code>s and trying to <code>Start</code>
    /// the devices if their <code>Ready</code> status is false.
    /// </summary>
    public class DeviceConnectionManager : DeviceBase {

        /// <summary>
        /// List of monitored devices.
        /// </summary>
        /// <value>The devices.</value>
        public IList<IDevice> Devices { get; }

        /// <summary>
        /// The delay in milliseconds before the monitoring starts.
        /// </summary>
        public int BootDelay { get; private set; }

        /// <summary>
        /// The interval in milliseconds for checking the <code>Ready</code> status of attached devices.
        /// </summary>
        public int UpdateInterval { get; private set; }

        /// <summary>
        /// Returns true if the device montitoring task is running.
        /// </summary>
        /// <value><c>true</c> if is running; otherwise, <c>false</c>.</value>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Contains a list of the last failures of starting a device.
        /// </summary>
        /// <value>The failures.</value>
        public Stack<Exception> Failures { get; }

        private CancellationTokenSource m_cancelationToken;

        public override bool Ready =>   m_cancelationToken != null && 
                                        !m_cancelationToken.Token.IsCancellationRequested &&
                                        IsRunning;

        public DeviceConnectionManager(string id, int bootDelay = 5000, int updateInterval = 1000) : base(id) {

            Devices = new List<IDevice>();
            m_cancelationToken = new CancellationTokenSource();
            Failures = new Stack<Exception>(10);
            BootDelay = bootDelay;
            UpdateInterval = updateInterval;

        }

        public void Add(IDevice device) {

            Devices.Add(device);

        }

        /// <summary>
        /// Start the monitoring after <code>BootDelay</code> milliseconds.
        /// </summary>
        public override void Start() {

            IsRunning = true;

            Task.Run(async () => {

                await Task.Delay(TimeSpan.FromMilliseconds(BootDelay));

                IList<IDevice> failedDevices = new List<IDevice>();

                while (!m_cancelationToken.Token.IsCancellationRequested) {

                    foreach (IDevice device in Devices) {

                        try {

                            if (!device.Ready && !failedDevices.Contains(device)) {

                                Log.w($"Device {device.Identifier} not ready. Will try to Start.", Identifier);
                                failedDevices.Add(device);

                            } else if (device.Ready && failedDevices.Contains(device)) {

                                Log.d($"Device {device.Identifier} started.", Identifier);
                                failedDevices.Remove(device);

                            }

                        } catch (Exception ex) {

                            Log.e(ex.Message);
                        
                        }
                    
                    }

                    foreach (IDevice failedDevice in failedDevices) {

                        try {

                            failedDevice.Start();

                        } catch (Exception ex) {

                            Failures.Push(ex);

                        }

                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(UpdateInterval));

                }

                IsRunning = false;
                m_cancelationToken = new CancellationTokenSource();

            }, m_cancelationToken.Token);

        }

        public override void Stop() {

            m_cancelationToken.Cancel();

        }

    }

}
