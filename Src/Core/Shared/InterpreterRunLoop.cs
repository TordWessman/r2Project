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
using Core.Scripting;
using System.Collections.Generic;

namespace Core
{
	
	public class InterpreterRunLoop : DeviceBase, IRunLoop
	{
		private IScript m_interpreterScript;
		private bool m_shouldRun;
		private IList<string> m_history;
		private int m_historyPosition;

		public InterpreterRunLoop (string id, IScript script) : base (id) {
		
			m_interpreterScript = script;
			m_history = new List<string> ();

		}

		public override void Start () {

			m_shouldRun = true;

			string line = "";

			do {
				
				ConsoleKeyInfo key = Console.ReadKey (true);

				if (key.Key == ConsoleKey.Escape) {
					
					m_shouldRun = false;
				
				} else if (key.Key == ConsoleKey.UpArrow && m_historyPosition > 0) {

					ClearLine (line);
					m_historyPosition--;
					line = m_history[m_historyPosition];
					PrintLine(line);

				} else if (key.Key == ConsoleKey.DownArrow ) {

					ClearLine (line);

					if (m_historyPosition < m_history.Count - 1) {
						
						m_historyPosition++;
						line = m_history[m_historyPosition];
					
					} else if (m_historyPosition == m_history.Count - 1) {

						line = "";
					
					}
					
					PrintLine(line);

				} else if (key.Key == ConsoleKey.Backspace && line.Length > 0) {

					ClearLine (line);
					line = line.Substring(0,line.Length - 1);
					PrintLine(line);

				} else if (key.Key == ConsoleKey.Enter && line.Trim().Length > 0) {

					line = line.Trim();

					ClearLine(line);
					Log.d("> " + line);
					m_history.Add(line);
					m_historyPosition = m_history.Count - 1;

					try {
						
						m_shouldRun = m_interpreterScript.MainClass.@interpret (line);
					
					} catch (Exception ex) {
					
						Console.Beep();
						Log.x(ex);

					}

					line = "";

				} else {
					
					line += key.KeyChar;
					PrintLine(line);

				}

			} while (m_shouldRun);

		}

		private void ClearLine(string line) {

			string clearLine = new string (' ', line.Length < Console.LargestWindowWidth ? line.Length : Console.LargestWindowWidth);

			int currentRow = Console.LargestWindowHeight - 1;

			Console.SetCursorPosition (0, currentRow);
			Console.Write (clearLine);
			Console.SetCursorPosition (0, currentRow);

		}

		//Re-prints the line at current row position.
		private void PrintLine(string line) {

			int currentRow = Console.LargestWindowHeight - 1;
			Console.SetCursorPosition (0, currentRow);
			Console.Write (line);

		}

		public override void Stop () {

			m_shouldRun = false;

		}
	}
}

