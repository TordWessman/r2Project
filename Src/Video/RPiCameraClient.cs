using System;
using System.Runtime.InteropServices;
using R2Core.Device;
using System.Threading.Tasks;
using R2Core.Common;

namespace R2Core.Video
{
	public class RPiCameraClient : DeviceBase {

		private const string dllPath = "libr2picamrecorder.so";

		protected delegate void FileRecordedCallback(string filename);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_rpir_record(string filename, FileRecordedCallback done);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_rpir_stop();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern bool _ext_rpir_get_recording();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern string _ext_rpir_get_filename();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_rpir_init(int port, string address);

		private Task m_recordTask;
		private string m_path;
		private IFileConverter m_converter;

		public RPiCameraClient(string id, string path, IFileConverter converter) : base (id) {

			m_path = path;
			m_converter = converter;
		
		}

		public void Setup(int port, string address) {
		
			_ext_rpir_init(port, address);

		}

		protected void RecordingFinished(string filename) {

			string outputFilename = System.IO.Path.Combine(m_path, filename + ".mp4");
			m_converter.Convert(filename, outputFilename);
			System.IO.File.Delete(filename);

		}

		public override void Stop() {
			
			_ext_rpir_stop();
		
		}

		public override void Start () {

			string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + ".h264";
			Log.d($"Recording video stream to: '{filename}'.");
			Record(filename);

		}

		public void Record(string filename) {

			if (!Ready) {
			
				throw new ApplicationException($"Unable to record {filename}. {this},{Identifier} is not Ready.");
			
			}

			m_recordTask = new Task (() => {

				if (_ext_rpir_record(filename, new FileRecordedCallback(this.RecordingFinished)) != 0) {
					
					Log.e($"Recording failed for file: '{_ext_rpir_get_filename()}'.");

				}

			});

			m_recordTask.Start();

		}

		public override bool Ready { get { return !_ext_rpir_get_recording() && !m_converter.IsConverting; } }

	}

}