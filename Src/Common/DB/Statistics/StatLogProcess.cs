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
//

using System;
using System.Timers;
using R2Core.Device;

namespace R2Core.Common {

    /// <summary>
    /// A process for recurring tracking of a IStatLoggable device.
    /// </summary>
    public class StatLogProcess<T> : DeviceBase {

        public DateTime? StartTime { get; private set; }
        public float Frequency { get; private set; }
        private Timer m_timer;
        private Timer m_startTimer;
        private readonly StatLogger m_logger;
        private readonly IStatLoggable<T> m_device;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:R2Core.Common.StatLogProcess`1"/> class.
        /// ´device´ is the object to track. ´logger´ is the StatLogger handling the tracking, 
        /// ´fequency´ is the frequency (in ms) of the tracking and the optional ´startTime´ defines when
        /// the tracking will start.
        /// </summary>
        /// <param name="device">Device.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="frequency">Frequency.</param>
        /// <param name="startTime">Start time.</param>
        public StatLogProcess(IStatLoggable<T> device, StatLogger logger, float frequency, DateTime? startTime = null) : base($"log_process_{device.Identifier}") {

            m_device = device;
            m_logger = logger;
            Frequency = frequency;
            StartTime = startTime;

            DateTime now = DateTime.Now;

            if (startTime != null) {

                if (StartTime < now) { StartTime?.AddDays(1); }

            }

        }

        public override void Start() {

            if (m_timer != null) { 

                Log.w($"StatLogProcess.Start(): Resetting timer for: {Identifier}");
                Stop();

            }

            if (StartTime == null || StartTime < DateTime.Now) {

                StartLogging();

            } else {

                TimeSpan startTime = (StartTime ?? DateTime.MaxValue) - DateTime.Now;
                m_startTimer = new Timer(startTime.TotalMilliseconds);
                m_startTimer.Elapsed += delegate { StartLogging(); };
                m_startTimer.Enabled = true;
                m_startTimer.AutoReset = false;
                m_startTimer.Start();

            }

        }

        public override void Stop() {

            if (m_timer != null) {

                m_timer.Enabled = false;
                m_timer.Stop();
                m_timer.Dispose();
                m_timer = null;

            } else {

                Log.w($"Trying to dispose StatLogProcess for ''{m_device.Identifier}', but process was not running.");

            }

            StopStartTimer();

        }

        private void StopStartTimer() {

            if (m_startTimer != null) {

                m_startTimer.Stop();
                m_startTimer.Enabled = false;
                m_startTimer.Dispose();
                m_startTimer = null;

            }

        }

        private void StartLogging() {

            StopStartTimer();
            m_timer = new Timer(Frequency);
            m_timer.Elapsed += delegate { LogDevice(); };
            m_timer.AutoReset = true;

            m_timer.Enabled = true;
            m_timer.Start();
            LogDevice();

        }

        private void LogDevice() {

            if (m_timer?.Enabled == true) {

                try { m_logger.Log(m_device); }
                catch (Exception ex) { Log.e(ex.Message, Identifier); }

            }

        }

    }

}
