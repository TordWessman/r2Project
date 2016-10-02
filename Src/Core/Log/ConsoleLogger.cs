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
using Core.Device;
using System.Collections.Generic;
using System.Timers;

namespace Core
{
public class ConsoleLogger : DeviceBase, IMessageLogger
	{
		private ConsoleColor m_defaultColor;
		private static readonly object m_lock = new object();
		private Queue<Action> m_queue;
		private Timer m_consoleTimer;
		private bool m_isPrinting;

		public bool Paused;

		public ConsoleLogger (string id) : base (id)
		{

			m_defaultColor = Console.ForegroundColor;
			m_queue = new Queue<Action> ();
			m_isPrinting = false;
			Paused = false;

		}

		public override void Start ()
		{

			if (m_consoleTimer != null) {

				m_consoleTimer.Start ();

			} else {

				m_isPrinting = false;
				m_consoleTimer = new Timer (200);
				m_consoleTimer.AutoReset = true;
				m_consoleTimer.Enabled = true;
				m_consoleTimer.Elapsed += PrintQueue;

			}

		}

		public void Pause ()
		{
			if (m_consoleTimer != null) {
				m_consoleTimer.Stop ();
			}
		}

		private void PrintQueue(Object source, ElapsedEventArgs e) {

			if (!m_isPrinting) {

				m_isPrinting = true;
			
				while (m_queue.Count > 0 && !Paused) {

					try {

						m_queue.Dequeue ().Invoke();

					} catch (Exception ex) {

						Console.WriteLine ("ERROR WRITING TO CONSOLE: " + ex.Message);

					}

				}

				m_isPrinting = false;

			}
					
		}

		public void Write(string message, LogTypes logType, string tag) 
		{
			lock (m_lock) {

				if (tag == null) {

					m_queue.Enqueue (() => {

						Write (message, logType, false);

					});

				} else {

					m_queue.Enqueue (() => {

						Write("[" + tag + "] " + message, logType, false);

					});
				}

			}

		}
		
		public void WriteLine (string message, LogTypes logType, string tag)
		{
			
			lock (m_lock) {

				if (tag == null) {

					m_queue.Enqueue (() => {

						Write (message, logType, true);

					});

				} else {

					m_queue.Enqueue (() => {

						Write("[" + tag + "] " + message, logType, true);

					});
				}
				

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

