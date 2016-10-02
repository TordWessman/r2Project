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

namespace Core
{
	public class SimpleConsoleLogger : DeviceBase, IMessageLogger
	{
		private ConsoleColor m_defaultColor;

		public SimpleConsoleLogger (string id) : base (id)
		{
			m_defaultColor = Console.ForegroundColor;
		}

		public void Write(string message, LogTypes logType, string tag) 
		{
					if (tag == null) {
				
						Write (message, logType, false);


				} else {
				
						Write("[" + tag + "] " + message, logType, false);
				}


		}

		public void WriteLine (string message, LogTypes logType, string tag)
		{
			if (tag == null) {

						Write (message, logType, true);

				} else {
				
						Write("[" + tag + "] " + message, logType, true);
				}

					}

		private void Write (string msg, LogTypes type, bool line = true ) {

			if (Console.OpenStandardOutput ().CanWrite) {

				SetConsoleColor(type);

				if (line) {
					Console.WriteLine (msg);
				} else {
					Console.Write (msg);
				}

				SetConsoleColor( m_defaultColor);
			}

		}

		private void SetConsoleColor (ConsoleColor color) {

			if (Console.OpenStandardOutput ().CanWrite) {
				Console.ForegroundColor = color;
			}
		}

		private void SetConsoleColor (LogTypes logType)
		{

			if (logType == LogTypes.Error)
				SetConsoleColor( ConsoleColor.Red);
			else if (logType == LogTypes.Warning)
				SetConsoleColor( ConsoleColor.Yellow);
			else if (logType == LogTypes.Temp)
				SetConsoleColor( ConsoleColor.Green);
			else
				SetConsoleColor( ConsoleColor.Gray);

		}
	}
}

