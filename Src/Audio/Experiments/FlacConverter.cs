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
using System.Threading.Tasks;


namespace R2Core.Audio.ASR
{
	public class FlacConverter
	{
		private const string GST_APP_NAME = @"sh";
		private const string DEFAULT_SCRIPT_NAME = @"../../../../Resources/Scripts/Shell/ConvertToFlac.sh";
		private const string COMMAND_STRING = @" {0} {1} {2}";
		
		System.Diagnostics.Process m_proc;

		private string m_scriptFile;
		
		public FlacConverter(string scriptName = DEFAULT_SCRIPT_NAME) {
			m_proc = new System.Diagnostics.Process();
			
			m_proc.EnableRaisingEvents = false;
			
			if (!System.IO.File.Exists(scriptName)) {
				throw new ArgumentException("Script: " + scriptName + " does not exist!");
			}
			
			m_scriptFile = scriptName;

			m_proc.StartInfo.FileName = GST_APP_NAME;
			m_proc.StartInfo.UseShellExecute = false;
			m_proc.StartInfo.RedirectStandardOutput = false;
			m_proc.StartInfo.CreateNoWindow = true;
			
		}
		
		public void Convert(string inputRawAudioFile,
		                     string outputFlacAudioFile) {
			
			m_proc.StartInfo.Arguments = string.Format(COMMAND_STRING,
			                         					m_scriptFile,
			                                            inputRawAudioFile,
			                                            outputFlacAudioFile);
			
			m_proc.Start();
			m_proc.WaitForExit();
			
			if (m_proc.ExitCode != 0) {
				throw new ApplicationException("Conversion failed: " + m_proc.ExitCode);
			}

		}
	}
}

