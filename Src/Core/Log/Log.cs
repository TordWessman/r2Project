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
using System.Threading;
using R2Core.Device;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace R2Core
{
	
	public class Log : DeviceBase, IMessageLogger {
		
		private List<IMessageLogger> loggers;
		protected static Log instance;

		public LogType LogLevel = LogType.Temp;

		/// <summary>
		/// Define how many rows to print of the stacktrace.
		/// </summary>
		public int MaxStackTrace = 10;

		// Keeps track of all log levels for specific threads.
		private IDictionary<int, LogType> m_threadLogLevels;

		public IEnumerable<ILogMessage> History {

			get {

				return loggers.SelectMany(t => t.History);

			}

		}

		public static void SetLogLevelForCurrentTask(LogType level) {
		
			if (System.Threading.Tasks.Task.CurrentId != null) {

				Instance.m_threadLogLevels [(int)System.Threading.Tasks.Task.CurrentId] = level;

			} else {
			
				Log.w("Can't SetLogLevelForCurrentTask. Not in a Task.");

			}

		}


		public static void Instantiate(string id) {
		
			Log.instance = new Log(id);

		}
		
		public static Log Instance  {

			get {

				return instance; 
				
			}

		}
		
		public Log(string id) : base(id) {

			loggers = new List<IMessageLogger>();
			m_threadLogLevels = new Dictionary<int, LogType>();

		}
		
		public void AddLogger(IMessageLogger logger) {
			
			loggers.Add(logger);
		
		}
	
		public void Write(ILogMessage message) {
			
			if (loggers.Count == 0) {

				throw new InvalidOperationException($"No logger attached for message '{message.Message}' and type '{message.Type}'.");
			
			}

			if (CanWrite(message)) {

				foreach (IMessageLogger logger in loggers) {

					logger.Write(message);

				}

			}

		}

		/// <summary>
		/// Returns true if all conditions are satisfied for writing messages.
		/// </summary>
		/// <returns><c>true</c> if this instance can write the specified message; otherwise, <c>false</c>.</returns>
		/// <param name="message">Message.</param>
		private bool CanWrite(ILogMessage message) {

			return message.Type >= LogLevel && 
					message.Type >= 
						((Task.CurrentId != null && m_threadLogLevels.ContainsKey((int)Task.CurrentId)) ? 
							m_threadLogLevels [(int)Task.CurrentId] : LogType.Temp);

		}

		public void message(object message, string tag = null) {
			
			Log.Instance.Write(new LogMessage(message, LogType.Message, tag));
		
		}

		public void warning(object message, string tag = null) {
		
			Log.Instance.Write(new LogMessage(message, LogType.Warning, tag));
		
		}

		public void error(object message, string tag = null) {
		
			Log.Instance.Write(new LogMessage(message, LogType.Error, tag));
		
		}

		public void temp(object message, string tag = null) {
		
			Log.Instance.Write(new LogMessage(message, LogType.Temp, tag));
		
		}

		/// <summary>
		/// Used to print debug messages
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="tag">Tag.</param>
		public static void d(object message, string tag = null) 
		{

			if (Log.Instance != null) {

				Log.Instance.message(message, tag);
			
			}

		}

		/// <summary>
		/// Used to print warning messages
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="tag">Tag.</param>
		public static void w(object message, string tag = null)  
		{
			if (Log.Instance != null) {

				Log.Instance.warning(message, tag);	
			}

		}

		/// <summary>
		/// Used to print error messages
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="tag">Tag.</param>
		public static void e(object message, string tag = null) 
		{

			if (Log.Instance != null) {

				Log.Instance.error(message, tag);

			}

		}

		/// <summary>
		/// Used for temporary testing outprint
		/// </summary>
		/// <param name="message">Message.</param>
		public static void t(object message) {
			if (Log.Instance != null) {

				Log.Instance.temp(message);

			}

		}

		/// <summary>
		/// Used to print error message using an Exception object as input
		/// </summary>
		/// <param name="ex">Ex.</param>
		/// <param name="recursionCount">Recursion count.</param>
		public static void x(Exception ex, int recursionCount = 0) {

			if (!string.IsNullOrEmpty(ex.Message)) {
				
				IList<string> stackTrace = ex.StackTrace?.Split('\n').ToList() ?? new List<string>();

				if (stackTrace.Count > Log.instance.MaxStackTrace) {

					stackTrace = stackTrace.Take(Log.instance.MaxStackTrace).ToList();
					stackTrace.Add("... (Ignoring the rest) ...");

				}

				string stackTraceString = stackTrace.Aggregate("", (current, next) => current + "\n" + next);
				string exString = 
					new string('-', recursionCount * 2) + "--" + ex.ToString() + "--" + "\n" +
					new string('-', recursionCount * 2) + ex.Source + "\n" +
					new string('-', recursionCount * 2) + ex.Message + "\n" +
					new string('-', recursionCount * 2) + stackTraceString + "\n";

				Log.Instance.Write(new LogMessage(exString, LogType.Error, null));

			}
			
			if (ex.InnerException != null && recursionCount < 10) {

				Log.Instance.Write(new LogMessage("==== Inner Exception ====", LogType.Error));
				x(ex.InnerException, recursionCount + 1);
			
			}
		
		}

		~Log() { instance = null; }

	}

}

