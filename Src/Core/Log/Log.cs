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

using System.Collections.Generic;
using System;
using R2Core.Device;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;

namespace R2Core
{
	
	public class Log : DeviceBase, IMessageLogger {
		
		private List<IMessageLogger> m_loggers;

		private static Log m_instance;

        private LogLevel m_logLevel = LogLevel.Message;

        /// <summary>
        /// The minimum visible log level.
        /// </summary>
		public LogLevel LogLevel {

            set {

                m_logLevel = value;

                foreach (IMessageLogger logger in m_loggers) {

                    logger.LogLevel = value;

                }

            }

            get { return m_logLevel;  }
        
        }

        /// <summary>
        /// Define how many rows to print of the stacktrace.
        /// </summary>
        public int MaxStackTrace = 10;

        /// <summary>
        /// If ´false´, messages of type Temp will not be printed.
        /// </summary>
        public bool PrintoutTempMessages = true;

		// Keeps track of all log levels for specific threads.
		private IDictionary<int, LogLevel> m_threadLogLevels;

		public IEnumerable<ILogMessage> History {

			get {

				return m_loggers.SelectMany(t => t.History);

			}

		}

		public static void SetLogLevelForCurrentTask(LogLevel level) {
		
			if (Task.CurrentId != null) {

				Instance.m_threadLogLevels[(int)System.Threading.Tasks.Task.CurrentId] = level;

			} else {
			
				w("Can't SetLogLevelForCurrentTask. Not in a Task.");

			}

		}


		public static Log Instantiate(string id) {
		
			m_instance = new Log(id);
			return m_instance;

		}
		
		public static Log Instance  {

			get {

				return m_instance; 
				
			}

		}
		
		public Log(string id) : base(id) {

			m_loggers = new List<IMessageLogger>();
			m_threadLogLevels = new Dictionary<int, LogLevel>();

		}
		
		public void AddLogger(IMessageLogger logger) {
			
			m_loggers.Add(logger);
		
		}
	
		public void Write(ILogMessage message) {
			
			if (m_loggers.Count == 0) {

				throw new InvalidOperationException($"No logger attached for message '{message.Message}' and type '{message.Type}'.");
			
			}

			if (CanWrite(message)) {

				foreach (IMessageLogger logger in m_loggers) {

                    if (message.Type >= logger.LogLevel) {

                        logger.Write(message);
                    
                    }
                   
				}

			}

		}

		/// <summary>
		/// Returns true if all conditions are satisfied for writing messages.
		/// </summary>
		/// <returns><c>true</c> if this instance can write the specified message; otherwise, <c>false</c>.</returns>
		/// <param name="msg">Message.</param>
		private bool CanWrite(ILogMessage msg) {

			return msg.Type >= 
						((Task.CurrentId != null && m_threadLogLevels.ContainsKey((int)Task.CurrentId)) ? 
							m_threadLogLevels[(int)Task.CurrentId] : LogLevel.Info)
                        && (PrintoutTempMessages || (!PrintoutTempMessages && msg.Type != LogLevel.Temp));

		}

		private string FormatMessage(object msg, int depth = 0) {
		
			string output = msg?.ToString();

			if (!(msg is string) && msg is IEnumerable) {
				
				output = "";

				foreach (object value in (msg as IEnumerable)) {

					output += new String('-', depth + 1) + $" {FormatMessage(value, depth + 1)}{Environment.NewLine}";

				}

			}

			return output;

		}

        public void info(object msg, string tag = null) {

            Instance?.Write(new LogMessage(FormatMessage(msg), LogLevel.Info, tag));

        }

        public void message(object msg, string tag = null) {
			
			Instance?.Write(new LogMessage(FormatMessage(msg), LogLevel.Message, tag));
		
		}

		public void warning(object msg, string tag = null) {
		
			Instance?.Write(new LogMessage(FormatMessage(msg), LogLevel.Warning, tag));
		
		}

		public void error(object msg, string tag = null) {
		
			Instance?.Write(new LogMessage(FormatMessage(msg), LogLevel.Error, tag));
		
		}

		public void temp(object msg, string tag = null) {
		
			Instance?.Write(new LogMessage(FormatMessage(msg), LogLevel.Temp, tag));
		
		}

        /// <summary>
        /// Log info message (low priority and defaults to be disabled).
        /// </summary>
        /// <param name="msg">Message.</param>
        /// <param name="tag">Tag.</param>
        public static void i(object msg, string tag = null) {

            Instance?.info(msg, tag);

        }

        /// <summary>
        /// Used to print debug messages
        /// </summary>
        /// <param name="msg">Message.</param>
        /// <param name="tag">Tag.</param>
        public static void d(object msg, string tag = null) {
			
			Instance?.message(msg, tag);

		}

		/// <summary>
		/// Used to print warning messages
		/// </summary>
		/// <param name="msg">Message.</param>
		/// <param name="tag">Tag.</param>
		public static void w(object msg, string tag = null) {
			
			Instance?.warning(msg, tag);	
		
		}

		/// <summary>
		/// Used to print error messages
		/// </summary>
		/// <param name="msg">Message.</param>
		/// <param name="tag">Tag.</param>
		public static void e(object msg, string tag = null) {

			Instance?.error(msg, tag);

		}

		/// <summary>
		/// Used for temporary testing outprint
		/// </summary>
		/// <param name="msg">Message.</param>
		public static void t(object msg, string tag = null) {
			
			Instance?.temp(msg, tag);

		}

		/// <summary>
		/// Used to print error message using an Exception object as input
		/// </summary>
		/// <param name="ex">Ex.</param>
		/// <param name="recursionCount">Recursion count.</param>
		public static void x(Exception ex, string tag = null, int recursionCount = 0) {

			if (!string.IsNullOrEmpty(ex.Message)) {

                IList<string> stackTrace = ex.StackTrace?.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList() ?? new List<string>();

				if (stackTrace.Count > (Instance?.MaxStackTrace ?? 0)) {

					stackTrace = stackTrace.Take(Instance?.MaxStackTrace ?? 10).ToList();
					stackTrace.Add("... (Ignoring the rest) ...");

				}

				string stackTraceString = stackTrace.Aggregate("", (current, next) => current + Environment.NewLine + next);
				string exString = 
					new string('-', recursionCount * 2) + $"--{ex}--{Environment.NewLine}" +
					new string('-', recursionCount * 2) + ex.Source + Environment.NewLine +
					new string('-', recursionCount * 2) + ex.Message + Environment.NewLine +
					new string('-', recursionCount * 2) + stackTraceString + Environment.NewLine;

				Instance.Write(new LogMessage(exString, LogLevel.Error, tag));

			}
			
			if (ex.InnerException != null && recursionCount < 10) {

				Instance.Write(new LogMessage("==== Inner Exception ====", LogLevel.Error));
				x(ex.InnerException, tag, recursionCount + 1);
			
			}
		
		}

		~Log() { m_instance = null; }

	}

}

