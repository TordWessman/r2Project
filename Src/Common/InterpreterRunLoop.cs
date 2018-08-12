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
using R2Core.Device;
using R2Core.Scripting;
using System.Collections.Generic;
using System.Linq;

namespace R2Core.Common
{
	
	public class InterpreterRunLoop : DeviceBase, IRunLoop, IDeviceObserver
	{
		// The string that is dislpayed before an executed command.
		private const string COMMAND_DEFINITION = "> ";

		private IScriptInterpreter m_interpreter;
		private bool m_shouldRun;
		private IList<string> m_history;
		private int m_historyPosition;
		private bool m_isRunning;
		private IMessageLogger m_logger;

		/// <summary>
		/// The IScript's MainClass is required to implement a 'bool interpret(string)' method. The return value of this method determines weither the loop should continue or not.
		/// The logger will be used to print output.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="script">Script.</param>
		/// <param name="logger">LOgger.</param>
		public InterpreterRunLoop (string id, IScriptInterpreter interpreter, IMessageLogger logger) : base (id) {
		
			m_interpreter = interpreter;
			m_history = new List<string> ();
			m_logger = logger;

		}

		public override void Start () {

			if (m_isRunning) {
				throw new ApplicationException("Unable to start run loop, since it has started.");
			}

			m_shouldRun = true;
			m_isRunning = true;

			RunLoop ();

			m_isRunning = false;

		}

		public override void Stop () {

			m_shouldRun = false;

		}

		/// <summary>
		/// Will block the current thread until finished
		/// </summary>
		private void RunLoop() {
		
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

					} else if (m_historyPosition == m_history.Count) {

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
					m_logger.Write(new LogMessage(COMMAND_DEFINITION + line,LogType.Message));
					m_history.Add(line);
					m_historyPosition = m_history.Count;

					try {

						InterpretText (line);

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

		/// <summary>
		/// Will interpret the text string through it's interpreter script.
		/// </summary>
		/// <param name="text">Text.</param>
		public void InterpretText(string text) {
			
			m_interpreter.Interpret (text);

		}

		public IEnumerable<ILogMessage> GetHistory(int historyCount) {
		
			return m_logger.History.Reverse().Take (historyCount);
				
		}

		public void OnValueChanged(IDeviceNotification<object> notification) {
		
			if (notification.Type == typeof(LogMessage)) {
			
				OnValueChanged (notification);

			}

		}

		/// <summary>
		/// Clear the line at the current position.
		/// </summary>
		/// <param name="line">Line.</param>
		private void ClearLine(string line) {

			string clearLine = new string (' ', line.Length < Console.LargestWindowWidth ? line.Length : Console.LargestWindowWidth);

			int currentRow = Console.LargestWindowHeight - 1;

			Console.SetCursorPosition (0, currentRow);
			Console.Write (clearLine);
			Console.SetCursorPosition (0, currentRow);

		}

		/// <summary>
		/// Re-prints the line at current row position.
		/// </summary>
		/// <param name="line">Line.</param>
		private void PrintLine(string line) {

			int currentRow = Console.LargestWindowHeight - 1;
			Console.SetCursorPosition (0, currentRow);
			Console.Write (line);

		}

	}

}