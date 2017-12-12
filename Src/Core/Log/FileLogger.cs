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
//
using System;
using Core.Device;
using System.Collections.Generic;
using System.IO;

namespace Core
{
	public class FileLogger: DeviceBase, IMessageLogger
	{

		private StreamWriter m_outputStream;
		private FileStream m_fs;

		private string m_fileName;
		private readonly object m_lock = new object(); 

		public FileLogger (string id, string path) : base (id)
		{

			m_fs = File.Open(path, FileMode.Create, FileAccess.ReadWrite);
			m_outputStream = new StreamWriter (m_fs);
			m_outputStream.AutoFlush = true;
			m_fileName = path;

		}

		~FileLogger() {

			m_fs.Close();

		}

		public void Write(ILogMessage message) {
		
			lock (m_lock) {

				m_outputStream.WriteLine($"[{message.Type}] [{message.TimeStamp}] : {message.Message} ");

			}

		}

		public IEnumerable<ILogMessage> History { 
		
			get {

				yield return new LogMessage ("History not allowed for FileLogger (not implemented)", LogType.Error);

			}
			 
		}

		private ILogMessage ParseLog(string line) {
		
			return new LogMessage (line, LogType.Temp);

		}

	}
}

