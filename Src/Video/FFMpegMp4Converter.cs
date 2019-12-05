using System;
using R2Core.Common;

namespace R2Core.Video
{
	public class FFMpegMp4Converter : IFileConverter {

		private bool m_isConverting = false;
		System.Diagnostics.Process m_proc;

		public FFMpegMp4Converter() {
			
			m_proc = new System.Diagnostics.Process();
			m_proc.EnableRaisingEvents = false;

			m_proc.StartInfo.FileName = "ffmpeg";
			m_proc.StartInfo.UseShellExecute = false;
			m_proc.StartInfo.RedirectStandardOutput = true;
			m_proc.StartInfo.CreateNoWindow = true;
		}

		public void Convert(string inputFile, string outputFile) {
			
			m_isConverting = true;

			m_proc.StartInfo.Arguments = $"-loglevel fatal -i {inputFile} -c copy {outputFile}"; 

			m_proc.Start();
			System.IO.StreamReader reader = m_proc.StandardOutput;
			string output = reader.ReadToEnd();

			m_proc.WaitForExit();

			m_isConverting = false;

			if (m_proc.ExitCode != 0) {

				throw new ApplicationException($"Conversion for file {inputFile} failed with code: {m_proc.ExitCode}.");

			}

		}

		public bool IsConverting { get { return m_isConverting; } }

	}

}

