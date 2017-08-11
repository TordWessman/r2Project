using System;
using Core.Device;
using System.Runtime.InteropServices;
using System.Linq;

namespace GPIO
{
	public class R2I2CMaster: DeviceBase, II2CMaster
	{

		private const string dllPath = "libr2I2C.so";

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern int r2I2C_init (int bus, int address);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern int r2I2C_send(byte[] data, int data_size);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern int r2I2C_receive(int data_size);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern  byte[] r2I2C_get_response();

		private int m_bus;
		private int m_port;

		public R2I2CMaster (string id, int bus, int port): base (id)
		{

			m_bus = bus;
			m_port = port;

			int status = r2I2C_init (bus, port);

			if (status < 0) {
			
				throw new System.IO.IOException ($"Unable to open I2C bus {bus} and port {port}. Status: {status}.");

			}

		}

		public byte [] Send (byte []data, int responseSize = 0) {
		
			int status = r2I2C_send(data, data.Length);

			if (status < 0) {

				throw new System.IO.IOException ($"Unable to send to I2C bus {m_bus} and port {m_port}. Status: {status}.");

			} else if (responseSize > 0) {
			
				status = r2I2C_receive (responseSize);

				if (status < 0) {

					throw new System.IO.IOException ($"Unable to receive from I2C bus {m_bus} and port {m_port}. Status: {status}.");

				}

			}

			return r2I2C_get_response ().Take (responseSize).ToArray();

		}

		public byte [] Read(int responseSize) {
		
			int status = r2I2C_receive (responseSize);

			if (status < 0) {

				throw new System.IO.IOException ($"Unable to receive from I2C bus {m_bus} and port {m_port}. Status: {status}.");

			}

			return r2I2C_get_response ().Take (responseSize).ToArray();

		}

	}

}

