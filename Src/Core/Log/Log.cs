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

namespace Core
{
	public class Log: DeviceBase, IMessageLogger
	{
		private List<IMessageLogger> loggers;
		protected static Log instance;
		protected bool _showThreadId;
		
		
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
		
		
		public void Write (string message,Core.LogTypes logType, string tag)
		{

			foreach (IMessageLogger logger in loggers) {

				logger.WriteLine(message, logType, tag);
			
			}
				
		}
	
		public void WriteLine (string message, Core.LogTypes logType, string tag)
		{

			if (_showThreadId) {

				message += "[" + Thread.CurrentThread.ManagedThreadId + "] ";

			}
				
			
			if (loggers.Count == 0) {

				throw new InvalidOperationException ("No logger attached for message: " + message + " and type: " + logType);
			
			}
			
			foreach (IMessageLogger logger in loggers) {
			
				logger.WriteLine(message, logType, tag);

			}
				
		}

		/// <summary>
		/// Used to print debug messages
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="tag">Tag.</param>
		public void debug(string message) 
		{

			Log.d (message);

		}

		/// <summary>
		/// Used to print warning messages
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="tag">Tag.</param>
		public void warn(string message) 
		{

			Log.w (message);

		}

		/// <summary>
		/// Used to print error messages
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="tag">Tag.</param>
		public void error(string message) 
		{

			Log.e (message);

		}

		/// <summary>
		/// Used for temporary testing outprint
		/// </summary>
		/// <param name="message">Message.</param>
		public void ok(string message) 
		{

			Log.t (message);

		}

		/// <summary>
		/// Used to print debug messages
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="tag">Tag.</param>
		public static void d(string message) 
		{

			Log.d (message, null);
		
		}

		/// <summary>
		/// Used to print warning messages
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="tag">Tag.</param>
		public static void w(string message) 
		{

			Log.w (message, null);
		
		}

		/// <summary>
		/// Used to print error messages
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="tag">Tag.</param>
		public static void e(string message) 
		{

			Log.e (message, null);
		
		}

		/// <summary>
		/// Used to print debug messages
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="tag">Tag.</param>
		public static void d(string message, object tag) 
		{

			if (Log.Instance != null) {

				Log.Instance.WriteLine(message, LogTypes.Message, tag != null ? tag.ToString() : null);	
			
			}

		}

		/// <summary>
		/// Used to print warning messages
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="tag">Tag.</param>
		public static void w(string message, object tag)  
		{
			if (Log.Instance != null) {

				Log.Instance.WriteLine (message, LogTypes.Warning, tag != null ? tag.ToString () : null);	
			}

		}

		/// <summary>
		/// Used to print error messages
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="tag">Tag.</param>
		public static void e(string message, object tag) 
		{

			if (Log.Instance != null) {

				Log.Instance.WriteLine (message, LogTypes.Error, tag != null ? tag.ToString () : null);

			}

		}

		/// <summary>
		/// Used for temporary testing outprint
		/// </summary>
		/// <param name="message">Message.</param>
		public static void t (string message)
		{
			if (Log.Instance != null) {

				Log.Instance.WriteLine (message, LogTypes.Temp, null);

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

				Log.Instance.WriteLine (exString, LogTypes.Error, null);

			}
			
			if (ex.InnerException != null && recursionCount < 10) {

				Log.Instance.WriteLine ("==== Inner Exception ====", LogTypes.Error, null);
				x (ex.InnerException, recursionCount + 1);
			
			}
		
		}
		
		public static bool ShowThreadId 
		{

			set {

				Log.instance._showThreadId = value;
			}

		}
		
		~Log() 
		{

			instance = null;
		
		}

	}

}

