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
using System.Globalization;

namespace R2Core.Common {

    /// <summary>
    /// Enables non-generic logging.
    /// </summary>
    public static class StatLoggerExtensions {

        /// <summary>
        /// Log a ´float´ value.
        /// </summary>
        public static void Log(this StatLogger self, IStatLoggable<float> device) { self.Log<float>(device); }

        /// <summary>
        /// Log an ´int´ value.
        /// </summary>
        public static void Log(this StatLogger self, IStatLoggable<int> device) { self.Log<int>(device); }

        /// <summary>
        /// Log a ´double´ value.
        /// </summary>
        public static void Log(this StatLogger self, IStatLoggable<double> device) { self.Log<double>(device); }

        /// <summary>
        /// Log a ´byte´ value.
        /// </summary>
        public static void Log(this StatLogger self, IStatLoggable<byte> device) { self.Log<byte>(device); }

        /// <summary>
        /// Log a ´long´ value.
        /// </summary>
        public static void Log(this StatLogger self, IStatLoggable<long> device) { self.Log<long>(device); }

        /// <summary>
        /// Log a ´string´ value.
        /// </summary>
        public static void Log(this StatLogger self, IStatLoggable<string> device) { self.Log<string>(device); }

        /// <summary>
        /// Track using ´startTime´ as an input string. Usable by callers that can't provide DateTime parameters.
        /// </summary>
        public static StatLogProcess<T> TrackFrom<T>(this StatLogger self, IStatLoggable<T> device, int frequency, string startTime) {

            return self.Track(device, frequency, startTime.ParseTime());

        }

    }

    public static class StatLoggerDateTimeExtensions {

        /// <summary>
        /// The string representation of (the least granular) StatLogger date format.
        /// </summary>
        public static string ToStatLoggerString(this DateTime self) {

            return self.ToString(StatLoggerDateParsingExtensions.GetStatLoggerDateFormat());

        }

    }

    public static class StatLoggerDateParsingExtensions {

        /// <summary>
        /// The DateFormat the StatLogger uses to represent a date with the least precission.
        /// </summary>
        /// <returns>The stat logger date format.</returns>
        public static string GetStatLoggerDateFormat() { return "yyyy-MM-dd"; }

        /// <summary>
        /// Gets the available time formats which ´string.ParseTime()´ uses.
        /// </summary>
        /// <returns>The available time formats.</returns>
        public static string[] GetAvailableTimeFormats() {

            return new string[] {
                SqlExtensions.SqliteDateFormat(), GetStatLoggerDateFormat(),
                "HH:mm:ss fff", "HH:mm:ss ff", "HH:mm:ss f",
                "HH:mm:ss", "HH:mm", "HH"
            };

        }

        /// <summary>
        /// Tries a discrete set of formats defined by ´GetAvailableTimeFormats()´ to parse ´this´ into a ´DateTime´.
        /// Returns ´null´ if unable to parse.
        /// </summary>
        public static DateTime? ParseTime(this string self) {

            if (self.Trim().Length == 0) { return null; }

            foreach (string format in GetAvailableTimeFormats()) {

                DateTime date;

                if (DateTime.TryParseExact(self, format, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out date)) {

                    return date;

                }
            
            }

            Log.w($"Unable to parse '{self}' into a valid ´DateTime´. Check ´DateParsingExtensions.GetAvailableTimeFormats()´ for a list of valid formats.");
            return null;

        }

    }

}
