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
using System.Timers;
using R2Core;
using R2Core.Device;

namespace MainFrame {

        public class MemoryChecker : DeviceBase {

        private Timer m_timer;
        public int Frequency = 1000 * 60 * 5;

        public MemoryChecker(int frequency = -1) : base("memory_checker") {

            if (frequency > 0) { Frequency = frequency; }

        }

        public void Check() {

            Log.d($" - Memmory used: {GC.GetTotalMemory(true)}");

        }

        public override void Start() {
            base.Start();

            m_timer = new Timer(Frequency);
            m_timer.Elapsed += delegate { Check(); };
            m_timer.AutoReset = true;

            m_timer.Enabled = true;
            m_timer.Start();

        }

        public override void Stop() {

            if (m_timer != null) {

                m_timer.Enabled = false;
                m_timer.Stop();
                m_timer.Dispose();
                m_timer = null;

            }

        }

    }
}
