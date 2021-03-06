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
using R2Core.Device;
using System.Collections.Generic;
using System.IO;

namespace R2Core
{
	public class FileLogger : DeviceBase, IMessageLogger {

		private StreamWriter m_outputStream;
		private FileStream m_fs;

		private readonly string m_fileName;
		private readonly object m_lock = new object();

        public LogLevel LogLevel { get; set; } = LogLevel.Message;

        public FileLogger(string id, string path) : base(id) {

			m_fs = File.Open(path, FileMode.Create, FileAccess.ReadWrite);
            m_outputStream = new StreamWriter(m_fs) {
                AutoFlush = true
            };
            m_fileName = path;

		}

		~FileLogger() {

			lock (m_lock) {
				
				m_fs.Close ();
				m_outputStream = null;
			
			}
		
		}

		public void Write(ILogMessage message) {
		
			lock(m_lock) {

				int id = System.Threading.Tasks.Task.CurrentId ?? 0;

				try {

					m_outputStream?.WriteLine($"[{id}]{(message.Tag == null ? "" : $"[{message.Tag}]")}[{message.Type}] [{message.TimeStamp} {message.TimeStamp.Millisecond}] : {message.Message} ");

				} catch (ObjectDisposedException) { /* The stream has bet shut down due to application exit. */ }

			}

		}

		public IEnumerable<ILogMessage> History { 
		
			get {

				yield return new LogMessage("History not allowed for FileLogger (not implemented)", LogLevel.Error);

			}
			 
		}

		private ILogMessage ParseLog(string line) {
		
			return new LogMessage(line, LogLevel.Temp);

		}

	}

}

