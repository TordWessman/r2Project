﻿// This file is part of r2Poject.
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
//
using System;

namespace Core
{
	/// <summary>
	/// Simple log message
	/// </summary>
	public class LogMessage : ILogMessage {

		private object m_message;
		private LogType m_type;
		private string m_tag;
		private DateTime m_creationTime;

		public LogMessage(object message, LogType type, string tag = null) {

			m_message = message;
			m_type = type;
			m_tag = tag;

			m_creationTime = DateTime.Now;

		}

		public object Message { get { return m_message; } }

		public LogType Type { get { return m_type; } }

		public string Tag { get { return m_tag; } }

		public DateTime TimeStamp { get { return m_creationTime; } } 

	}
}

