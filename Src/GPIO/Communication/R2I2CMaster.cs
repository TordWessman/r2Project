using System;
using Core.Device;
using System.Runtime.InteropServices;
using System.Linq;
using Core;

namespace GPIO
{
	public class R2I2CMaster: DeviceBase, ISerialConnection
	{

		private const string dllPath = "libr2I2C.so";

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern int r2I2C_init (int bus, int address);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern int r2I2C_send(byte[] data, int data_size);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern int r2I2C_receive(int wait);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern byte r2I2C_get_response_size();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern  byte r2I2C_get_response(int position);

		// Delay before starting to read from slave. Usefull if slave response is slow.
		public int ReadDelay = Settings.Consts.I2CReadDelay();

		private static readonly object m_lock = new object();

		/// <summary>
		/// Defined in r2I2C.h
		/// </summary>
		public enum I2CError: int {
		
			BusError = -1,
			WriteError = -2,
			ReadError = -4,
			// Returned if an receive/send operation has been commenced after should_run has been set to false. 
			ShouldNotRun = -8,
			// The receive/send operation was busy.
			Busy = -16,

		}

		private int m_bus;
		private int m_port;

		public R2I2CMaster (string id, int? bus = null, int? port = null): base (id)
		{

			m_bus = bus ?? Settings.Consts.I2CDefaultBus();
			m_port = port ?? Settings.Consts.I2CDefaultPort();

			int status = r2I2C_init (m_bus, m_port);

			if (status < 0) {
			
				throw new System.IO.IOException ($"Unable to open I2C bus {m_bus} and port {m_port}. Error type: {(I2CError)status}.");

			}

		}

		private byte[] Response {
		
			get {
				
				byte[] response = new byte[r2I2C_get_response_size()];

				for (int i = 0; i < r2I2C_get_response_size (); i++) { response [i] = r2I2C_get_response (i); }

				return response;

			}

		}

		public byte [] Send (byte []data) {
		
			lock (m_lock) {

				int status = r2I2C_send(data, data.Length);
				if (status < 0) {

					throw new System.IO.IOException ($"Unable to send to I2C bus {m_bus} and port {m_port}. Error type: {(I2CError)status}.");

				} 

				return Read ();

			}

		}

		public byte [] Read() {
		
			int status = r2I2C_receive (ReadDelay);

			if (status < 0) {

				throw new System.IO.IOException ($"Unable to receive from I2C bus {m_bus} and port {m_port}. Error type: {(I2CError)status}..");

			}


			byte[] b = Response;

			Log.t ($"Got size: {b.Length}, expected: {r2I2C_get_response_size ()}");
			for (int i = 0; i < r2I2C_get_response_size (); i++) {
				byte bbb = Response [i];
				Log.t ($"Got response : {bbb}");
			}
			return Response.Take (r2I2C_get_response_size()).ToArray();

		}

	}

}

