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
using System.Collections.Generic;
using System.Linq;

namespace Core
{
	public class SimpleConsoleLogger : DeviceBase, IMessageLogger
	{
		private ConsoleColor m_defaultColor;
		private Stack<ILogMessage> m_history;
		private int m_maxHistory;

		/// <summary>
		/// Use the maxHistory to define how many messages that can be contain in the message history.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="maxHistory">Max history.</param>
		public SimpleConsoleLogger (string id, int maxHistory) : base (id)
		{
			m_defaultColor = Console.ForegroundColor;
			m_history = new Stack<ILogMessage> ();
			m_maxHistory = maxHistory;
		}

		public IEnumerable<ILogMessage> History {
		
			get {
		
				return m_history.Reverse ().Select (t => t);

			}
		
		}

		public void Write (ILogMessage message) {
			
			m_history.Push (message);

			try {
				
				NotifyChange (message);
			
			} catch (Exception ex) {
			
				_Write (new LogMessage ("Logger could not notify observers: " + ex.Message + " stacktrace: " + ex.StackTrace, LogType.Error));

			}

			_Write (message);

		}

		private void _Write (ILogMessage message) {
		
			if (Console.OpenStandardOutput ().CanWrite) {

				SetConsoleColor(message.Type);

				Console.WriteLine ((message.Tag != null ? "[" + message.Tag + "] " : "") + message.Message);

				SetConsoleColor( m_defaultColor);

			}

		}

		private void SetConsoleColor (ConsoleColor color) {

			if (Console.OpenStandardOutput ().CanWrite) {
				
				Console.ForegroundColor = color;
			
			}
		
		}

		private void SetConsoleColor (LogType logType)
		{

			if (logType == LogType.Error) {
		
				SetConsoleColor (ConsoleColor.Red);
			
			} else if (logType == LogType.Warning) {
			
				SetConsoleColor (ConsoleColor.Yellow);
			
			} else if (logType == LogType.Temp) {
			
				SetConsoleColor (ConsoleColor.Green);
			
			} else {
			
				SetConsoleColor (ConsoleColor.Gray);
			
			}
		
		}
	
	}

}