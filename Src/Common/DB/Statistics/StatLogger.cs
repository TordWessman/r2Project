﻿// This file is part of r2Poject.
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
using System.Collections.Generic;
using System.Linq;
using R2Core.Device;

namespace R2Core.Common {

    /// <summary>
    /// Tool for storing values of devices implementing IStatLoggable.
    /// </summary>
    public class StatLogger : DeviceBase {

        public IStatLoggerDBAdapter Adapter { get; private set; }
        private IDictionary<string, IDevice> m_processes;

        /// <summary>
        /// Userfriendly name added to Description parameter of the StatLogEntry.
        /// </summary>
        /// <value>The device names.</value>
        public IDictionary<string, string> DeviceNames { get; private set; }

        /// <summary>
        /// The types of IStatLoggable that has logging supported.
        /// </summary>
        public readonly Type[] AllowedTypes = { typeof(int), typeof(float), typeof(byte), typeof(double), typeof(long), typeof(string) };

        public StatLogger(string id, IStatLoggerDBAdapter adapter) : base (id) {

            Adapter = adapter;
            m_processes = new Dictionary<string, IDevice>();
            DeviceNames = new Dictionary<string, string>();

        }

        /// <summary>
        /// Logs an entry of a device.
        /// </summary>
        /// <param name="device">Device.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Log<T>(IStatLoggable<T> device) {

            if (!AllowedTypes.Contains(typeof(T))) {

                throw new InvalidOperationException($"Logging a value of type {typeof(T)} is not implemented.");
            
            }

            if (typeof(T) == typeof(string)) {

            } else {

                LogEntry(device.Identifier, Convert.ToDouble(device.Value));

            }

        }

        /// <summary>
        /// Returns a dictionary containing all track entries for the devices specified by ´idesnifiers´.
        /// ´startTime´ and ´endTime´ is the delimiter for the tracking span.
        /// </summary>
        /// <returns>The entries.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IDictionary<string, IEnumerable<StatLogEntry<T>>> GetEntries<T>(IEnumerable<string> identifiers, DateTime? startTime = null, DateTime? endTime = null) {

            IDictionary<string, IEnumerable<StatLogEntry<T>>> entries = new Dictionary<string, IEnumerable<StatLogEntry<T>>>();

            foreach (string identifier in identifiers) {

                if (!entries.ContainsKey(identifier)) {

                    entries[identifier] = new List<StatLogEntry<T>>();

                }

                lock(Adapter) {

                    foreach (StatLogEntry<T> entry in Adapter.GetEntries<T>(identifier)) {

                        if (startTime != null && startTime > entry.Timestamp) { continue; }
                        if (endTime != null && endTime < entry.Timestamp) { continue; }

                        (entries[identifier] as List<StatLogEntry<T>>).Add(new StatLogEntry<T> { 
                            Value = entry.Value,
                            Timestamp = entry.Timestamp,
                            Description = entry.Description,
                            Identifier = entry.Identifier,
                            Id = entry.Id,
                            Name = DeviceNames.ContainsKey(entry.Identifier) ? DeviceNames[entry.Identifier] : entry.Identifier
                        });

                    }

                }

            }

            return entries;

        }

        /// <summary>
        /// Starts tracking a device using ´frequency´ in minutes. 
        /// ´startTime´ defines which time of the day the tracking will commence on.
        /// </summary>
        /// <returns>The tracking process.</returns>
        /// <param name="device">Device.</param>
        /// <param name="frequency">Frequency.</param>
        /// <param name="startTime">Start time.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public StatLogProcess<T> Track<T>(IStatLoggable<T> device, float frequency, DateTime? startTime = null) {

            lock (m_processes) {

                if (m_processes.ContainsKey(device.Identifier)) {

                    RemoveProcess(device.Identifier);

                }

                R2Core.Log.i($"StatLogger starting to track: '{device.Identifier}' using frequency: {frequency} minutes. Start time: {startTime?.ToString() ?? "now" }");

                StatLogProcess<T> process = new StatLogProcess<T>(device, this, (float)(frequency * 60.0 * 1000.0), startTime);
                process.Start();
                m_processes[device.Identifier] = process;
                return process;

            }

        }

        /// <summary>
        /// Removes a device from track processing.
        /// </summary>
        /// <param name="device">Device.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Untrack(IDevice device) {

            lock(m_processes) {

                RemoveProcess(device.Identifier);

            }
           
        }

        public override void Stop() {

            lock (m_processes) {

                foreach (string identifier in new List<string>(m_processes.Keys)) {

                    RemoveProcess(identifier);

                }

            }

        }

        /// <summary>
        /// Get entries for callers unable to provide ´DateTime´ parameters and handle generics.
        /// </summary>
        public IDictionary<string, IEnumerable<StatLogEntry<double>>> GetValues(IEnumerable<string> identifiers, string startDate, string endDate) {

            return GetEntries<double>(identifiers, startDate?.ParseTime(), endDate?.ParseTime());

        }

        public void SetDescription(IDevice device, string description) {

            Adapter.SetDescription(device.Identifier, description);

        }

        private void LogEntry(string identifier, string value) {

            double.TryParse(value, out double result);

            lock (Adapter) {

                Adapter.SaveEntry(new StatLogEntry<double> { Identifier = identifier, Value = result, Timestamp = DateTime.Now, Description = value });

            }

        }

        private void LogEntry(string identifier, double value, string description = null) {

            lock (Adapter) {

                Adapter.SaveEntry(new StatLogEntry<double> { 
                    Identifier = identifier, 
                    Value = value, 
                    Timestamp = DateTime.Now, 
                    Description = description 
               });

            }

        }

        private void RemoveProcess(string identifier) {

            if (m_processes.ContainsKey(identifier)) {

                m_processes[identifier]?.Stop();
                m_processes.Remove(identifier);
                R2Core.Log.i($"Untracking process: '{identifier}'.");

            } else {

                R2Core.Log.w($"Unable to Untrack '{identifier}'. Not registered for tracking.");

            }

        }

    }

}
