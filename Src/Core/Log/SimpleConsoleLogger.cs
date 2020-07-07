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
using System.Collections.Generic;
using System.Linq;

namespace R2Core
{
    /// <summary>
    /// Simplified version of ´ConsoleLogger´. It just writes stuff to stdio
    /// </summary>
    public class SimpleConsoleLogger : DeviceBase, IMessageLogger {

        private readonly ConsoleColor m_defaultColor;
        private readonly Stack<ILogMessage> m_history;
        private readonly int m_maxHistory;

        public LogLevel LogLevel { get; set; } = LogLevel.Message;

        public const int DefaultMaxHistory = 100;

		/// <summary>
		/// Use the maxHistory to define how many messages that can be contain in the message history.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="maxHistory">Max history.</param>
		public SimpleConsoleLogger(string id, int maxHistory = DefaultMaxHistory) : base(id) {

			m_defaultColor = Console.ForegroundColor;
			m_history = new Stack<ILogMessage>();
			m_maxHistory = maxHistory;

		}

		public IEnumerable<ILogMessage> History  => m_history.Reverse().Select(t => t);

		public void Write(ILogMessage message) {
			
			m_history.Push(message);

			try {
				
				NotifyChange(message);
			
			} catch (Exception ex) {
			
				_Write(new LogMessage("Logger could not notify observers: " + ex.Message + " stacktrace: " + ex.StackTrace, LogLevel.Error));

			}

			_Write(message);

		}

		private void _Write(ILogMessage message) {
		
			try {

				if (Console.OpenStandardOutput().CanWrite) {

					SetConsoleColor(message.Type);

					Console.WriteLine((message.Tag != null ? "[" + message.Tag + "] " : "") + "[" +  message.TimeStamp + "] " + message.Message);

					SetConsoleColor(m_defaultColor);

				}

			} catch (NotSupportedException) { /* Indicates that the logger is being closed. */ }

		}

		private void SetConsoleColor(ConsoleColor color) {

			if (Console.OpenStandardOutput().CanWrite) {
				
				Console.ForegroundColor = color;
			
			}
		
		}

		private void SetConsoleColor(LogLevel logType) {

			switch (logType) {

			case LogLevel.Error:

				SetConsoleColor(ConsoleColor.Red);
				break;

			case LogLevel.Warning:

				SetConsoleColor(ConsoleColor.Yellow);
				break;

			case LogLevel.Temp:

				SetConsoleColor(ConsoleColor.Green);
				break;

			case LogLevel.Message:

				SetConsoleColor(ConsoleColor.Gray);
				break;

            case LogLevel.Info:
                
                SetConsoleColor(ConsoleColor.DarkBlue);
                break;

            }
		
		}
	
	}

}