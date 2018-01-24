using System;
using Core.Device;
using System.Collections.Generic;

namespace GPIO
{
	public class SerialHost: DeviceBase, ISerialHost
	{
		private ISerialConnection m_connection;
		private ISerialPackageFactory m_packageFactory;

		private Action<byte> m_delegate;

		/// <summary>
		/// Informs that a host has been reinitialized and need to be reconfigured (i.e. add devices). 
		/// </summary>
		public Action<byte> HostDidReset { 
			get { return m_delegate; }
			set { m_delegate = value; }
		}

		public SerialHost (string id, ISerialConnection connection, ISerialPackageFactory packageFactory) : base(id)
		{
			
			m_connection = connection;
			m_packageFactory = packageFactory;

		}

		public override void Start () {

			if (m_connection?.Ready == false) {

				m_connection.Start ();
				Initialize (ArduinoSerialPackageFactory.DEVICE_HOST_LOCAL);

			}

		}

		public override void Stop () {

			if (m_connection?.Ready == true) {

				m_connection.Stop ();

			}

		}

		public override bool Ready { get { return m_connection.Ready; } }

		public dynamic GetValue(byte slaveId, int hostId) {

			DeviceResponsePackage response = Send (m_packageFactory.GetDevice (slaveId, (byte)hostId));

			return response.Value;

		}

		public void Set(byte slaveId, int hostId, int value) {

			DeviceResponsePackage response = Send (m_packageFactory.SetDevice (slaveId, (byte)hostId, value));

		}

		public byte Create(int hostId, SerialDeviceType type, byte[] parameters) {

			DeviceRequestPackage request = m_packageFactory.CreateDevice ((byte)hostId, type, parameters);

			DeviceResponsePackage response = Send (request);

			return response.Id;

		}

		public void Initialize(int host = ArduinoSerialPackageFactory.DEVICE_HOST_LOCAL) {

			ResetSlave ((byte)host);

		}

		/// <summary>
		/// Sends the request to host, converts it to a DeviceResponsePackage, checks for errors and returns the package.
		/// </summary>
		/// <param name="request">Request.</param>
		public DeviceResponsePackage Send(DeviceRequestPackage request) {

			if (!Ready) { throw new System.IO.IOException ("Communication not started."); }

			byte[] responseData = m_connection.Send (request.ToBytes ());

			DeviceResponsePackage response = m_packageFactory.ParseResponse (responseData);//new DeviceResponsePackage (responseData);

			if (response.Action == ActionType.Initialization) {

				// The slave needs to be reset. Reset the slave and notify delegate, allowing it to re-create and/or re-send
				ResetSlave(response.Host); 
				if (HostDidReset != null) { HostDidReset (response.Host); }

				// Resend the current request
				responseData = m_connection.Send (request.ToBytes ());
				response = m_packageFactory.ParseResponse (responseData);

			}

			if (response.IsError) { throw new System.IO.IOException(System.Text.Encoding.Default.GetString(response.Content)); }

			return response;

		}

		/// <summary>
		/// Will send the Initialize request to the host and clear it's data.
		/// </summary>
		/// <param name="host">Host.</param>
		private void ResetSlave(byte host) {

			Send (m_packageFactory.Initialize (host));

		}

	}
}

