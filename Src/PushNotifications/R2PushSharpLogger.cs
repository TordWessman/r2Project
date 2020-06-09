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
using PushSharp.Core;

namespace R2Core.PushNotifications {

    /// <summary>
    /// Wraps a PushSharps ILogger instance.
    /// </summary>
    public class R2PushSharpLogger : ILogger {

        private readonly IMessageLogger m_logger;
        private const string PushSharpLogTag = "PushSharp";

        public R2PushSharpLogger(IMessageLogger logger) {

            m_logger = logger;

        }

        public void Write(PushSharp.Core.LogLevel level, string msg, params object[] args) {

            m_logger.Write(new LogMessage(msg, Convert(level), PushSharpLogTag));

        }

        public LogLevel Convert(PushSharp.Core.LogLevel level) {

            switch(level) {

                case PushSharp.Core.LogLevel.Debug:
                    return LogLevel.Message;
                case PushSharp.Core.LogLevel.Info:
                    return LogLevel.Info;
                case PushSharp.Core.LogLevel.Error:
                    return LogLevel.Warning;

            }

            throw new ArgumentException($"WTF! Unable to convert log level: {level}");

        }

    }
}
