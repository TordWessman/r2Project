using System;
using R2Core.Device;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace R2Core.Video
{
	public class RPiCameraServer : DeviceBase
	{
		private const string dllPath = "libr2picam.so";

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_rpi_start();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_rpi_stop();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_rpi_setup();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_rpi_get_framerate();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_rpi_get_width();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_rpi_get_height();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern bool _ext_rpi_get_initiated();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern bool _ext_rpi_get_recording();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_rpi_init(int width, int height, int port);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_rpi_init_extended(int width, int height, int port, int bitrate, int framerate);

		public RPiCameraServer(string id, int width, int height, int port) : base(id) {

			_ext_rpi_init(width, height, port); 

		}

		public override bool Ready { get { return _ext_rpi_get_initiated() && !_ext_rpi_get_recording(); } }
		public bool IsPlaying { get { return _ext_rpi_get_recording(); } }

		public int Width { get { return _ext_rpi_get_width(); } }
		public int Height { get { return _ext_rpi_get_height(); } }
		public int Framerate { get { return _ext_rpi_get_framerate(); } }

		public override void Start() {
			
			int status = _ext_rpi_setup(); 

			if (status != 0) {

				throw new ApplicationException("Unable to start rpi camera {status}");

			}

			Task.Factory.StartNew (() => { _ext_rpi_start(); });
		
		}

		public override void Stop() {

			_ext_rpi_stop();

		}

	}

}

