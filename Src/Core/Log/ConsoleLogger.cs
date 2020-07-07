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
using R2Core.Device;
using System.Collections.Generic;
using System.Timers;
using System.Linq;

namespace R2Core
{
	public class ConsoleLogger : DeviceBase, IMessageLogger {
		
		private readonly ConsoleColor m_defaultColor;
		private static readonly object m_lock = new object();
		private Queue<Action> m_queue;
		private Timer m_consoleTimer;
		private bool m_isPrinting;
		private Stack<ILogMessage> m_history;

        public LogLevel LogLevel { get; set; } = LogLevel.Message;

        public bool Paused;

		public ConsoleLogger(string id) : base(id) {

			m_defaultColor = Console.ForegroundColor;
			m_queue = new Queue<Action>();
			m_isPrinting = false;
			m_history = new Stack<ILogMessage>();

			Paused = false;

		}

		public IEnumerable<ILogMessage> History {

			get {

				return m_history.Reverse().Select(t => t);
		
			}
		
		}

		public override void Start() {

			if (m_consoleTimer != null) {

				m_consoleTimer.Start();

			} else {

				m_isPrinting = false;
				m_consoleTimer = new Timer(200);
				m_consoleTimer.AutoReset = true;
				m_consoleTimer.Enabled = true;
				m_consoleTimer.Elapsed += PrintQueue;

			}

		}

		public void Pause() {

			m_consoleTimer?.Stop();

		}

		private void PrintQueue(Object source, ElapsedEventArgs e) {

			if (!m_isPrinting) {

				m_isPrinting = true;
			
				while(m_queue.Count > 0 && !Paused) {

					try {

						m_queue.Dequeue().Invoke();

					} catch (Exception ex) {

						Console.WriteLine("ERROR WRITING TO CONSOLE: " + ex.Message);

					}

				}

				m_isPrinting = false;

			}
					
		}

		public void Write(ILogMessage message) {

			lock(m_lock) {

				m_history.Push(message);

				m_queue.Enqueue(() => {

					try {

						NotifyChange(message);

					} catch (Exception ex) {

						_Write(new LogMessage("Logger could not notify observers: " + ex.Message + " stacktrace: " + ex.StackTrace, LogLevel.Error));

					}

					_Write(message);

				});

			}

		}
			
		private void _Write(ILogMessage message) {

			if (Console.OpenStandardOutput().CanWrite) {

				SetConsoleColor(message.Type);

				Console.WriteLine((message.Tag != null ? $"[{message.Tag }] " : "") + message.Message);

				SetConsoleColor(m_defaultColor);

			}

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

