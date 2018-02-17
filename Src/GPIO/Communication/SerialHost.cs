using System;
using Core.Device;
using System.Collections.Generic;
using Core;

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
				Initialize (ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL);

			}

		}

		public override void Stop () {

			if (m_connection?.Ready == true) {

				m_connection.Stop ();

			}

		}

		public override bool Ready { get { return m_connection?.Ready == true; } }

		public dynamic GetValue(byte slaveId, int nodeId) {

			DeviceResponsePackage response = Send (m_packageFactory.GetDevice (slaveId, (byte)nodeId));

			return response.Value;

		}

		public void Set(byte slaveId, int nodeId, int value) {

			DeviceResponsePackage response = Send (m_packageFactory.SetDevice (slaveId, (byte)nodeId, value));

		}

		public byte Create(int nodeId, SerialDeviceType type, byte[] parameters) {

			DeviceRequestPackage request = m_packageFactory.CreateDevice ((byte)nodeId, type, parameters);

			DeviceResponsePackage response = Send (request);

			return response.Id;

		}

		public void Initialize(int host = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {

			ResetSlave ((byte)host);

		}

		public bool IsNodeAvailable(int nodeId) {

			DeviceRequestPackage request = m_packageFactory.IsNodeAvailable ((byte)nodeId);
			DeviceResponsePackage response = Send (request);

			return response.Value == true;
		
		}

		public byte[] GetNodes() {
		
			DeviceRequestPackage request = new DeviceRequestPackage () { Action = (byte)ActionType.GetNodes };
			DeviceResponsePackage response = Send (request);

			return response.Content;

		}

		/// <summary>
		/// Sends the request to host, converts it to a DeviceResponsePackage, checks for errors and returns the package.
		/// </summary>
		/// <param name="request">Request.</param>
		public DeviceResponsePackage Send(DeviceRequestPackage request) {

			if (!Ready) { throw new System.IO.IOException ("Communication not started."); }

			byte[] responseData = m_connection.Send (request.ToBytes ());

			DeviceResponsePackage response = m_packageFactory.ParseResponse (responseData);

			//Log.d ($"Sending package: {(ActionType) request.Action} to node: {request.NodeId}.");
			//Log.d ($"Receiving: {response.Action} to from: {request.NodeId}.");
			if (response.Action == ActionType.Initialization) {

				// The slave needs to be reset. Reset the slave and notify delegate, allowing it to re-create and/or re-send
				ResetSlave (response.NodeId);
				Log.t ($"Resetting slave with id: {response.NodeId}.");

				if (HostDidReset != null) {
					HostDidReset (response.NodeId);
				}

				// Resend the current request
				responseData = m_connection.Send (request.ToBytes ());
				response = m_packageFactory.ParseResponse (responseData);

			} else if (response.IsError) { 

				int info = (response.Content?.Length ?? (int)0) > 1 ? response.Content [ArduinoSerialPackageFactory.POSITION_CONTENT_POSITION_ERROR_INFO] : 0;
				throw new System.IO.IOException ($"Response contained an error for action '{(ActionType) request.Action}': '{response.Value}'. Info: {info}. NodeId: {request.NodeId}.");
			
			} else if (!((ActionType) request.Action == ActionType.Initialization && response.Action == ActionType.InitializationOk) &&
				response.Action != (ActionType) request.Action) {

				// The .InitializationOk is a response to a successfull .Initialization. Other successfull requests should return the requested Action.
				throw new System.IO.IOException ($"Response action missmatch: Expected '{(ActionType) request.Action}'. Got: '{response.Action}'. NodeId: {request.NodeId}.");

			}

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

