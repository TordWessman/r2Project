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
using System.Collections.Generic;

namespace R2Core
{
	public enum LogType {
		Message,
		Warning,
		Error,
		Temp
	}

	/// <summary>
	/// Represents a message to be printed.
	/// </summary>
	public interface ILogMessage {
	
		/// <summary>
		/// The message to be printed.
		/// </summary>
		/// <value>The message.</value>
		object Message { get; }

		/// <summary>
		/// Type of message
		/// </summary>
		/// <value>The type.</value>
		LogType Type { get; }

		/// <summary>
		/// Optional tag for the message.
		/// </summary>
		/// <value>The tag.</value>
		string Tag { get; }

		/// <summary>
		/// Time of log message creation
		/// </summary>
		/// <value>The time stamp.</value>
		DateTime TimeStamp { get; }

	}

	public interface IMessageLogger
	{
		/// <summary>
		/// Writes message to the logger.
		/// </summary>
		/// <param name="message">Message.</param>
		void Write(ILogMessage message);

		/// <summary>
		/// Returns output history (descending age) 
		/// </summary>
		/// <returns>The history.</returns>
		/// <param name="count">Maximum number of output messages.</param>
		IEnumerable<ILogMessage> History { get; } 

	}
}

