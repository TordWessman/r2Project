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
using Core.Device;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq;

namespace Core
{
	public class Log: DeviceBase, IMessageLogger
	{
		private List<IMessageLogger> loggers;
		protected static Log instance;

		public IEnumerable<ILogMessage> History {

			get {

				return loggers.SelectMany (t => t.History);

			}

		}
		
		public static Log Instance 
		{
			get 
			{

				return instance; 
				
			}

		}
		
		public Log (string id): base (id)
		{

			loggers = new List<IMessageLogger>();
		
		}
		
		public void AddLogger(IMessageLogger logger) 
		{
			Log.instance = this;

			loggers.Add(logger);
		
		}
	
		public void Write (ILogMessage message)
		{
			
			if (loggers.Count == 0) {

				throw new InvalidOperationException ("No logger attached for message: " + message.Message + " and type: " + message.Type);
			
			}
			
			foreach (IMessageLogger logger in loggers) {
			
				logger.Write(message);

			}
				
		}

		public void message(object message, string tag = null) {
			Log.Instance.Write (new LogMessage (message, LogType.Message, tag));
		}
		public void warning(object message, string tag = null) {
			Log.Instance.Write (new LogMessage (message, LogType.Warning, tag));
		}
		public void error(object message, string tag = null) {
			Log.Instance.Write (new LogMessage (message, LogType.Error, tag));
		}
		public void temp(object message, string tag = null) {
			Log.Instance.Write (new LogMessage (message, LogType.Temp, tag));
		}

		/// <summary>
		/// Used to print debug messages
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="tag">Tag.</param>
		public static void d(object message, string tag = null) 
		{

			if (Log.Instance != null) {

				Log.Instance.message (message, tag);
			
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

				Log.Instance.warning (message, tag);	
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

				Log.Instance.error (message, tag);

			}

		}

		/// <summary>
		/// Used for temporary testing outprint
		/// </summary>
		/// <param name="message">Message.</param>
		public static void t (object message)
		{
			if (Log.Instance != null) {

				Log.Instance.temp (message);

			}

		}

		/// <summary>
		/// Used to print error message using an Exception object as input
		/// </summary>
		/// <param name="ex">Ex.</param>
		/// <param name="recursionCount">Recursion count.</param>
		public static void x (Exception ex, int recursionCount = 0)
		{

			if (!string.IsNullOrEmpty (ex.Message)) {

				string exString = 
					new string ('-', recursionCount * 2) + ex.ToString () + "\n" +
					new string ('-', recursionCount * 2) + ex.Message + "\n" +
					new string ('-', recursionCount * 2) + ex.StackTrace + "\n" +
					new string ('-', recursionCount * 2) + ex.Source + "\n";

				Log.Instance.Write (new LogMessage (exString, LogType.Error, null));

			}
			
			if (ex.InnerException != null && recursionCount < 10) {

				Log.Instance.Write (new LogMessage("==== Inner Exception ====", LogType.Error));
				x (ex.InnerException, recursionCount + 1);
			
			}
		
		}

		~Log() 
		{

			instance = null;
		
		}

	}

}

